namespace MataDewa4
{
    partial class AdditionalButton
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdditionalButton));
            this.Additional_btnArming = new System.Windows.Forms.Button();
            this.Additional_btnTakeOFf = new System.Windows.Forms.Button();
            this.Additional_btnParachute = new System.Windows.Forms.Button();
            this.Additional_btnDisArming = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Additional_btnArming
            // 
            this.Additional_btnArming.Image = ((System.Drawing.Image)(resources.GetObject("Additional_btnArming.Image")));
            this.Additional_btnArming.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Additional_btnArming.Location = new System.Drawing.Point(18, 9);
            this.Additional_btnArming.Name = "Additional_btnArming";
            this.Additional_btnArming.Size = new System.Drawing.Size(100, 28);
            this.Additional_btnArming.TabIndex = 0;
            this.Additional_btnArming.Text = "ARMING";
            this.Additional_btnArming.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Additional_btnArming.UseVisualStyleBackColor = true;
            this.Additional_btnArming.Click += new System.EventHandler(this.Additional_btnArming_Click);
            // 
            // Additional_btnTakeOFf
            // 
            this.Additional_btnTakeOFf.Image = ((System.Drawing.Image)(resources.GetObject("Additional_btnTakeOFf.Image")));
            this.Additional_btnTakeOFf.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Additional_btnTakeOFf.Location = new System.Drawing.Point(18, 39);
            this.Additional_btnTakeOFf.Name = "Additional_btnTakeOFf";
            this.Additional_btnTakeOFf.Size = new System.Drawing.Size(100, 28);
            this.Additional_btnTakeOFf.TabIndex = 1;
            this.Additional_btnTakeOFf.Text = "TAKE OFF";
            this.Additional_btnTakeOFf.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Additional_btnTakeOFf.UseVisualStyleBackColor = true;
            this.Additional_btnTakeOFf.Click += new System.EventHandler(this.Additional_btnTakeOFf_Click);
            // 
            // Additional_btnParachute
            // 
            this.Additional_btnParachute.Image = ((System.Drawing.Image)(resources.GetObject("Additional_btnParachute.Image")));
            this.Additional_btnParachute.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Additional_btnParachute.Location = new System.Drawing.Point(18, 69);
            this.Additional_btnParachute.Name = "Additional_btnParachute";
            this.Additional_btnParachute.Size = new System.Drawing.Size(100, 28);
            this.Additional_btnParachute.TabIndex = 2;
            this.Additional_btnParachute.Text = "PARACHUTE";
            this.Additional_btnParachute.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Additional_btnParachute.UseVisualStyleBackColor = true;
            this.Additional_btnParachute.Click += new System.EventHandler(this.Additional_btnParachute_Click);
            // 
            // Additional_btnDisArming
            // 
            this.Additional_btnDisArming.Image = ((System.Drawing.Image)(resources.GetObject("Additional_btnDisArming.Image")));
            this.Additional_btnDisArming.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Additional_btnDisArming.Location = new System.Drawing.Point(18, 99);
            this.Additional_btnDisArming.Name = "Additional_btnDisArming";
            this.Additional_btnDisArming.Size = new System.Drawing.Size(100, 28);
            this.Additional_btnDisArming.TabIndex = 3;
            this.Additional_btnDisArming.Text = "DIS-ARMING";
            this.Additional_btnDisArming.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Additional_btnDisArming.UseVisualStyleBackColor = true;
            this.Additional_btnDisArming.Click += new System.EventHandler(this.Additional_btnDisArming_Click);
            // 
            // AdditionalButton
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(132, 131);
            this.ControlBox = false;
            this.Controls.Add(this.Additional_btnDisArming);
            this.Controls.Add(this.Additional_btnParachute);
            this.Controls.Add(this.Additional_btnTakeOFf);
            this.Controls.Add(this.Additional_btnArming);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdditionalButton";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Launch Control";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Additional_btnArming;
        private System.Windows.Forms.Button Additional_btnTakeOFf;
        private System.Windows.Forms.Button Additional_btnParachute;
        private System.Windows.Forms.Button Additional_btnDisArming;
    }
}