using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ICPClientLinq
{
    public partial class LimitInsertForm : Form
    {
        tblLimit retVal;

        public LimitInsertForm()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Validates the limit insert data.
        /// </summary>
        /// <returns></returns>
        private bool validateLimitInsert(List<decimal> insertValues)
        {
            decimal reportLimit = 0.00M;
            decimal mdl = 0.00M;
            decimal diff = 0.00M;

            bool retVal = true;
            StringBuilder errMsg = new StringBuilder("The new record can not be inserted due to the following errors: \n\n");

            if (txtElement.Text.Length == 0 || txtElement.Text.Length > 2)
            {
                retVal = false;
                errMsg.AppendLine("\tInvalid Element Value:  Must be less than or equal to 2 characters.");
            }
            else
            {
                
                ICPClientDataContext ctx = new ICPClientDataContext();
                var existingLimits = from lim in ctx.tblLimits
                                             where lim.Element == txtElement.Text
                                             select lim;

                if (existingLimits != null && existingLimits.Count() > 0)
                {
                    retVal = false;
                    errMsg.AppendLine("\tInvalid Element Value:  Element already exists in database.");
                }
                ctx.Dispose();
            }


            if (txtReportLimit.Text.Length == 0 || !Decimal.TryParse(txtReportLimit.Text, out reportLimit))
            {
                retVal = false;
                errMsg.AppendLine("\tInvalid Reporting Limit Value:  Can not be blank and must be a decimal number.");
            }

            if (txtMDL.Text.Length == 0 || !Decimal.TryParse(txtMDL.Text, out mdl))
            {
                retVal = false;
                errMsg.AppendLine("\tInvalid MDL Value:  Can not be blank and must be a decimal number.");
            }

            if (txtDvsTDiff.Text.Length == 0 || !Decimal.TryParse(txtDvsTDiff.Text, out diff))
            {
                retVal = false;
                errMsg.AppendLine("\tInvalid Difference Value:  Can not be blank and must be a decimal number.");
            }

            if (!retVal)
            {
                MessageBox.Show(errMsg.ToString(), "Insert Errors", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                insertValues.Add(reportLimit);
                insertValues.Add(mdl);
                insertValues.Add(diff);
            }

            return retVal;
        }


        /// <summary>
        /// Handles the Click event of the btnInsert control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnInsert_Click(object sender, EventArgs e)
        {
            List<decimal> insertValues = new List<decimal>();
            if (validateLimitInsert(insertValues))
            {
                retVal = new tblLimit();
                retVal.Element = txtElement.Text;
                retVal.Reporting = insertValues[0];
                retVal.MDL = insertValues[1];
                retVal.DvsTDifference = insertValues[2];

                this.Tag = retVal;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                return;
            }

        }

    }
}
