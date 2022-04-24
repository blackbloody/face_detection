
using System;

namespace face_recognition
{
    partial class MainScreen
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
            this.cv_box = new System.Windows.Forms.PictureBox();
            this.list_view_img = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize)(this.cv_box)).BeginInit();
            this.SuspendLayout();
            // 
            // cv_box
            // 
            this.cv_box.Location = new System.Drawing.Point(12, 12);
            this.cv_box.Name = "cv_box";
            this.cv_box.Size = new System.Drawing.Size(953, 630);
            this.cv_box.TabIndex = 0;
            this.cv_box.TabStop = false;
            // 
            // list_view_img
            // 
            this.list_view_img.HideSelection = false;
            this.list_view_img.Location = new System.Drawing.Point(971, 12);
            this.list_view_img.Name = "list_view_img";
            this.list_view_img.Size = new System.Drawing.Size(220, 630);
            this.list_view_img.TabIndex = 1;
            this.list_view_img.UseCompatibleStateImageBehavior = false;
            // 
            // MainScreen
            // 
            this.ClientSize = new System.Drawing.Size(1203, 654);
            this.Controls.Add(this.list_view_img);
            this.Controls.Add(this.cv_box);
            this.Name = "MainScreen";
            this.Load += new System.EventHandler(this.MainScreen_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cv_box)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.PictureBox cv_box;
        private System.Windows.Forms.ListView list_view_img;
    }
}