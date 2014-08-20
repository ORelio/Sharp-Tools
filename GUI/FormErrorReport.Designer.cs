// By ORelio - (c) 2014 - Available under the CDDL-1.0 license

namespace SharpTools
{
    partial class FormErrorReport
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
            this.label_Message = new System.Windows.Forms.Label();
            this.Box_ErrorReport = new System.Windows.Forms.RichTextBox();
            this.button_Copy = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_Message
            // 
            this.label_Message.AutoSize = true;
            this.label_Message.BackColor = System.Drawing.Color.Transparent;
            this.label_Message.Location = new System.Drawing.Point(13, 13);
            this.label_Message.Name = "label_Message";
            this.label_Message.Size = new System.Drawing.Size(16, 13);
            this.label_Message.TabIndex = 0;
            this.label_Message.Text = "...";
            // 
            // Box_ErrorReport
            // 
            this.Box_ErrorReport.Location = new System.Drawing.Point(12, 39);
            this.Box_ErrorReport.Name = "Box_ErrorReport";
            this.Box_ErrorReport.Size = new System.Drawing.Size(520, 250);
            this.Box_ErrorReport.TabIndex = 1;
            this.Box_ErrorReport.Text = "";
            // 
            // button_Copy
            // 
            this.button_Copy.Location = new System.Drawing.Point(197, 302);
            this.button_Copy.Name = "button_Copy";
            this.button_Copy.Size = new System.Drawing.Size(75, 23);
            this.button_Copy.TabIndex = 2;
            this.button_Copy.Text = "Copy";
            this.button_Copy.UseVisualStyleBackColor = true;
            this.button_Copy.Click += new System.EventHandler(this.button_Copy_Click);
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(278, 302);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "OK";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // FormErrorReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 340);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Copy);
            this.Controls.Add(this.Box_ErrorReport);
            this.Controls.Add(this.label_Message);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormErrorReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Error Report";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label_Message;
        private System.Windows.Forms.RichTextBox Box_ErrorReport;
        private System.Windows.Forms.Button button_Copy;
        private System.Windows.Forms.Button button_OK;
    }
}