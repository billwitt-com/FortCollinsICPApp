namespace ICPClientLinq
{
    partial class DetailUpdateForm
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSample = new System.Windows.Forms.Label();
            this.lblBarCode = new System.Windows.Forms.Label();
            this.lblDuplicate = new System.Windows.Forms.Label();
            this.lblSampleValue = new System.Windows.Forms.Label();
            this.lblBarCodeValue = new System.Windows.Forms.Label();
            this.lblDuplicateValue = new System.Windows.Forms.Label();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tlpDetails = new System.Windows.Forms.TableLayoutPanel();
            this.lblFail = new System.Windows.Forms.Label();
            this.lblCheckFlag = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblAnaDate = new System.Windows.Forms.Label();
            this.lblAnaTime = new System.Windows.Forms.Label();
            this.lblElem = new System.Windows.Forms.Label();
            this.lblWavelength = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.lblDelelte = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(84, 20);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(222, 17);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Element Results Maintenance";
            // 
            // lblSample
            // 
            this.lblSample.AutoSize = true;
            this.lblSample.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSample.Location = new System.Drawing.Point(22, 67);
            this.lblSample.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSample.Name = "lblSample";
            this.lblSample.Size = new System.Drawing.Size(52, 13);
            this.lblSample.TabIndex = 1;
            this.lblSample.Text = "Sample:";
            // 
            // lblBarCode
            // 
            this.lblBarCode.AutoSize = true;
            this.lblBarCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBarCode.Location = new System.Drawing.Point(97, 67);
            this.lblBarCode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblBarCode.Name = "lblBarCode";
            this.lblBarCode.Size = new System.Drawing.Size(67, 13);
            this.lblBarCode.TabIndex = 2;
            this.lblBarCode.Text = "Bar Code: ";
            // 
            // lblDuplicate
            // 
            this.lblDuplicate.AutoSize = true;
            this.lblDuplicate.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDuplicate.Location = new System.Drawing.Point(186, 67);
            this.lblDuplicate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDuplicate.Name = "lblDuplicate";
            this.lblDuplicate.Size = new System.Drawing.Size(65, 13);
            this.lblDuplicate.TabIndex = 3;
            this.lblDuplicate.Text = "Duplicate:";
            // 
            // lblSampleValue
            // 
            this.lblSampleValue.AutoSize = true;
            this.lblSampleValue.Location = new System.Drawing.Point(22, 115);
            this.lblSampleValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblSampleValue.Name = "lblSampleValue";
            this.lblSampleValue.Size = new System.Drawing.Size(0, 13);
            this.lblSampleValue.TabIndex = 5;
            // 
            // lblBarCodeValue
            // 
            this.lblBarCodeValue.AutoSize = true;
            this.lblBarCodeValue.Location = new System.Drawing.Point(97, 115);
            this.lblBarCodeValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblBarCodeValue.Name = "lblBarCodeValue";
            this.lblBarCodeValue.Size = new System.Drawing.Size(0, 13);
            this.lblBarCodeValue.TabIndex = 6;
            // 
            // lblDuplicateValue
            // 
            this.lblDuplicateValue.AutoSize = true;
            this.lblDuplicateValue.Location = new System.Drawing.Point(186, 115);
            this.lblDuplicateValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDuplicateValue.Name = "lblDuplicateValue";
            this.lblDuplicateValue.Size = new System.Drawing.Size(0, 13);
            this.lblDuplicateValue.TabIndex = 7;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(51, 694);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(88, 26);
            this.btnUpdate.TabIndex = 8;
            this.btnUpdate.Text = "Update Results";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(214, 694);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(82, 26);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel Update";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tlpDetails
            // 
            this.tlpDetails.ColumnCount = 7;
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1F));
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tlpDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tlpDetails.Location = new System.Drawing.Point(25, 182);
            this.tlpDetails.Margin = new System.Windows.Forms.Padding(2);
            this.tlpDetails.Name = "tlpDetails";
            this.tlpDetails.RowCount = 22;
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpDetails.Size = new System.Drawing.Size(320, 496);
            this.tlpDetails.TabIndex = 10;
            // 
            // lblFail
            // 
            this.lblFail.AutoSize = true;
            this.lblFail.Location = new System.Drawing.Point(272, 167);
            this.lblFail.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFail.Name = "lblFail";
            this.lblFail.Size = new System.Drawing.Size(13, 13);
            this.lblFail.TabIndex = 11;
            this.lblFail.Text = "F";
            // 
            // lblCheckFlag
            // 
            this.lblCheckFlag.AutoSize = true;
            this.lblCheckFlag.Location = new System.Drawing.Point(296, 167);
            this.lblCheckFlag.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCheckFlag.Name = "lblCheckFlag";
            this.lblCheckFlag.Size = new System.Drawing.Size(14, 13);
            this.lblCheckFlag.TabIndex = 12;
            this.lblCheckFlag.Text = "C";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(274, 67);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Ana Date:";
            // 
            // lblAnaDate
            // 
            this.lblAnaDate.AutoSize = true;
            this.lblAnaDate.Location = new System.Drawing.Point(278, 115);
            this.lblAnaDate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAnaDate.Name = "lblAnaDate";
            this.lblAnaDate.Size = new System.Drawing.Size(0, 13);
            this.lblAnaDate.TabIndex = 14;
            // 
            // lblAnaTime
            // 
            this.lblAnaTime.AutoSize = true;
            this.lblAnaTime.Location = new System.Drawing.Point(278, 134);
            this.lblAnaTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAnaTime.Name = "lblAnaTime";
            this.lblAnaTime.Size = new System.Drawing.Size(0, 13);
            this.lblAnaTime.TabIndex = 15;
            // 
            // lblElem
            // 
            this.lblElem.AutoSize = true;
            this.lblElem.Location = new System.Drawing.Point(35, 167);
            this.lblElem.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblElem.Name = "lblElem";
            this.lblElem.Size = new System.Drawing.Size(30, 13);
            this.lblElem.TabIndex = 16;
            this.lblElem.Text = "Elem";
            // 
            // lblWavelength
            // 
            this.lblWavelength.AutoSize = true;
            this.lblWavelength.Location = new System.Drawing.Point(72, 167);
            this.lblWavelength.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblWavelength.Name = "lblWavelength";
            this.lblWavelength.Size = new System.Drawing.Size(65, 13);
            this.lblWavelength.TabIndex = 17;
            this.lblWavelength.Text = "Wavelength";
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(192, 167);
            this.lblResult.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(37, 13);
            this.lblResult.TabIndex = 18;
            this.lblResult.Text = "Result";
            // 
            // lblDelelte
            // 
            this.lblDelelte.AutoSize = true;
            this.lblDelelte.Location = new System.Drawing.Point(326, 167);
            this.lblDelelte.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblDelelte.Name = "lblDelelte";
            this.lblDelelte.Size = new System.Drawing.Size(15, 13);
            this.lblDelelte.TabIndex = 19;
            this.lblDelelte.Text = "D";
            // 
            // DetailUpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 741);
            this.Controls.Add(this.lblDelelte);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblWavelength);
            this.Controls.Add(this.lblElem);
            this.Controls.Add(this.lblAnaTime);
            this.Controls.Add(this.lblAnaDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblCheckFlag);
            this.Controls.Add(this.lblFail);
            this.Controls.Add(this.tlpDetails);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.lblDuplicateValue);
            this.Controls.Add(this.lblBarCodeValue);
            this.Controls.Add(this.lblSampleValue);
            this.Controls.Add(this.lblDuplicate);
            this.Controls.Add(this.lblBarCode);
            this.Controls.Add(this.lblSample);
            this.Controls.Add(this.lblTitle);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DetailUpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Element Detail Update Form";
            this.Load += new System.EventHandler(this.DetailUpdateForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSample;
        private System.Windows.Forms.Label lblBarCode;
        private System.Windows.Forms.Label lblDuplicate;
        private System.Windows.Forms.Label lblSampleValue;
        private System.Windows.Forms.Label lblBarCodeValue;
        private System.Windows.Forms.Label lblDuplicateValue;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TableLayoutPanel tlpDetails;
        private System.Windows.Forms.Label lblFail;
        private System.Windows.Forms.Label lblCheckFlag;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblAnaDate;
        private System.Windows.Forms.Label lblAnaTime;
        private System.Windows.Forms.Label lblElem;
        private System.Windows.Forms.Label lblWavelength;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Label lblDelelte;
    }
}