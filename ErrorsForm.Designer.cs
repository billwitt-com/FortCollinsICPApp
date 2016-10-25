namespace ICPClientLinq
{
    partial class formError
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
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.btnCloseErrorForm = new System.Windows.Forms.Button();
            this.tbErrors = new System.Windows.Forms.TextBox();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.SuspendLayout();
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // btnCloseErrorForm
            // 
            this.btnCloseErrorForm.Location = new System.Drawing.Point(182, 314);
            this.btnCloseErrorForm.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCloseErrorForm.Name = "btnCloseErrorForm";
            this.btnCloseErrorForm.Size = new System.Drawing.Size(112, 28);
            this.btnCloseErrorForm.TabIndex = 0;
            this.btnCloseErrorForm.Text = "Close";
            this.btnCloseErrorForm.UseVisualStyleBackColor = true;
            this.btnCloseErrorForm.Click += new System.EventHandler(this.btnCloseErrorForm_Click);
            // 
            // tbErrors
            // 
            this.tbErrors.Location = new System.Drawing.Point(41, 37);
            this.tbErrors.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tbErrors.Multiline = true;
            this.tbErrors.Name = "tbErrors";
            this.tbErrors.Size = new System.Drawing.Size(419, 256);
            this.tbErrors.TabIndex = 2;
            // 
            // formError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 379);
            this.Controls.Add(this.tbErrors);
            this.Controls.Add(this.btnCloseErrorForm);
            this.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "formError";
            this.Text = "ICP Error Report";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.Button btnCloseErrorForm;
        private System.Windows.Forms.TextBox tbErrors;
        private System.Drawing.Printing.PrintDocument printDocument1;
    }
}