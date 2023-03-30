using OpenTK;

namespace XLightStudio
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.glControl = new OpenTK.GLControl();
            this.ExportButton = new System.Windows.Forms.Button();
            this.modelsListBox = new System.Windows.Forms.ListBox();
            this.BakingProgressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // glControl
            // 
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.Location = new System.Drawing.Point(12, 12);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(596, 393);
            this.glControl.TabIndex = 1;
            this.glControl.VSync = false;
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(12, 411);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(172, 27);
            this.ExportButton.TabIndex = 0;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            // 
            // modelsListBox
            // 
            this.modelsListBox.FormattingEnabled = true;
            this.modelsListBox.Location = new System.Drawing.Point(614, 12);
            this.modelsListBox.Name = "modelsListBox";
            this.modelsListBox.Size = new System.Drawing.Size(174, 394);
            this.modelsListBox.TabIndex = 2;
            // 
            // BakingProgressBar
            // 
            this.BakingProgressBar.Location = new System.Drawing.Point(190, 411);
            this.BakingProgressBar.Name = "BakingProgressBar";
            this.BakingProgressBar.Size = new System.Drawing.Size(598, 27);
            this.BakingProgressBar.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.BakingProgressBar);
            this.Controls.Add(this.modelsListBox);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.glControl);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ProgressBar BakingProgressBar;
        private System.Windows.Forms.Button ExportButton;
        private OpenTK.GLControl glControl;
        private System.Windows.Forms.ListBox modelsListBox;

        #endregion
    }
}