using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Consoles.Nintendo.NDS;

namespace FindLuigi;

[ExternalTool("FindLuigi")]
public sealed partial class FindLuigiTool : ToolFormBase, IExternalToolForm
{
    private const int TappingInterval = 32;

    protected override string WindowTitleStatic => "Find Luigi";
    [OptionalApi] public IJoypadApi? JoypadApi { get; set; }
    [OptionalApi] public IGuiApi? GuiApi { get; set; }
    [OptionalService] private IEmulator? Emulator { get; set; }

    private readonly Bitmap? LookupBitmap;
    private int _lastLookingFor = -1;

    private readonly string[] _names = new[] { "Nobody", "Wario", "Yoshi", "Mario", "Luigi" };

    private int lastX = -1, lastY = -1;
    private int unmoovedFor = 0;
    private int foundFor = 0;

    public FindLuigiTool()
    {
        InitializeComponent();

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("FindLuigi.lookup.png");
        if (stream == null)
        {
            lookingFor.Text = @"Failed to load bitmap: " + assembly.GetManifestResourceNames()[1];

            return;
        }

        using var tempBitmap = new Bitmap(stream); // Is this needed?
        LookupBitmap = new Bitmap(tempBitmap);
    }

    private int _tappedLastFrame;

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (JoypadApi == null) return;
        JoypadApi.SetAnalog("Touch Y");
        JoypadApi.SetAnalog("Touch X");
        JoypadApi.Set("Touch");
    }


    protected override void UpdateAfter()
    {
        if (!enabled.Checked) return;

        GuiApi?.WithSurface(
            DisplaySurfaceID.Client,
            () =>
            {
                GuiApi.ClearGraphics();
                if (lastY > 0 && lastX > 0)
                {
                    GuiApi.DrawBox(lastX - 20, lastY - 20, lastX + 20, lastY + 20, Color.Aqua);
                }
            }
        );

        if (Emulator is not NDS nds)
        {
            lookingFor.Text = $@"Expected NDS, but Emulator was {Emulator?.GetType().Name ?? "null"}";
            return;
        }

        if (JoypadApi == null || LookupBitmap == null) return;

        if (_tappedLastFrame > 0)
        {
            _tappedLastFrame--;
            switch (_tappedLastFrame)
            {
                case > TappingInterval / 2:
                    JoypadApi.SetAnalog("Touch X", lastX);
                    JoypadApi.SetAnalog("Touch Y", lastY - 192);
                    JoypadApi.Set("Touch", true);
                    break;
                default:
                    JoypadApi.Set("Touch", false);
                    break;
            }

            return;
        }

        var videoBuffer = nds.GetVideoBuffer();
        var width = nds.BufferWidth;
        var height = nds.BufferHeight;

        if (width != 256 || height != 192 * 2)
        {
            lookingFor.Text = $@"Screenshot had size {width}, {height}";
            lastX = -1;
            return;
        }

        var facePixel = videoBuffer[(height / 4 - 16) * width + width / 2] & 0x00FFFFFF;

        var lookFor = facePixel switch
        {
            0xB26171 => 1,
            0x30A230 => 2,
            0x410000 => 3,
            0xFBAA82 => 4,
            _ => 0
        };


        if (lookFor != _lastLookingFor)
        {
            _lastLookingFor = lookFor;
            lookingFor.Text = $@"Looking For: {_names[lookFor]}";
            lastX = -1;
        }

        if (lookFor == 0) return;

        var lookForColor = (byte)(lookFor * 255 / 4);

        var sumX = 0f;
        var sumY = 0f;
        var totalFound = 0;

        for (var y = 192; y < 192 * 2 - 1; y++)
        {
            for (var x = 0; x < 256 - 1; x++)
            {
                var core = Convert(videoBuffer[y * 256 + x]);
                var right = Convert(videoBuffer[y * 256 + x + 1]);
                var down = Convert(videoBuffer[(y + 1) * 256 + x]);
                var diag = Convert(videoBuffer[(y + 1) * 256 + x + 1]);

                if (core is 0 or >= 64 || right is 0 or >= 64 || down is 0 or >= 64 || diag is 0 or >= 64) continue;

                var lookupX = right * 64 + core;
                var lookupY = diag * 64 + down;
                var pixel = LookupBitmap.GetPixel(lookupX, lookupY);
                if (pixel.R != lookForColor) continue;
                sumX += x;
                sumY += y;
                totalFound++;
            }
        }

        if (totalFound <= 0)
        {
            lastClicked.Text = @$"Character not found";
            lastX = -1;
            unmoovedFor = 0;
            return;
        }

        foundFor++;

        var tapX = (int)(sumX / totalFound);
        var tapY = (int)(sumY / totalFound);

        var deltaX = tapX - lastX;
        var deltaY = tapY - lastY;

        if (deltaX == 0 || deltaY == 0)
        {
            unmoovedFor++;
        }
        else
        {
            unmoovedFor = 0;
        }

        lastX = tapX;
        lastY = tapY;
        if (foundFor < 5) return;

        if (unmoovedFor < 30)
        {
            switch (foundFor)
            {
                case < 15 when totalFound <= 300:
                case < 30 when totalFound <= 200:
                case < 45 when totalFound <= 100:
                case >= 45 when totalFound <= 145 - foundFor:
                    lastClicked.Text = @$"Skipped {
                        tapX
                    }, {
                        tapY - 192
                    }: (Visibility: {
                        totalFound
                    }, Found For: {
                        foundFor
                    }, Unmooved For: {
                        unmoovedFor
                    })";
                    return;
            }
        }

        var dist = deltaX == 0 && deltaY == 0 ? 0 : Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        tapX += (int)(deltaX * 5f / dist);
        tapY += (int)(deltaY * 5f / dist);

        lastX = tapX;
        lastY = tapY;
        tapY -= 192;

        lastClicked.Text = @$"Last Touched: {tapX}, {tapY} (Visibility: {totalFound})";
        _tappedLastFrame = TappingInterval;
        JoypadApi.SetAnalog("Touch X", tapX);
        JoypadApi.SetAnalog("Touch Y", tapY);
        JoypadApi.Set("Touch", true);
        unmoovedFor = 0;
        foundFor = 0;
    }

    private int Convert(int value)
    {
        var r = ((value >> 16) & 0xFF) / 15;
        var g = ((value >> 8) & 0xFF) / 63;
        return g * 16 + r;
    }

    ~FindLuigiTool()
    {
        LookupBitmap?.Dispose();
    }
}