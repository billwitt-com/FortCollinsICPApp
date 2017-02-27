namespace ICPClientLinq
{
    partial class LimitInsertForm
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
            this.lblInsertTitle = new System.Windows.Forms.Label();
            this.lblElement = new System.Windows.Forms.Label();
            this.lblReportLimit = new System.Windows.Forms.Label();
            this.lblMDL = new System.Windows.Forms.Label();
            this.lblDvTDiff = new System.Windows.Forms.Label();
            this.txtElement = new System.Windows.Forms.TextBox();
            this.txtReportLimit = new System.Windows.Forms.TextBox();
            this.txtMDL = new System.Windows.Forms.TextBox();
            this.txtDvsTDiff = new System.Windows.Forms.TextBox();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblInsertTitle
            // 
            this.lblInsertTitle.AutoSize = true;
            this.lblInsertTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInsertTitle.Location = new System.Drawing.Point(43, 13);
            this.lblInsertTitle.Name = "lblInsertTitle";
            this.lblInsertTitle.Size = new System.Drawing.Size(153, 20);
            this.lblInsertTitle.TabIndex = 0;
            this.lblInsertTitle.Text = "Insert Limit Page";
            // 
            // lblElement
            // 
            this.lblElement.AutoSize = true;
            this.lblElement.Location = new System.Drawing.Point(88, 65);
            this.lblElement.Name = "lblElement";
            this.lblElement.Size = new System.Drawing.Size(67, 17);
            this.lblElement.TabIndex = 1;
            this.lblElement.Text = "Element: ";
            // 
            // lblReportLimit
            // 
            this.lblReportLimit.AutoSize = true;
            this.lblReportLimit.Location = new System.Drawing.Point(88, 110);
            this.lblReportLimit.Name = "lblReportLimit";
            this.lblReportLimit.Size = new System.Drawing.Size(111, 17);
            this.lblReportLimit.TabIndex = 2;
            this.lblReportLimit.Text = "Reporting Limit: ";
            // 
            // lblMDL
            // 
            this.lblMDL.AutoSize = true;
            this.lblMDL.Location = new System.Drawing.Point(88, 155);
            this.lblMDL.Name = "lblMDL";
            this.lblMDL.Size = new System.Drawing.Size(139, 17);
            this.lblMDL.TabIndex = 3;
            this.lblMDL.Text = "Min. Detection Limit: ";
            // 
            // lblDvTDiff
            // 
            this.lblDvTDiff.AutoSize = true;
            this.lblDvTDiff.Location = new System.Drawing.Point(88, 200);
            this.lblDvTDiff.Name = "lblDvTDiff";
            this.lblDvTDiff.Size = new System.Drawing.Size(182, 17);
            this.lblDvTDiff.TabIndex = 4;
            this.lblDvTDiff.Text = "Dissolved/Total Difference: ";
            // 
            // txtElement
            // 
            this.txtElement.Location = new System.Drawing.Point(308, 60);
            this.txtElement.Name = "txtElement";
            this.txtElement.Size = new System.Drawing.Size(100, 22);
            this.txtElement.TabIndex = 5;
            // 
            // txtReportLimit
            // 
            this.txtReportLimit.Location = new System.Drawing.Point(308, 105);
            this.txtReportLimit.Name = "txtReportLimit";
            this.txtReportLimit.Size = new System.Drawing.Size(100, 22);
            this.txtReportLimit.TabIndex = 6;
            // 
            // txtMDL
            // 
            this.txtMDL.Location = new System.Drawing.Point(308, 150);
            this.txtMDL.Name = "txtMDL";
            this.txtMDL.Size = new System.Drawing.Size(100, 22);
            this.txtMDL.TabIndex = 7;
            // 
            // txtDvsTDiff
            // 
            this.txtDvsTDiff.Location = new System.Drawing.Point(308, 195);
            this.txtDvsTDiff.Name = "txtDvsTDiff";
            this.txtDvsTDiff.Size = new System.Drawing.Size(100, 22);
            this.txtDvsTDiff.TabIndex = 8;
            // 
            // btnInsert
            // 
            this.btnInsert.Location = new System.Drawing.Point(112, 262);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(91, 29);
            this.btnInsert.TabIndex = 9;
            this.btnInsert.Text = "Insert";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(263, 262);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(91, 28);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // LimitInsertForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(459, 329);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.txtDvsTDiff);
            this.Controls.Add(this.txtMDL);
            this.Controls.Add(this.txtReportLimit);
            this.Controls.Add(this.txtElement);
            this.Controls.Add(this.lblDvTDiff);
            this.Controls.Add(this.lblMDL);
            this.Controls.Add(this.lblReportLimit);
            this.Controls.Add(this.lblElement);
            this.Controls.Add(this.lblInsertTitle);
            this.Name = "LimitInsertForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Insert Limits for a new Element";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInsertTitle;
        private System.Windows.Forms.Label lblElement;
        private System.Windows.Forms.Label lblReportLimit;
        private System.Windows.Forms.Label lblMDL;
        private System.Windows.Forms.Label lblDvTDiff;
        private System.Windows.Forms.TextBox txtElement;
        private System.Windows.Forms.TextBox txtReportLimit;
        private System.Windows.Forms.TextBox txtMDL;
        private System.Windows.Forms.TextBox txtDvsTDiff;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnCancel;
    }
}