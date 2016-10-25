using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// line 105 - elements are gathered per barcode
namespace ICPClientLinq
{
    public partial class DetailUpdateForm : Form
    {

        #region Attributes
        private TempICP updateICP;
        private List<int> updateRows = new List<int>();

        #endregion


        public DetailUpdateForm(TempICP icpParam)
        {
            InitializeComponent();
            this.updateICP = icpParam;
        }


        /// <summary>
        /// Handles the Load event of the DetailUpdateForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DetailUpdateForm_Load(object sender, EventArgs e)
        {
            lblBarCodeValue.Text = updateICP.barCode;
            lblDuplicateValue.Text = updateICP.duplicate;
            lblSampleValue.Text = updateICP.code;
            lblAnaDate.Text = updateICP.anaDate.ToShortDateString();
            lblAnaTime.Text = updateICP.anaDate.ToShortTimeString();

            populateDetailTable();

            btnUpdate.Enabled = false;
        }


        /// <summary>
        /// Handles the TextChanged event of the txtAveResult control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void txtAveResult_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                int row = tlpDetails.GetPositionFromControl(tb).Row;
                tb.ForeColor = System.Drawing.Color.Red;
                btnUpdate.Enabled = true;

                if (!updateRows.Contains(row))
                {
                    updateRows.Add(row);
                }
            }
        }


        /// <summary>
        /// Handles the CheckedChanged event of the cbFlag control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void cbFlag_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                int row = tlpDetails.GetPositionFromControl(cb).Row;
                if (row >= 0)
                {
                    btnUpdate.Enabled = true;

                    if (!updateRows.Contains(row))
                    {
                        updateRows.Add(row);
                    }
                }
            }
        }


        /// <summary>
        /// Populates the detail table from the updateICP attribute.
        /// </summary>
        private void populateDetailTable()
        {

            ICPClientDataContext icpClientCtx = new ICPClientDataContext();
            var intermediateICPList = from interICP in icpClientCtx.LocaltblIntermeditateICPs
                                      where interICP.BarCode == updateICP.barCode 
                                      select interICP;

            TevaICPDataContext tevaIcpCtx = new TevaICPDataContext();
            var elementList = from elem in tevaIcpCtx.ElementLines
                              where elem.Sample.Name.Equals(updateICP.barCode)
                              && !elem.ElementSymbol.Equals("Y")
                              select elem;

            int col = 0;
            int row = 0;

            foreach (var element in elementList)
            {
                decimal aveResult = Convert.ToDecimal(element.AverageResult.Value);
                bool failFlag = element.FailFlags.Trim().Length > 0;
                bool checkFlag = element.CheckFlag.HasValue && element.CheckFlag.Value != ' ';
                bool deleteFlag = false;


                // getting compiler errors here with existingElem.checkflag which is a string, 
                // but is being treated here as object. I re-created this data base dbml to add log table
                // this may have caused a newer version of compiler to balk at the use of HasValue and Value
                // for a string 
                //if (intermediateICPList.Count() > 0)
                //{
                //    foreach (var existingElem in intermediateICPList)
                //    {
                //        if (existingElem.Element == element.ElementSymbol &&
                //            existingElem.IndexLine.Value == element.LineIndex)
                //        {
                //            aveResult = existingElem.AverageResult.Value;
                //            failFlag = existingElem.FailFlags.Trim().Length > 0;
                //            checkFlag = existingElem.CheckFlag.HasValue &&
                //                    existingElem.CheckFlag.Value != " ";
                //            deleteFlag = (existingElem.DeleteFlag.HasValue) ?
                //                    existingElem.DeleteFlag.Value : false;
                //            break;
                //        }
                //    }
                //}
                // check if a manual override exists
                // changed the logic here for strings 
                if (intermediateICPList.Count() > 0)
                {
                    foreach (var existingElem in intermediateICPList)
                    {
                        if (existingElem.Element == element.ElementSymbol &&
                            existingElem.IndexLine.Value == element.LineIndex)
                        {
                            aveResult = existingElem.AverageResult.Value;
                            failFlag = existingElem.FailFlags.Trim().Length > 0;
                            checkFlag = (existingElem.CheckFlag.Length > 0) && // use length rather than hasValue
                                    existingElem.CheckFlag != " " ;             // remove the .Value 
                            deleteFlag = (existingElem.DeleteFlag.HasValue)?
                                    existingElem.DeleteFlag.Value : false;
                            break;
                        }
                    }
                }

                Label lblIndex = new Label();
                lblIndex.TextAlign = ContentAlignment.MiddleCenter;
                lblIndex.Anchor = AnchorStyles.None;
                lblIndex.Text = element.LineIndex.ToString();
                tlpDetails.Controls.Add(lblIndex, col, row);
                col++;

                Label lblElement = new Label();
                lblElement.TextAlign = ContentAlignment.MiddleCenter;
                lblElement.Anchor = AnchorStyles.None;
                lblElement.Text = element.ElementSymbol;
                tlpDetails.Controls.Add(lblElement, col, row);
                col++;

                Label lblWavelength = new Label();
                lblWavelength.TextAlign = ContentAlignment.MiddleCenter;
                lblWavelength.Anchor = AnchorStyles.None;
                lblWavelength.Text = element.Wavelength;
                tlpDetails.Controls.Add(lblWavelength, col, row);
                col++;

                TextBox txtAveResult = new TextBox();
                txtAveResult.TextAlign = HorizontalAlignment.Center;
                txtAveResult.Anchor = AnchorStyles.None;
                txtAveResult.Text = aveResult.ToString();
                txtAveResult.TextChanged += new EventHandler(txtAveResult_TextChanged);
                tlpDetails.Controls.Add(txtAveResult, col, row);
                col++;

                CheckBox cbFailFlag = new CheckBox();
                cbFailFlag.Anchor = AnchorStyles.None;
                cbFailFlag.Text = string.Empty;
                cbFailFlag.CheckedChanged += new EventHandler(cbFlag_CheckedChanged);
                cbFailFlag.Checked = failFlag;
                tlpDetails.Controls.Add(cbFailFlag, col, row);
                col++;

                CheckBox cbCheckFlag = new CheckBox();
                cbCheckFlag.Anchor = AnchorStyles.None;
                cbCheckFlag.Text = string.Empty;
                cbCheckFlag.CheckedChanged += new EventHandler(cbFlag_CheckedChanged);
                cbCheckFlag.Checked = checkFlag;
                tlpDetails.Controls.Add(cbCheckFlag, col, row);
                col++;

                CheckBox cbDeleteFlag = new CheckBox();
                cbDeleteFlag.Anchor = AnchorStyles.None;
                cbDeleteFlag.Text = string.Empty;
                cbDeleteFlag.CheckedChanged += new EventHandler(cbFlag_CheckedChanged);
                cbDeleteFlag.Checked = deleteFlag;
                tlpDetails.Controls.Add(cbDeleteFlag, col, row);

                col = 0;
                row++;
            }
        }


        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnUpdate.Enabled = false;
        }


        /// <summary>
        /// Handles the Click event of the btnUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (updateRows.Count > 0)   // redundant check - should not be enabled if no update has occurred
            {
                // validate Results.
                if (validateResults())
                {
                    List<LocaltblIntermeditateICP> interICPList = new List<LocaltblIntermeditateICP>();
                    ICPClientDataContext icpClientCtx = new ICPClientDataContext();
                    int row = 0;

                    while (tlpDetails.GetControlFromPosition(0, row) != null)
                    {
                        LocaltblIntermeditateICP newInterICP = null;

                        Label lbI = tlpDetails.GetControlFromPosition(0, row) as Label;
                        Label lbE = tlpDetails.GetControlFromPosition(1, row) as Label;
                        Label lbW = tlpDetails.GetControlFromPosition(2, row) as Label;
                        TextBox tb = tlpDetails.GetControlFromPosition(3, row) as TextBox;
                        CheckBox cbF = tlpDetails.GetControlFromPosition(4, row) as CheckBox;
                        CheckBox cbC = tlpDetails.GetControlFromPosition(5, row) as CheckBox;
                        CheckBox cbD = tlpDetails.GetControlFromPosition(6, row) as CheckBox;

                        if (updateRows.Contains(row))
                        {

                            var existingInterICP = from exist in icpClientCtx.LocaltblIntermeditateICPs
                                                   where exist.BarCode == updateICP.barCode
                                                   && exist.Element == lbE.Text
                                                   && exist.IndexLine == (Convert.ToInt16(lbI.Text))
                                                   select exist;

                            if (existingInterICP.Count() > 0)
                            {
                                // update existing record
                                newInterICP = existingInterICP.FirstOrDefault();
                                newInterICP.AverageResult = Convert.ToDecimal(tb.Text);
                                newInterICP.CheckFlag = (cbC.Checked) ? "F" : "     ";
                                newInterICP.FailFlags = (cbF.Checked) ? "C" : "     ";
                                newInterICP.DeleteFlag = cbD.Checked;
                            }
                            else
                            {
                                // insert record into database
                                newInterICP = new LocaltblIntermeditateICP();
                                newInterICP.Anadate = updateICP.anaDate;
                                newInterICP.AverageResult = Convert.ToDecimal(tb.Text);
                                newInterICP.BarCode = updateICP.barCode;
                                newInterICP.CheckFlag = (cbC.Checked) ? "F" : "    ";
                                newInterICP.Element = lbE.Text;
                                newInterICP.Wavelength = lbW.Text;
                                newInterICP.FailFlags = (cbF.Checked) ? "C" : "     ";
                                newInterICP.Code = updateICP.code;
                                newInterICP.Duplicate = updateICP.duplicate;
                                newInterICP.IndexLine = Convert.ToInt16(lbI.Text);
                                newInterICP.DeleteFlag = cbD.Checked;

                                icpClientCtx.LocaltblIntermeditateICPs.InsertOnSubmit(newInterICP);
                            }

                            try
                            {
                                icpClientCtx.SubmitChanges();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Database Update Failed: " + newInterICP.Element + "\n\n\t" + ex.Message, "Database Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            newInterICP = new LocaltblIntermeditateICP();
                            newInterICP.Anadate = updateICP.anaDate;
                            newInterICP.AverageResult = Convert.ToDecimal(tb.Text);
                            newInterICP.BarCode = updateICP.barCode;
                            newInterICP.CheckFlag = (cbC.Checked) ? "F" : "   ";
                            newInterICP.Element = lbE.Text;
                            newInterICP.Wavelength = lbW.Text;
                            newInterICP.FailFlags = (cbF.Checked) ? "C" : "     ";
                            newInterICP.Code = updateICP.code;
                            newInterICP.Duplicate = updateICP.duplicate;
                            newInterICP.IndexLine = Convert.ToInt16(lbI.Text);
                            newInterICP.DeleteFlag = cbD.Checked;
                        }

                        interICPList.Add(newInterICP);
                        row++;
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Tag = interICPList;
                }
            }
        }


        /// <summary>
        /// Validates the user entered results.
        /// </summary>
        /// <returns></returns>
        private bool validateResults()
        {
            bool retVal = true;
            StringBuilder errMsg = new StringBuilder("The following errors prevent the update of the barcode record:");

            foreach (int row in updateRows)
            {
                decimal result = 0.00M;
                TextBox tb = tlpDetails.GetControlFromPosition(3, row) as TextBox;
                //Label lbl = tlpDetails.GetControlFromPosition(1, row) as Label;
                if (tb != null)
                {
                    if (tb.ForeColor == System.Drawing.Color.Red)
                    {
                        if (tb.Text.Length == 0 || !Decimal.TryParse(tb.Text, out result))
                        {
                            retVal = false;
                            errMsg.AppendLine("\tInvalid Result Value:  Can not be blank and must be a decimal number.");
                        }
                    }
                }
            }
            if (!retVal)
            {
                MessageBox.Show(errMsg.ToString(), "Update Errors", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            return retVal;

        }
    }
}
