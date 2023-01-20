namespace E17_CPS
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
            this.buttonSaveCPS = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textboxDirectoryImage = new System.Windows.Forms.TextBox();
            this.buttonBrowseImage = new System.Windows.Forms.Button();
            this.listboxFiles = new System.Windows.Forms.ListBox();
            this.buttonBatch = new System.Windows.Forms.Button();
            this.picturebox = new System.Windows.Forms.PictureBox();
            this.buttonBrowseCPS = new System.Windows.Forms.Button();
            this.textboxDirectoryCPS = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picturebox)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonSaveCPS
            // 
            this.buttonSaveCPS.Location = new System.Drawing.Point(431, 46);
            this.buttonSaveCPS.Name = "buttonSaveCPS";
            this.buttonSaveCPS.Size = new System.Drawing.Size(110, 34);
            this.buttonSaveCPS.TabIndex = 1;
            this.buttonSaveCPS.Text = "Convert to CPS";
            this.buttonSaveCPS.UseVisualStyleBackColor = true;
            this.buttonSaveCPS.Click += new System.EventHandler(this.buttonSaveCPS_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Image Directory";
            // 
            // textboxDirectoryImage
            // 
            this.textboxDirectoryImage.Location = new System.Drawing.Point(100, 17);
            this.textboxDirectoryImage.Name = "textboxDirectoryImage";
            this.textboxDirectoryImage.Size = new System.Drawing.Size(219, 20);
            this.textboxDirectoryImage.TabIndex = 3;
            this.textboxDirectoryImage.TextChanged += new System.EventHandler(this.textboxDirectory_TextChanged);
            // 
            // buttonBrowseImage
            // 
            this.buttonBrowseImage.Location = new System.Drawing.Point(325, 9);
            this.buttonBrowseImage.Name = "buttonBrowseImage";
            this.buttonBrowseImage.Size = new System.Drawing.Size(75, 34);
            this.buttonBrowseImage.TabIndex = 4;
            this.buttonBrowseImage.Text = "Browse";
            this.buttonBrowseImage.UseVisualStyleBackColor = true;
            this.buttonBrowseImage.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // listboxFiles
            // 
            this.listboxFiles.FormattingEnabled = true;
            this.listboxFiles.Location = new System.Drawing.Point(16, 86);
            this.listboxFiles.Name = "listboxFiles";
            this.listboxFiles.Size = new System.Drawing.Size(120, 303);
            this.listboxFiles.TabIndex = 5;
            this.listboxFiles.SelectedIndexChanged += new System.EventHandler(this.listboxFiles_SelectedIndexChanged);
            // 
            // buttonBatch
            // 
            this.buttonBatch.Location = new System.Drawing.Point(432, 9);
            this.buttonBatch.Name = "buttonBatch";
            this.buttonBatch.Size = new System.Drawing.Size(110, 34);
            this.buttonBatch.TabIndex = 6;
            this.buttonBatch.Text = "Batch Convert to CPS";
            this.buttonBatch.UseVisualStyleBackColor = true;
            this.buttonBatch.Click += new System.EventHandler(this.buttonBatch_Click);
            // 
            // picturebox
            // 
            this.picturebox.Location = new System.Drawing.Point(142, 86);
            this.picturebox.Name = "picturebox";
            this.picturebox.Size = new System.Drawing.Size(400, 300);
            this.picturebox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picturebox.TabIndex = 7;
            this.picturebox.TabStop = false;
            // 
            // buttonBrowseCPS
            // 
            this.buttonBrowseCPS.Location = new System.Drawing.Point(325, 46);
            this.buttonBrowseCPS.Name = "buttonBrowseCPS";
            this.buttonBrowseCPS.Size = new System.Drawing.Size(75, 34);
            this.buttonBrowseCPS.TabIndex = 10;
            this.buttonBrowseCPS.Text = "Browse";
            this.buttonBrowseCPS.UseVisualStyleBackColor = true;
            this.buttonBrowseCPS.Click += new System.EventHandler(this.buttonBrowseCPS_Click);
            // 
            // textboxDirectoryCPS
            // 
            this.textboxDirectoryCPS.Location = new System.Drawing.Point(100, 54);
            this.textboxDirectoryCPS.Name = "textboxDirectoryCPS";
            this.textboxDirectoryCPS.Size = new System.Drawing.Size(219, 20);
            this.textboxDirectoryCPS.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "CPS Directory";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(553, 396);
            this.Controls.Add(this.buttonBrowseCPS);
            this.Controls.Add(this.textboxDirectoryCPS);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.picturebox);
            this.Controls.Add(this.buttonBatch);
            this.Controls.Add(this.listboxFiles);
            this.Controls.Add(this.buttonBrowseImage);
            this.Controls.Add(this.textboxDirectoryImage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonSaveCPS);
            this.Name = "Form1";
            this.Text = "Ever17 CPS Converter";
            ((System.ComponentModel.ISupportInitialize)(this.picturebox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonSaveCPS;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textboxDirectoryImage;
        private System.Windows.Forms.Button buttonBrowseImage;
        private System.Windows.Forms.ListBox listboxFiles;
        private System.Windows.Forms.Button buttonBatch;
        private System.Windows.Forms.PictureBox picturebox;
        private System.Windows.Forms.Button buttonBrowseCPS;
        private System.Windows.Forms.TextBox textboxDirectoryCPS;
        private System.Windows.Forms.Label label2;
    }
}

