using System.ComponentModel;

namespace FindLuigi;

partial class FindLuigiTool
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.enabled = new System.Windows.Forms.CheckBox();
        this.lookingFor = new System.Windows.Forms.Label();
        this.lastClicked = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // enabled
        // 
        this.enabled.AccessibleDescription = "";
        this.enabled.Location = new System.Drawing.Point(12, 12);
        this.enabled.Name = "enabled";
        this.enabled.Size = new System.Drawing.Size(109, 24);
        this.enabled.TabIndex = 0;
        this.enabled.Text = "Enabled";
        this.enabled.UseVisualStyleBackColor = true;
        this.enabled.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
        // 
        // lookingFor
        // 
        this.lookingFor.Location = new System.Drawing.Point(12, 39);
        this.lookingFor.Name = "lookingFor";
        this.lookingFor.Size = new System.Drawing.Size(389, 18);
        this.lookingFor.TabIndex = 1;
        this.lookingFor.Text = "Looking for: Nobody";
        // 
        // lastClicked
        // 
        this.lastClicked.Location = new System.Drawing.Point(12, 57);
        this.lastClicked.Name = "lastClicked";
        this.lastClicked.Size = new System.Drawing.Size(389, 18);
        this.lastClicked.TabIndex = 2;
        this.lastClicked.Text = "Last Clicked: Nothing";
        // 
        // FindLuigiTool
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.lastClicked);
        this.Controls.Add(this.lookingFor);
        this.Controls.Add(this.enabled);
        this.Name = "FindLuigiTool";
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Label lookingFor;
    private System.Windows.Forms.Label lastClicked;

    private System.Windows.Forms.CheckBox enabled;

    #endregion
}