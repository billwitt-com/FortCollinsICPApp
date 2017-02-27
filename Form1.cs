using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;

// a thought to speed up initial load - bwitt dec 7, 2011
// at about Line 929 the table 'limits' is hit one time for each element - this table should be cached into an object 
// to reduce the number of hits.
// found 'bug' in listing flags where a string with only a space was being reported to the form DetailUpdate 
// forced input from box number into caps so it would work with lower case user entry
// removed dups from localcooler and set BarCode to primary key to help prevent dups
// bwitt Oct 25, 2016 Updating for WEBAPI instead of direct data base access
// 02/18 looking for spread sheet output
// found issues at line 827 where if result value is less than 'limit' it is set to 0.00m - Bad news

namespace ICPClientLinq
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            btnExport.Enabled = false;
        }


        #region Attributes
        List<TempICP> tempIcpList = new List<TempICP>();
        BindingList<TempICP> noResultsIcpList = new BindingList<TempICP>();
        IEnumerable<tblLimit> limitList = new List<tblLimit>();
        BindingList<tblInboundICP> exportList = new BindingList<tblInboundICP>();
        List<LocalCooler> coolerList = new List<LocalCooler>();

        // I think this should come from data base as query, or be moved to config file

        string[] elementTypes = { "Al", "As", "Ca", "Cd", "Cu", "Fe", "K", "Mg", "Mn", "Na", "Pb", "Se", "Zn" };
        string userName = string.Empty;
        bool limitChanged = false;
        TabPage currentPage;
        bool CoolerFilter = false;

        SortedList<string, List<string>> warningList = new SortedList<string, List<string>>(); 

        #endregion


        /// <summary>
        /// Handles the Load event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // write to log file to indicate startup bwitt Dec 7 2011

            userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            userName = userName.Replace('\\', '&'); 
            int idx = userName.IndexOf("&") ;
            int len = userName.Length - idx;
            userName = userName.Substring(idx + 1, len-1);
            lblWelcome.Text = "Welcome " + userName; 

            logErrors("ICPClient Started by " + userName, "Informational", 0);

            currentPage = tabStatus;    // default to the status page
            panel1.Visible = false;            

            ICPClientDataContext ctx = new ICPClientDataContext();  // local db context, not RiverWatch production db
            limitList = from lim in ctx.tblLimits
                        select lim;

            NameValueCollection settings = ConfigurationManager.AppSettings;
            
            // XXXX get new web api credentials
            string WebAPI_UserName = settings["WebAPI_UserName"]; //Properties.Settings.Default[              //.WebAPI_UserName;
            string APIPassword = settings["APIPassword"];

            int startYear = Convert.ToInt32((String)settings["baseYear"]);
            int currentYear = DateTime.Now.Year;

            List<String> yearFilterList = new List<string>();
            yearFilterList.Add(String.Empty);

            for (int indx = currentYear; indx >= startYear; indx--)
            {
                yearFilterList.Add(indx.ToString());
            }

            cbYearFilter.DataSource = yearFilterList;

        }


        /// <summary>
        /// Handles the SelectedIndexChanged event of the tabICP control.
        /// Initializes the various tabs prior to display.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void tabICP_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabICP.SelectedIndex)
            {
                case 0: // tabStatus
                    currentPage = tabStatus;
                    break;

                case 1: //tabLimits
                    currentPage = tabLimits;
                    limitChanged = false;
                    btnSaveLimit.Enabled = false;
                    btnDeleteLimit.Enabled = false;

                    populateLimitTable();
                    break;

                case 2: //tabDetails

                    currentPage = tabDetails;

                    if (tempIcpList.Count() > 0)
                    {
                        // sort the list by anadate desc
                        tempIcpList.Sort(new CompareTempICP("AnaDate"));

                        foreach (Control cntrl in tabDetails.Controls)
                        {
                            cntrl.Visible = true;
                        }

                        // dont build new nodes if they already exist.
                        if (tvBarCodes.Nodes[0].Nodes.Count == 0)
                        {
                            IEnumerable<string> boxNumList =
                                    (from icp in tempIcpList
                                     select icp.boxNumber).Distinct();

                            foreach (var boxNum in boxNumList)
                            {
                                string boxNumber = (string.IsNullOrEmpty(boxNum)) ? "Default" : boxNum;
                                TreeNode tn = new TreeNode(boxNumber);
                                tvBarCodes.Nodes[0].Nodes.Add(tn);
                            }

                            foreach (TempICP tempICP in tempIcpList)
                            {
                                TreeNode icpNode = new TreeNode(tempICP.barCode);
                                icpNode.ForeColor = getNodeColor(tempICP);
                                string boxNum = (String.IsNullOrEmpty(tempICP.boxNumber)) ?
                                    "Default" : tempICP.boxNumber;

                                findBoxNodeFromText(boxNum).Nodes.Add(icpNode);

                            }

                            foreach (TreeNode boxNumNode in tvBarCodes.Nodes[0].Nodes)
                            {
                                boxNumNode.Text = boxNumNode.Text + ": (" + boxNumNode.Nodes.Count + ")";
                            }
                        }

                        tvBarCodes.Nodes[0].Expand();
                        txtBarCodeFilter.Text = String.Empty;
                    }
                    else
                    {
                        foreach (Control cntrl in tabDetails.Controls)
                        {
                            cntrl.Visible = false;
                        }
                        MessageBox.Show("There are no detail records to display.");
                    }
                    break;

                case 3:  // tabRiverwatch

                    currentPage = tabRiverwatch;
                    if (noResultsIcpList.Count() > 0)
                    {
                        dgvNoResults.Visible = true;
                        dgvNoResults.AutoGenerateColumns = false;
                        dgvNoResults.DataSource = noResultsIcpList;

                        lblNoResults.Visible = false;
                    }
                    else
                    {
                        dgvNoResults.Visible = false;
                        lblNoResults.Visible = true;
                    }

                    lblVerifyResults1.Text = String.Empty;
                    lblVerifyResults2.Text = String.Empty;
                    txtBarcodeToCheck.Text = String.Empty;

                    break;

                case 4:   // tabExport
                    currentPage = tabExport;

                    if (exportList.Count() > 0)
                    {
                        foreach (Control cntrl in tabExport.Controls)
                        {
                            cntrl.Visible = true;
                        }
                    }
                    else
                    {
                        foreach (Control cntrl in tabExport.Controls)
                        {
                            cntrl.Visible = false;
                        }
                        MessageBox.Show("There are no export records to display.");
                    }
                    break;

            }
        }


        /// <summary>
        /// Handles the Selecting event of the tabICP control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.TabControlCancelEventArgs"/> instance containing the event data.</param>
        private void tabICP_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (currentPage == tabLimits && limitChanged)
            {
                DialogResult inputFromUser = MessageBox.Show("Do you want to save the changes made to the Limit values?",
                        "Save Changes?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (inputFromUser == DialogResult.Yes)
                {
                    btnSaveLimit.PerformClick();
                }
            }
        }


        #region Limits Tab
        /// <summary>
        /// Dynamically creates the table in the limits tab from data in the Limits table.
        /// </summary>
        private void populateLimitTable()
        {
            clearLimitTable();

            ICPClientDataContext ctx = new ICPClientDataContext();
            limitList = from lim in ctx.tblLimits
                        select lim;

            int col = 0;
            int row = 1;

            foreach (var limit in limitList)
            {
                Label lblElement = new Label();
                lblElement.TextAlign = ContentAlignment.MiddleCenter;
                lblElement.Anchor = AnchorStyles.None;
                lblElement.Text = limit.Element;
                tlpLimits.Controls.Add(lblElement, col, row);
                col++;

                TextBox txtReportingLimit = new TextBox();
                txtReportingLimit.TextAlign = HorizontalAlignment.Center;
                txtReportingLimit.Anchor = AnchorStyles.None;
                txtReportingLimit.Text = limit.Reporting.ToString();
                txtReportingLimit.TextChanged += new EventHandler(txtLimitValue_TextChanged);
                tlpLimits.Controls.Add(txtReportingLimit, col, row);
                col++;

                TextBox txtMDL = new TextBox();
                txtMDL.TextAlign = HorizontalAlignment.Center;
                txtMDL.Anchor = AnchorStyles.None;
                txtMDL.Text = limit.MDL.ToString();
                txtMDL.TextChanged += new EventHandler(txtLimitValue_TextChanged);
                tlpLimits.Controls.Add(txtMDL, col, row);
                col++;

                TextBox txtDiff = new TextBox();
                txtDiff.TextAlign = HorizontalAlignment.Center;
                txtDiff.Anchor = AnchorStyles.None;
                txtDiff.Text = limit.DvsTDifference.ToString();
                txtDiff.TextChanged += new EventHandler(txtLimitValue_TextChanged);
                tlpLimits.Controls.Add(txtDiff, col, row);
                col++;

                CheckBox cbSelected = new CheckBox();
                cbSelected.Anchor = AnchorStyles.None;
                cbSelected.Text = string.Empty;
                cbSelected.CheckedChanged += new EventHandler(cbSelected_CheckedChanged);
                tlpLimits.Controls.Add(cbSelected, col, row);

                col = 0;
                row++;
            }

            col = 0;
            row++;
        }


        /// <summary>
        /// Handles the Click event of the btnInsertLimit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnInsertLimit_Click(object sender, EventArgs e)
        {
            Form limitInsert = new LimitInsertForm();
            DialogResult button = limitInsert.ShowDialog();
            if (button == DialogResult.OK)
            {
                tblLimit retVal = limitInsert.Tag as tblLimit;
                if (retVal != null)
                {
                    ICPClientDataContext ctx = new ICPClientDataContext();
                    ctx.tblLimits.InsertOnSubmit(retVal);

                    try
                    {
                        ctx.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Database Insert Failed: \n\n\t" + ex.Message, "Database Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    btnSaveLimit.Enabled = false;
                    populateLimitTable();
                }
                else
                {
                    MessageBox.Show("Limit Insert Failed", "Insert Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }


        /// <summary>
        /// Handles the Click event of the btnCancelLimit control.
        /// Removes any insert text boxes and sets change flag to false.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnCancelLimit_Click(object sender, EventArgs e)
        {
            limitChanged = false;
            btnSaveLimit.Enabled = false;
            btnDeleteLimit.Enabled = false;
            populateLimitTable();
        }


        /// <summary>
        /// Handles the Click event of the btnSaveLimit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnSaveLimit_Click(object sender, EventArgs e)
        {
            List<string> changedElems = new List<string>();
            List<int> changedRows = new List<int>();
            for (int row = 1; row < tlpLimits.RowCount; row++)
            {
                for (int col = 1; col < tlpLimits.ColumnCount - 1; col++)
                {
                    TextBox cntrl = tlpLimits.GetControlFromPosition(col, row) as TextBox;
                    if (cntrl != null && cntrl.ForeColor == System.Drawing.Color.Red)
                    {
                        if (!changedRows.Contains(row))
                        {
                            changedRows.Add(row);
                        }
                    }
                }
            }
            ICPClientDataContext ctx = new ICPClientDataContext();

            foreach (int elemRow in changedRows)
            {
                List<decimal> updateLimits = new List<decimal>();
                if (validateLimitUpdate(elemRow, updateLimits))
                {

                    string elem = (tlpLimits.GetControlFromPosition(0, elemRow) as Label).Text;
                    decimal reportLimit = updateLimits[0];
                    decimal mdLimit = updateLimits[1];
                    if (updateLimits.Count == 3)
                    {
                        decimal diffLimit = updateLimits[2];
                    }

                    tblLimit limit = ctx.tblLimits.Single(lim => lim.Element == elem);
                    if (limit != null)
                    {
                        limit.Element = (tlpLimits.GetControlFromPosition(0, elemRow) as Label).Text;
                        limit.Reporting = updateLimits[0];
                        limit.MDL = updateLimits[1];
                        if (updateLimits.Count == 3)
                        {
                            limit.DvsTDifference = updateLimits[2];
                        }

                        try
                        {
                            ctx.SubmitChanges();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Database Insert Failed: \n\n\t" + ex.Message, "Database Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            btnSaveLimit.Enabled = false;
            populateLimitTable();

        }



        /// <summary>
        /// Handles the Click event of the btnDeleteLimit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnDeleteLimit_Click(object sender, EventArgs e)
        {
            List<string> deleteRows = new List<string>();
            string confirmMsg = string.Empty;

            for (int row = 1; row < tlpLimits.RowCount; row++)
            {
                CheckBox rowCheck = tlpLimits.GetControlFromPosition(4, row) as CheckBox;
                if (rowCheck != null && rowCheck.Checked)
                {
                    string elem = (tlpLimits.GetControlFromPosition(0, row) as Label).Text;
                    deleteRows.Add(elem);
                    confirmMsg += elem + ", ";
                }
            }
            DialogResult okayToDelete = MessageBox.Show("Are you sure you want to delete limit values for the elements: " +
                    confirmMsg.Substring(0, confirmMsg.Length - 2), "Confirm Limit Deletes",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (okayToDelete == DialogResult.OK)
            {
                ICPClientDataContext ctx = new ICPClientDataContext();
                var limitsToGo = from lim in ctx.tblLimits
                                 where deleteRows.Contains(lim.Element)
                                 select lim;
                ctx.tblLimits.DeleteAllOnSubmit(limitsToGo);

                try
                {
                    ctx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Delete Failed: " + ex.Message);
                }
                btnDeleteLimit.Enabled = false;
                populateLimitTable();
            }
        }


        /// <summary>
        /// Handles the TextChanged event of the various txtLimitValue controls.
        /// Sets the change flag to true on a text change event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void txtLimitValue_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null)
            {
                tb.ForeColor = System.Drawing.Color.Red;
                limitChanged = true;
                btnSaveLimit.Enabled = true;
            }
        }


        /// <summary>
        /// Handles the CheckedChanged event of the cbSelected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void cbSelected_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.Checked)
            {
                btnDeleteLimit.Enabled = true;
            }
            else
            {
                bool atLeastOneChecked = false;
                for (int row = 1; row < tlpLimits.RowCount; row++)
                {
                    CheckBox rowCheck = tlpLimits.GetControlFromPosition(4, row) as CheckBox;
                    if (rowCheck != null && rowCheck.Checked)
                    {
                        atLeastOneChecked = true;
                        break;
                    }
                }
                btnDeleteLimit.Enabled = atLeastOneChecked;
            }
        }


        /// <summary>
        /// Clears the limit table of all non header controls.
        /// </summary>
        private void clearLimitTable()
        {
            for (int row = 1; row < tlpLimits.RowCount; row++)
            {
                for (int col = 0; col < tlpLimits.ColumnCount; col++)
                {
                    Control cntlToDie = tlpLimits.GetControlFromPosition(col, row);
                    tlpLimits.Controls.Remove(cntlToDie);
                }
            }
        }


        /// <summary>
        /// Validates the limit insert data.
        /// </summary>
        /// <returns></returns>
        private bool validateLimitUpdate(int elemRow, List<decimal> insertValues)
        {
            decimal reportLimit = 0.00M;
            decimal mdl = 0.00M;
            decimal diff = 0.00M;

            bool retVal = true;
            StringBuilder errMsg = new StringBuilder("The new record can not be inserted due to the following errors: \n\n");

            TextBox txtReportLimit = (tlpLimits.GetControlFromPosition(1, elemRow) as TextBox);
            if (txtReportLimit == null || txtReportLimit.Text.Length == 0 || !Decimal.TryParse(txtReportLimit.Text, out reportLimit))
            {
                retVal = false;
                errMsg.AppendLine("\tInvalid Reporting Limit Value:  Can not be blank and must be a decimal number.");
            }

            TextBox txtMDL = (tlpLimits.GetControlFromPosition(2, elemRow) as TextBox);
            if (txtMDL == null || txtMDL.Text.Length == 0 || !Decimal.TryParse(txtMDL.Text, out mdl))
            {
                retVal = false;
                errMsg.AppendLine("\tInvalid MDL Value:  Can not be blank and must be a decimal number.");
            }

            TextBox txtDvsTDiff = (tlpLimits.GetControlFromPosition(3, elemRow) as TextBox);
            if (txtDvsTDiff == null || (txtDvsTDiff.Text.Length > 0 && !Decimal.TryParse(txtDvsTDiff.Text, out diff)))
            {
                retVal = false;
                errMsg.AppendLine("\tInvalid Difference Value:  If entered the value must be a decimal number.");
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


        #endregion


        #region Details Tab
        /// <summary>
        /// Populates the details tab with data from the passed in TempICP object.
        /// </summary>
        /// <param name="icp">The icp.</param>
        private void populateDetails(TempICP icp)
        {
            lblBarCodeText.Text = icp.barCode;
            lblSampleText.Text = icp.code;
            lblBoxText.Text = icp.boxNumber;
            lblCodeText.Text = icp.duplicate;
            lblAnadateText.Text = icp.anaDate.ToShortDateString();


            // Display the element values
            // Consider Redo of this and make it generic
            lblAlValue.Text = (icp.elements.ContainsKey("Al")) ? icp.elements["Al"].ToString() : String.Empty;
            lblAsValue.Text = (icp.elements.ContainsKey("As")) ? icp.elements["As"].ToString() : String.Empty;
            lblCaValue.Text = (icp.elements.ContainsKey("Ca")) ? icp.elements["Ca"].ToString() : String.Empty;
            lblCdValue.Text = (icp.elements.ContainsKey("Cd")) ? icp.elements["Cd"].ToString() : String.Empty;
            lblCuValue.Text = (icp.elements.ContainsKey("Cu")) ? icp.elements["Cu"].ToString() : String.Empty;
            lblFeValue.Text = (icp.elements.ContainsKey("Fe")) ? icp.elements["Fe"].ToString() : String.Empty;
            lblKValue.Text = (icp.elements.ContainsKey("K")) ? icp.elements["K"].ToString() : String.Empty;
            lblMgValue.Text = (icp.elements.ContainsKey("Mg")) ? icp.elements["Mg"].ToString() : String.Empty;
            lblMnValue.Text = (icp.elements.ContainsKey("Mn")) ? icp.elements["Mn"].ToString() : String.Empty;
            lblNaValue.Text = (icp.elements.ContainsKey("Na")) ? icp.elements["Na"].ToString() : String.Empty;
            lblPbValue.Text = (icp.elements.ContainsKey("Pb")) ? icp.elements["Pb"].ToString() : String.Empty;
            lblSeValue.Text = (icp.elements.ContainsKey("Se")) ? icp.elements["Se"].ToString() : String.Empty;
            lblZnValue.Text = (icp.elements.ContainsKey("Zn")) ? icp.elements["Zn"].ToString() : String.Empty;

            SortedList<string, TempICP> others = createSampleList(icp, false);

            // dynamically populate the associated ICP table and performs dependent validations
            tlpAssocICP.Controls.Clear();
            int row = 1;

            switch (icp.duplicate)
            {

                case "00":
                    setAssocRow("Pair:", others.ContainsKey("04") ? others["04"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("10") ? others["10"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("14") ? others["14"] : null, ref row);
                    setAssocRow("Duplicate:", others.ContainsKey("20") ? others["20"] : null, ref row);
                    setAssocRow("Duplicate:", others.ContainsKey("24") ? others["24"] : null, ref row);
                    break;

                case "01":
                    setAssocRow("Blank:", others.ContainsKey("11") ? others["11"] : null, ref row);
                    setAssocRow("Duplicate:", others.ContainsKey("21") ? others["21"] : null, ref row);
                    break;

                case "03":
                    setAssocRow("Blank:", others.ContainsKey("13") ? others["13"] : null, ref row);
                    setAssocRow("Duplicate:", others.ContainsKey("23") ? others["23"] : null, ref row);
                    break;

                case "04":
                    setAssocRow("Pair:", others.ContainsKey("00") ? others["00"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("10") ? others["10"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("14") ? others["14"] : null, ref row);
                    setAssocRow("Duplicate:", others.ContainsKey("20") ? others["20"] : null, ref row);
                    setAssocRow("Duplicate:", others.ContainsKey("24") ? others["24"] : null, ref row);
                    break;

                case "10":
                    setAssocRow("Pair:", others.ContainsKey("14") ? others["14"] : null, ref row);
                    setAssocRow("Normal:", others.ContainsKey("00") ? others["00"] : null, ref row);
                    setAssocRow("Normal:", others.ContainsKey("04") ? others["04"] : null, ref row);
                    setAssocRow("Duplicate:", others.ContainsKey("20") ? others["20"] : null, ref row);
                    setAssocRow("Duplicate:", others.ContainsKey("24") ? others["24"] : null, ref row);
                    break;

                case "11":
                    setAssocRow("Normal:", others.ContainsKey("01") ? others["01"] : null, ref row);
                    setAssocRow("Duplicate:", others.Keys.Contains("21") ? others["21"] : null, ref row);
                    break;

                case "13":
                    setAssocRow("Normal:", others.ContainsKey("03") ? others["03"] : null, ref row);
                    setAssocRow("Duplicate:", others.Keys.Contains("23") ? others["23"] : null, ref row);
                    break;

                case "14":
                    setAssocRow("Pair:", others.ContainsKey("10") ? others["10"] : null, ref row);
                    setAssocRow("Normal:", others.ContainsKey("00") ? others["00"] : null, ref row);
                    setAssocRow("Normal:", others.ContainsKey("04") ? others["04"] : null, ref row);
                    setAssocRow("Duplicate:", others.Keys.Contains("20") ? others["20"] : null, ref row);
                    setAssocRow("Duplicate:", others.Keys.Contains("24") ? others["24"] : null, ref row);
                    break;

                case "20":
                    setAssocRow("Pair:", others.ContainsKey("24") ? others["24"] : null, ref row);
                    setAssocRow("Normal:", others.ContainsKey("00") ? others["00"] : null, ref row);
                    setAssocRow("Normal:", others.ContainsKey("04") ? others["04"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("10") ? others["10"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("14") ? others["14"] : null, ref row);
                    break;

                case "21":
                    setAssocRow("Normal:", others.ContainsKey("01") ? others["01"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("11") ? others["11"] : null, ref row);
                    break;

                case "23":
                    setAssocRow("Normal:", others.ContainsKey("03") ? others["03"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("13") ? others["13"] : null, ref row);
                    break;

                case "24":
                    setAssocRow("Pair:", others.ContainsKey("20") ? others["20"] : null, ref row);
                    setAssocRow("Normal:", others.ContainsKey("00") ? others["00"] : null, ref row);
                    setAssocRow("Normal:", others.ContainsKey("04") ? others["04"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("10") ? others["10"] : null, ref row);
                    setAssocRow("Blank:", others.ContainsKey("14") ? others["14"] : null, ref row);
                    break;

                default:
                    break;
            }

            // Display the errors
            StringBuilder errorMsg = new StringBuilder();
            foreach (string line in icp.errorList)
            {
                errorMsg.AppendLine(line);
            }
            rtbErrorListing.Text = errorMsg.ToString();

            // Display the warnings
            StringBuilder warningMsg = new StringBuilder();
            foreach (string line in icp.warningList)
            {
                warningMsg.AppendLine(line);
            }
            rtbWarningListing.Text = warningMsg.ToString();

            // set the ready for export check box
            bool readyToExport = icp.errorList.Count == 0;
            foreach (TempICP otherICP in others.Values)
            {
                readyToExport &= otherICP.errorList.Count == 0;
            }
            cbReadyForExport.Checked = readyToExport;
        }


        private void clearDetails()
        {
            lblBarCodeText.Text = String.Empty;
            lblSampleText.Text = String.Empty;
            lblBoxText.Text = String.Empty;
            lblCodeText.Text = String.Empty;
            lblAnadateText.Text = String.Empty;


            // Display the element values
            // Consider Redo of this and make it generic
            lblAlValue.Text = String.Empty;
            lblAsValue.Text = String.Empty;
            lblCaValue.Text = String.Empty;
            lblCdValue.Text = String.Empty;
            lblCuValue.Text = String.Empty;
            lblFeValue.Text = String.Empty;
            lblKValue.Text = String.Empty;
            lblMgValue.Text = String.Empty;
            lblMnValue.Text = String.Empty;
            lblNaValue.Text = String.Empty;
            lblPbValue.Text = String.Empty;
            lblSeValue.Text = String.Empty;
            lblZnValue.Text = String.Empty;

            // dynamically populate the associated ICP table and performs dependent validations
            tlpAssocICP.Controls.Clear();

            rtbErrorListing.Text = "";
            rtbWarningListing.Text = "";

            cbReadyForExport.Checked = false;

        }


        /// <summary>
        /// Validates the elements associated with a given barcode.
        /// </summary>
        /// <param name="tempICP">The temp ICP.</param>
        /// <param name="interICPList">The inter ICP list.</param>
        private void validateElements(TempICP tempICP, List<LocaltblIntermeditateICP> interICPList)
        {
            bool firstOne = true;
            foreach (var interICP in interICPList)
            {
                if (firstOne && interICP.Anadate.HasValue)
                {
                    tempICP.anaDate = interICP.Anadate.Value;
                    firstOne = false;
                }

                // Make sure that duplicates are not in the TevaICP database
                if (interICP.AverageResult.HasValue)
                {
                    if (tempICP.elements.Keys.Contains(interICP.Element))
                    {
                        if (!interICP.DeleteFlag.HasValue ||
                        (interICP.DeleteFlag.HasValue && !interICP.DeleteFlag.Value))
                        {

                            string errorMsg = String.Empty;
                            if (tempICP.anaDate == interICP.Anadate)
                            {
                                errorMsg = interICP.BarCode +
                                        ": Multiple values for Element: " + interICP.Element;
                            }
                            else
                            {
                                errorMsg = interICP.BarCode +
                                    "   Duplicate element results in Teva ICP database:  Anadates: " +
                                    tempICP.anaDate + ",  " + interICP.Anadate;
                            }
                            tempICP.errorList.Add(errorMsg);
                        }
                    }
                    else
                    {
                        IEnumerable<decimal> elemLimit = from lim in limitList
                                                         where lim.Element == interICP.Element
                                                         select lim.Reporting;

                        decimal limit = elemLimit.FirstOrDefault();
                        if (interICP.AverageResult.Value < limit)
                        {
                            if (!interICP.DeleteFlag.HasValue ||
                                    (interICP.DeleteFlag.HasValue && !interICP.DeleteFlag.Value))
                            {
                                tempICP.elements.Add(interICP.Element, 0.00M);
                            }

                            // don't add comment if sample is a blank
                            if (!tempICP.duplicate.Trim().Equals("10") &&
                                    !tempICP.duplicate.Trim().Equals("14"))
                            {
                                tempICP.comments += interICP.Element + ": < RL.  ";
                            }
                        }
                        else
                        {
                            decimal? result = null;
                            switch (interICP.Element)
                            {
                                case "Cd":
                                    result = interICP.AverageResult.HasValue ?
                                        (decimal?)Math.Round(interICP.AverageResult.Value, 2) : (decimal?)null;
                                    break;
                                case "Cu":
                                    result = interICP.AverageResult.HasValue ?
                                        (decimal?)Math.Round(interICP.AverageResult.Value, 1) : (decimal?)null;
                                    break;
                                case "Mn":
                                    result = interICP.AverageResult.HasValue ?
                                        (decimal?)Math.Round(interICP.AverageResult.Value, 1) : (decimal?)null;
                                    break;
                                case "Pb":
                                    result = interICP.AverageResult.HasValue ?
                                        (decimal?)Math.Round(interICP.AverageResult.Value, 1) : (decimal?)null;
                                    break;
                                case "Se":
                                    result = interICP.AverageResult.HasValue ?
                                        (decimal?)Math.Round(interICP.AverageResult.Value, 1) : (decimal?)null;
                                    break;
                                case "Zn":
                                    result = interICP.AverageResult.HasValue ?
                                        (decimal?)Math.Round(interICP.AverageResult.Value, 1) : (decimal?)null;
                                    break;
                                default:
                                    result = interICP.AverageResult.HasValue ?
                                        (decimal?)Math.Round(interICP.AverageResult.Value, 0) : (decimal?)null;
                                    break;
                            }
                            if (!interICP.DeleteFlag.HasValue ||
                                    (interICP.DeleteFlag.HasValue && !interICP.DeleteFlag.Value))
                            {
                                tempICP.elements.Add(interICP.Element, result);
                            }
                        }
                    }
                }
                // Check for checkFlag values
                // bwitt dec 7, 2011 added a check for is alpha rather than empty string, which seems to happen after updates

                //                if (string.IsNullOrEmpty(inputStr)) 
                //11.        return false;
                //12. 
                //13.    //now we need to loop through the string, examining each character
                //14.    for (int i = 0; i < inputStr.Length; i++)
                //15.    {
                //16.        //if this character isn't a letter and it isn't a number then return false
                //17.        //because it means this isn't a valid alpha numeric string
                //18.        if (!(char.IsLetter(inputStr[i])) && (!(char.IsNumber(inputStr[i]))))
                //19.            return false;
                //20.    }

                if (!string.IsNullOrEmpty(interICP.CheckFlag))
                {

                    for (int i = 0; i < interICP.CheckFlag.Length; i++)
                    {
                        //if this character isn't a letter and it isn't a number then return false
                        //because it means this isn't a valid alpha numeric string

                        if (char.IsLetter(interICP.CheckFlag[i]))  // if this is a valid letter, record it, otherwise, move on
                        {
                            string errorMsg = interICP.BarCode + ":   " + interICP.Element +
                            ": Check Flag error from the Teva ICP database.  Check Flag value: " +
                            interICP.CheckFlag;
                            tempICP.errorList.Add(errorMsg);
                        }
                    }
                    // Check for failFlag values
                    if (interICP.FailFlags.Trim().Length > 0)
                    {
                        string errorMsg = interICP.BarCode + ":   " + interICP.Element +
                                ": Fail Flag error from the Teva ICP database.  Fail Flag value: " +
                                interICP.FailFlags.Trim();
                        tempICP.errorList.Add(errorMsg);
                    }
                }
            }
        }


        /// <summary>
        /// Validates the dissolved total.
        /// </summary>
        /// <param name="icp">The 00 element.</param>
        /// <param name="pair">The 04 element.</param>
        /// <returns></returns>
        private void validateDissolvedTotal(TempICP tot, TempICP dis)
        {

            ICPClientDataContext ctx = new ICPClientDataContext();
            foreach (string elem in elementTypes)
            {
                if (tot.elements.ContainsKey(elem) && dis.elements.ContainsKey(elem))
                {
                    decimal totElem = tot.elements[elem].Value;
                    decimal disElem = dis.elements[elem].Value;

                    IEnumerable<tblLimit> limit = from lim in ctx.tblLimits
                                                  where lim.Element.Equals(elem)
                                                  select lim;

                    if ((disElem - totElem) > limit.FirstOrDefault().DvsTDifference.Value)
                    {
                        tot.warningList.Add(tot.code + ":   " + tot.duplicate + ":   " +
                                elem + ": Dissolved - Total value difference is too large.");
                    }
                }
            }
            ctx.Dispose();
        }


        /// <summary>
        /// Validates that each of the blank elements is less than the reporting limit.
        /// </summary>
        /// <param name="icp">The blank icp element to validate.</param>
        /// <returns></returns>
        private void validateBlankElements(TempICP icp)
        {
            foreach (string elem in elementTypes)
            {
                if (icp.elements.ContainsKey(elem))
                {
                    decimal icpElem = icp.elements[elem].Value;

                    IEnumerable<tblLimit> limit = from lim in limitList
                                                  where lim.Element.Equals(elem)
                                                  select lim;

                    if (icpElem > limit.FirstOrDefault().Reporting)
                    {
                        icp.warningList.Add(icp.code + ":   " + icp.duplicate + ":   " +
                                elem + ": Result is greater than reporting limit");
                    }
                }
            }
        }


        /// <summary>
        /// Validates that each of the duplicate elements within 20% of normal.
        /// </summary>
        /// <param name="icp">The blank icp element to validate.</param>
        /// <returns></returns>
        private void validateDupElements(TempICP dup, TempICP norm)
        {

            foreach (string elem in elementTypes)
            {
                if (dup.elements.ContainsKey(elem) && norm.elements.ContainsKey(elem))
                {
                    decimal dupElem = dup.elements[elem].Value;
                    decimal normElem = norm.elements[elem].Value;

                    if (normElem != 0 && (Math.Abs(dupElem - normElem) / normElem) > 0.20M)
                    {
                        dup.warningList.Add(dup.code + ":   " + dup.duplicate + ":   " +
                                elem + ": Difference in duplicate and normal values exceeds 20%");
                    }
                }
            }
        }


        /// <summary>
        /// Dynamically creates a row from the passed in information 
        /// in the Details tab association table.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="other">The other.</param>
        /// <param name="row">The row.</param>
        private void setAssocRow(string header, TempICP other, ref int row)
        {
            int col = 0;
            Label lblHead = new Label();
            lblHead.TextAlign = ContentAlignment.MiddleLeft;
            lblHead.Text = header;
            lblHead.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tlpAssocICP.Controls.Add(lblHead, col, row);
            col++;

            if (other != null)
            {
                Label lblSamp = new Label();
                lblSamp.TextAlign = ContentAlignment.MiddleCenter;
                lblSamp.Text = "Sample:";
                tlpAssocICP.Controls.Add(lblSamp, col, row);
                col++;

                Label lblSampValue = new Label();
                lblSampValue.TextAlign = ContentAlignment.MiddleLeft;
                lblSampValue.Text = other.code;
                lblSampValue.ForeColor = (other.errorList.Count > 0) ? System.Drawing.Color.Red :
                        ForeColor = System.Drawing.Color.Black;
                tlpAssocICP.Controls.Add(lblSampValue, col, row);
                col++;

                Label lblBarCode = new Label();
                lblBarCode.TextAlign = ContentAlignment.MiddleCenter;
                lblBarCode.Text = "Bar Code:";
                tlpAssocICP.Controls.Add(lblBarCode, col, row);
                col++;

                Label lblBarCodeValue = new Label();
                lblBarCodeValue.TextAlign = ContentAlignment.MiddleLeft;
                lblBarCodeValue.Text = other.barCode;
                lblBarCodeValue.ForeColor = (other.errorList.Count > 0) ? System.Drawing.Color.Red :
                        ForeColor = System.Drawing.Color.Black;
                lblBarCodeValue.Click += new EventHandler(lblBarCodeValue_Click);

                tlpAssocICP.Controls.Add(lblBarCodeValue, col, row);
                col++;

                Label lblCode = new Label();
                lblCode.TextAlign = ContentAlignment.MiddleCenter;
                lblCode.Text = "Code:";
                tlpAssocICP.Controls.Add(lblCode, col, row);
                col++;

                Label lblCodeValue = new Label();
                lblCodeValue.TextAlign = ContentAlignment.MiddleLeft;
                lblCodeValue.Text = other.duplicate;
                lblCodeValue.ForeColor = (other.errorList.Count > 0) ? System.Drawing.Color.Red :
                        ForeColor = System.Drawing.Color.Black;
                tlpAssocICP.Controls.Add(lblCodeValue, col, row);
                row++;
            }
            else
            {
                Label lblNone = new Label();
                lblNone.TextAlign = ContentAlignment.MiddleCenter;
                lblNone.Text = "None";
                tlpAssocICP.Controls.Add(lblNone, col, row);
                row++;
            }
        }


        /// <summary>
        /// Handles the Click event of the lblBarCodeValue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void lblBarCodeValue_Click(object sender, EventArgs e)
        {
            Label lbl = sender as Label;

            if (lbl != null && !string.IsNullOrEmpty(lbl.Text))
            {
                TreeNode selected = findNodeFromText(lbl.Text);
                if (selected != null)
                {
                    tvBarCodes.SelectedNode = selected;
                    tvBarCodes.SelectedNode.EnsureVisible();
                }
            }
        }


        /// <summary>
        /// Handles the AfterSelect event of the tvBarCodes control.
        /// Details Panel
        /// This method retrieves the selected node from the tree view and populates the 
        /// details section with the selected bar code.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
        private void tvBarCodes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeView tv = sender as TreeView;
            TreeNode selNode = tv.SelectedNode;

            if (selNode != null)
            {
                IEnumerable<TempICP> currentICP = from icp in tempIcpList
                                                  where icp.barCode == selNode.Text
                                                  select icp;
                if (currentICP != null && currentICP.Count() == 1)
                {
                    populateDetails(currentICP.FirstOrDefault());
                }
            }
        }


        /// <summary>
        /// This text box allows a user populate its contents by text entry or scanning.
        /// It populates the Details panel with the bar code corresponding to the entered value.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void txtbarCodeFilter_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (tb != null && !string.IsNullOrEmpty(tb.Text))
            {
                TreeNode selected = findNodeFromText(tb.Text);
                if (selected != null)
                {
                    tvBarCodes.SelectedNode = selected;
                    tvBarCodes.SelectedNode.EnsureVisible();
                }
                else
                {
                    clearDetails();
                    if (tb.Text.Length >= 9)
                    {
                        tabICP.SelectTab(tabRiverwatch);
                        txtBarcodeToCheck.Text = tb.Text;
                    }
                }
            }
        }


        /// <summary>
        /// Handles the Click event of the btnUpdateDetails control.
        /// Pops up the DetailUpdateForm to allow user modification of element results
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnUpdateDetails_Click(object sender, EventArgs e)
        {
            string barCode = lblBarCodeText.Text;
            if (!string.IsNullOrEmpty(barCode))
            {
                IEnumerable<TempICP> currentICP = from icp in tempIcpList
                                                  where icp.barCode == barCode
                                                  select icp;
                if (currentICP != null && currentICP.Count() == 1)
                {
                    Form updateDetailForm = new DetailUpdateForm(currentICP.First());
                    DialogResult button = updateDetailForm.ShowDialog();
                    if (button == DialogResult.OK)
                    {
                        List<LocaltblIntermeditateICP> newInterICPList =
                                updateDetailForm.Tag as List<LocaltblIntermeditateICP>;
                        TempICP tempIcp = currentICP.First();
                        tempIcp.elements.Clear();
                        tempIcp.errorList.Clear();
                        tempIcp.warningList.Clear();
                        tempIcp.comments = string.Empty;

                        validateElements(tempIcp, newInterICPList);
                        verifyElementsComplete(tempIcp);
                        validateICP(tempIcp);

                        // remove the old node
                        TreeNode oldNode = findNodeFromText(tempIcp.barCode);
                        oldNode.Remove();

                        // add and select the new node

                        tvBarCodes.SelectedNode = addTempIcpToTree(tempIcp);
                        tvBarCodes.SelectedNode.EnsureVisible();

                        populateDetails(tempIcp);
                    }
                }
            }
            else
            {
                MessageBox.Show("You must select a barcode before you can edit it.");
            }
        }


        /// <summary>
        /// Returns the Color associated with the TempICP error/warning lists.
        /// </summary>
        /// <param name="icp">The TempICP object.</param>
        /// <returns>The Color associated with the passed in TempICP object</returns>
        private Color getNodeColor(TempICP icp)
        {
            Color retVal = System.Drawing.Color.Black;

            if (icp.errorList.Count > 0)
            {
                retVal = System.Drawing.Color.Red;

            }
            else if (icp.warningList.Count > 0)
            {
                retVal = System.Drawing.Color.Plum;
            }

            return retVal;
        }

        /// <summary>
        /// Searches the entire detail tree and returns the tree node associated with the passed 
        /// in barcode.
        /// </summary>
        /// <param name="text">The barcode to find.</param>
        /// <returns></returns>
        private TreeNode findNodeFromText(string text)
        {
            TreeNode retVal = null;

            foreach (TreeNode treeCollect in tvBarCodes.Nodes[0].Nodes)
            {
                foreach (TreeNode child in treeCollect.Nodes)
                {
                    if (child.Text == text)
                    {
                        retVal = child;
                        break;
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Searches the detail tree and returns the box number tree node associated with the passed 
        /// in box number.
        /// </summary>
        /// <param name="text">The barcode to find.</param>
        /// <returns></returns>
        private TreeNode findBoxNodeFromText(string text)
        {
            TreeNode retVal = null;

            foreach (TreeNode boxNumNode in tvBarCodes.Nodes[0].Nodes)
            {
                int endIndx = boxNumNode.Text.IndexOf(":");
                string boxNum = (endIndx >= 0) ? boxNumNode.Text.Substring(0, endIndx) :
                        boxNumNode.Text;
                if (boxNum == text)
                {
                    retVal = boxNumNode;
                    break;
                }
            }
            return retVal;
        }



        /// <summary>
        /// Creates a tree node using the passed in barcode..
        /// </summary>
        /// <param name="tempIcp">The temp icp.</param>
        private TreeNode addTempIcpToTree(TempICP tempICP)
        {
            string boxNum = (string.IsNullOrEmpty(tempICP.boxNumber)) ?
                    "Default" : tempICP.boxNumber;
            TreeNode parentNode = findBoxNodeFromText(boxNum);
            TreeNode retVal = null;

            // Insert the new node alphabetically
            bool inserted = false;
            foreach (TreeNode node in parentNode.Nodes)
            {
                if (node.Text.CompareTo(tempICP.barCode) > 0)
                {
                    retVal = parentNode.Nodes.Insert(node.Index, tempICP.barCode);
                    retVal.ForeColor = getNodeColor(tempICP);
                    inserted = true;
                    break;
                }
            }
            if (!inserted)
            {
                retVal = parentNode.Nodes.Add(tempICP.barCode);
                retVal.ForeColor = getNodeColor(tempICP);
            }

            return retVal;
        }


        #endregion


        #region Export Tab

        /// <summary>
        /// Handles the Click event of the btnExportData control.
        /// BWitt no checking for duplicates here either
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnExportData_Click(object sender, EventArgs e)
        {
            // create a csv file of export list data
            string csvFile = createCsvFile();

            // create the associated warning report
            string warnFile = createWarningFile();

            // create and send an email to recepients
            // createExportEmail(csvFile, warnFile);

            // export export list to Riverwatch database
            exportResultsToRiverwatch();

            // back up data to the local cooler table
            backupResultsToCooler();

            // delete localtblInboundIcp records.
            deleteLocalIntermediateResults();

            btnExportData.Enabled = false;

            MessageBox.Show("Export Complete");
            tabICP.SelectTab(tabStatus);
        }


        /// <summary>
        /// Backs up the results in the Riverwatch export to the local cooler table.
        /// BWitt - seems to be no check for duplicate bar codes here Dec 10, 2011
        /// </summary>
        private void backupResultsToCooler()
        {
            ICPClientDataContext icpClientCtx = new ICPClientDataContext();
            foreach (LocalCooler cooler in coolerList)
            {
                try
                {
                    icpClientCtx.LocalCoolers.InsertOnSubmit(cooler);
                    icpClientCtx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred on insert into the local cooler table: " +
                        cooler.CODE + ", " + cooler.DUPLICATE + "  Error: " + ex.Message);
                }
            }
            icpClientCtx.Dispose();
        }


        /// <summary>
        /// Deletes the local intermediate results associated with the exported
        /// records.
        /// </summary>
        private void deleteLocalIntermediateResults()
        {
            ICPClientDataContext icpClientCtx = new ICPClientDataContext();
            IEnumerable<LocaltblIntermeditateICP> deleteList =
                    from toKill in icpClientCtx.LocaltblIntermeditateICPs
                    where (from code in exportList
                           select code.CODE).Distinct().Contains(toKill.Code)
                    select toKill;

            icpClientCtx.LocaltblIntermeditateICPs.DeleteAllOnSubmit(deleteList);
            icpClientCtx.SubmitChanges();
            icpClientCtx.Dispose();
        }


        /// <summary>
        /// Exports the results to riverwatch.
        /// </summary>
        /// 
        // XXXX need to call web api here to save each bar code

        private void exportResultsToRiverwatch()
        {
            RiverwatchDataContext rwCtx = new RiverwatchDataContext();  // this is production data base

            foreach (tblInboundICP inIcp in exportList)
            {
                try
                {
                    rwCtx.tblInboundICPs.InsertOnSubmit(inIcp); // writes to data base
                    rwCtx.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Insert of results into the Riverwatch database: " +
                            inIcp.CODE + ", " + inIcp.DUPLICATE +
                            " failed with the following error: " + ex.Message);
                }
            }
            rwCtx.Dispose();
        }


        /// <summary>
        /// This appears to be unused bwitt Oct 25, 2016
        /// Creates the export email.
        /// </summary>
        /// <param name="csvFile">The CSV file.</param>
        /// <param name="warnFile">The warn file.</param>
        private void createExportEmail(string csvFile, string warnFile)
        {
            try
            {
                // create mail message content
                StringBuilder plainViewText = new StringBuilder();

                plainViewText.AppendLine("ICP metal data was inserted into the Riverwatch database: tblInboundICP.");
                plainViewText.Append("\tSamples contained in export:  ");
                plainViewText.AppendLine(lblExportSampleCntValue.Text);
                plainViewText.AppendLine();
                plainViewText.Append("Attached please find the csv backup file for the database insert and a text file ");
                plainViewText.AppendLine("outlining the warnings associated with the inserted samples.");


                StringBuilder htmlViewText = new StringBuilder();
                htmlViewText.Append("<html><body><div style='font-family:Verdana; font-size:smaller'>");
                htmlViewText.Append("<p><img alt='DOW Banner Page' src='cid:DOWLogo' /></p>");
                htmlViewText.Append("<h3 style='text-align:center'>ICP Metal Data Export Notification</h3>");
                htmlViewText.Append("<p>ICP metal data was inserted into the Riverwatch database: tblInboundICP.</p><ol>");
                htmlViewText.Append("<li>Samples contained in export:  ");
                htmlViewText.Append(lblExportSampleCntValue.Text + "</li>");
                htmlViewText.Append("<li>Date / Time Sent: ");
                htmlViewText.Append(DateTime.Now.ToShortDateString() + "  ");
                htmlViewText.Append(DateTime.Now.ToShortTimeString() + "</li></ol>");
                htmlViewText.Append("<p>Attached please find the csv backup file for the database insert and a text file ");
                htmlViewText.Append("outlining the warnings associated with the inserted samples.</br></p>");
                htmlViewText.Append("<p>Please let me know if there are any questions or problems with this export.</p>");
                htmlViewText.Append("</div></body></html>");

                MailMessage mm = new MailMessage();
                // from address retrieved from web.config default
                mm.To.Add(new MailAddress("jlawless@adcc.com"));
                mm.Subject = "Notification of ICP Metal Data Upload";
                mm.From = new MailAddress("Matt.McIntyre@state.co.us");

                AlternateView plainView = AlternateView.
                        CreateAlternateViewFromString(plainViewText.ToString(), null, "text/plain");
                AlternateView htmlView = AlternateView.
                        CreateAlternateViewFromString(htmlViewText.ToString(), null, "text/html");

                // TODO get this image path into the app_settings
                string imagePath = "../../images/Banner.jpg";
                LinkedResource logo = new LinkedResource(imagePath);
                logo.ContentId = "DOWLogo";
                htmlView.LinkedResources.Add(logo);

                mm.AlternateViews.Add(plainView);
                mm.AlternateViews.Add(htmlView);

                Attachment csvAttach = new Attachment(csvFile);
                if (csvAttach != null)
                {
                    mm.Attachments.Add(csvAttach);
                }

                Attachment warnAttach = new Attachment(warnFile);
                if (warnAttach != null)
                {
                    mm.Attachments.Add(warnAttach);
                }


                SmtpClient smtp = new SmtpClient();
                smtp.Host = "relay.appriver.com";
                smtp.Port = 2525;

                smtp.Send(mm);
                foreach (Attachment attach in mm.Attachments)
                {
                    attach.Dispose();
                }

                MessageBox.Show("A notification email was sent to the Riverwatch Data Coordinator.", "Export Notification Email");
            }
            catch (SmtpException smtpEx)
            {
                MessageBox.Show("SMTP Exception: The notification email failed as follows:  " +
                        smtpEx.Message, "Notification Email Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("General Email Exception:  The notification email failed as follows:  " +
                        ex.Message, "Notification Email Error");
            }
        }


        /// <summary>
        /// Creates the warning file and returns the file path.
        /// </summary>
        /// <returns></returns>
        private string createWarningFile()
        {
            string retVal = null;

            if (exportList.Count > 0)
            {
                NameValueCollection settings = ConfigurationManager.AppSettings;
                string dirPath = settings["exportDir"];
                retVal = dirPath + "MetalWarnings" + createFileDateString() + ".txt";

                StreamWriter textOut = null;

                try
                {
                    textOut = new StreamWriter(
                            new FileStream(retVal, FileMode.Create, FileAccess.Write));

                    textOut.WriteLine("Associated Result File:      MetalImport" + createFileDateString() + ".csv");
                    textOut.WriteLine();

                    foreach (string key in warningList.Keys)
                    {
                        textOut.WriteLine(key);

                        List<string> listOfWarnings = warningList[key];

                        if (listOfWarnings.Count() == 0)
                        {
                            textOut.WriteLine("\tNone");
                        }
                        else
                        {
                            foreach (string warn in listOfWarnings)
                            {
                                textOut.WriteLine("\t" + warn);
                            }
                            textOut.WriteLine();
                        }
                    }
                    textOut.Close();
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show(dirPath + "not found.", "Directory Not Found",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IOException ioe)
                {
                    MessageBox.Show(ioe.Message, "IOException");
                }
                finally
                {
                    if (textOut != null)
                    {
                        textOut.Close();
                    }
                }

            }
            return retVal;
        }


        /// <summary>
        /// Creates the CSV file and returns the file path.
        /// </summary>
        /// <returns></returns>
        private string createCsvFile() 
        {
            string retVal = null;

            if (exportList.Count > 0)
            {
                NameValueCollection settings = ConfigurationManager.AppSettings;
                string dirPath = settings["exportDir"];
                retVal = dirPath + "MetalImport" + createFileDateString() + ".csv";

                StreamWriter textOut = null;

                try
                {
                    textOut = new StreamWriter(
                            new FileStream(retVal, FileMode.Create, FileAccess.Write));

                    textOut.WriteLine("CODE,DUPLICATE,AL_D,AL_T,AS_D,AS_T,CA_D,CA_T,CD_D,CD_T," +
                            "CU_D,CU_T,FE_D,FE_T,PB_D,PB_T,MG_D,MG_T,MN_D,MN_T,SE_D,SE_T," +
                            "ZN_D,ZN_T,NA_D,NA_T,K_D,K_T,ANADATE,COMPLETE,DATE_SENT,Comments," +
                            "PassValStep,Reviewed,FailedChems,tblSampleID");

                    foreach (tblInboundICP inIcp in exportList)
                    {
                        textOut.Write(inIcp.CODE + ",");
                        textOut.Write(inIcp.DUPLICATE + ",");
                        textOut.Write(inIcp.AL_D + ",");
                        textOut.Write(inIcp.AL_T + ",");
                        textOut.Write(inIcp.AS_D + ",");
                        textOut.Write(inIcp.AS_T + ",");
                        textOut.Write(inIcp.CA_D + ",");
                        textOut.Write(inIcp.CA_T + ",");
                        textOut.Write(inIcp.CD_D + ",");
                        textOut.Write(inIcp.CD_T + ",");
                        textOut.Write(inIcp.CU_D + ",");
                        textOut.Write(inIcp.CU_T + ",");
                        textOut.Write(inIcp.FE_D + ",");
                        textOut.Write(inIcp.FE_T + ",");
                        textOut.Write(inIcp.PB_D + ",");
                        textOut.Write(inIcp.PB_T + ",");
                        textOut.Write(inIcp.MG_D + ",");
                        textOut.Write(inIcp.MG_T + ",");
                        textOut.Write(inIcp.MN_D + ",");
                        textOut.Write(inIcp.MN_T + ",");
                        textOut.Write(inIcp.SE_D + ",");
                        textOut.Write(inIcp.SE_T + ",");
                        textOut.Write(inIcp.ZN_D + ",");
                        textOut.Write(inIcp.ZN_T + ",");
                        textOut.Write(inIcp.NA_D + ",");
                        textOut.Write(inIcp.NA_T + ",");
                        textOut.Write(inIcp.K_D + ",");
                        textOut.Write(inIcp.K_T + ",");
                        textOut.Write(inIcp.ANADATE + ",");
                        textOut.Write(inIcp.COMPLETE + ",");
                        textOut.Write(inIcp.DATE_SENT + ",");
                        textOut.Write(inIcp.Comments + ",");
                        textOut.Write(inIcp.PassValStep + ",");
                        textOut.Write(inIcp.Reviewed + ",");
                        textOut.Write(inIcp.FailedChems + ",");
                        textOut.WriteLine(inIcp.tblSampleID);
                    }
                    textOut.Close();
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show(dirPath + "not found.", "Directory Not Found",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IOException ioe)
                {
                    MessageBox.Show(ioe.Message, "IOException");
                }
                finally
                {
                    if (textOut != null)
                    {
                        textOut.Close();
                    }
                }
            }
            return retVal;
        }


        /// <summary>
        /// Creates the file date string from the current date time.
        /// </summary>
        /// <returns></returns>
        private string createFileDateString()
        {
            string dateStr = DateTime.Now.Year.ToString();
            string monthStr = (DateTime.Now.Month.ToString().Length == 1) ?
                    "0" + DateTime.Now.Month.ToString() : DateTime.Now.Month.ToString();
            string dayStr = (DateTime.Now.Day.ToString().Length == 1) ?
                    "0" + DateTime.Now.Day.ToString() : DateTime.Now.Day.ToString();
            string hourStr = (DateTime.Now.Hour.ToString().Length == 1) ?
                    "0" + DateTime.Now.Hour.ToString() : DateTime.Now.Hour.ToString();
            string minuteStr = (DateTime.Now.Minute.ToString().Length == 1) ?
                    "0" + DateTime.Now.Minute.ToString() : DateTime.Now.Minute.ToString();

            return dateStr + monthStr + dayStr + hourStr + minuteStr;
        }

        #endregion


        #region Status Tab

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// 

        // XXXX data base hits here too

        private void btnStart_Click(object sender, EventArgs e)
        { 
            DateTime startTime = DateTime.Now;  // added to try to keep watch on time to read data base
            int records = 0; // count of how many bar codes were downloaded

            IEnumerable<tblMetalBarCode> barCodeList = null;   // move this from inside try section
            statLabel.Text = "Retrieving bar code data from the Riverwatch database.";
            statusStrip1.Update();
            try
            {
                RiverwatchDataContext rwCtx = new RiverwatchDataContext();  // rw data base
                ICPClientDataContext icptx = new ICPClientDataContext();    // local icp data base

                // added 5 Dec 2011 - bwitt to help with possible timeout errors 
                // will also increase timeout time 

                rwCtx.Connection.Open();
                rwCtx.ExecuteCommand("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                rwCtx.CommandTimeout = 120; // increase timeout to 2 min

                // Get the list of all bar codes associated with samples that do not have results.
                // bwitt Dec 12, 2011 
                barCodeList =                        
                        from bc in rwCtx.tblMetalBarCodes
                        join samp in rwCtx.tblSamples
                        on bc.SampleID equals samp.SampleID
                        where samp.DateCollected.Year >= 2006
                        && !(from cs in rwCtx.tblChemSamps
                             where cs.CollMeth == 6
                             select cs.SampleID).Distinct().Contains(bc.SampleID.Value)
                        && !(from inICP in rwCtx.tblInboundICPs
                             select inICP.tblSampleID).Distinct().Contains(bc.SampleID.Value)                      
                        orderby bc.LabID
                        select bc;

                rwCtx.Connection.Close();   // must close as we opened and set timeout, above

                records = barCodeList.ToList().Count; // must convert to list to get count ??
                // make list of bar codes in localcooler table to compare against barcodelist

               List<string>  lcList = (from lc in icptx.LocalCoolers
                                           select lc.BarCode).ToList<string>() ; 

                // filter out the barcodes that are currently in cooler list
                // if requested by pushbutton
               if (CoolerFilter)
               {
                   barCodeList = from bcl in barCodeList
                                 where !(lcList.Contains(bcl.LabID))
                                 select bcl;
               }        

                if ((string)cbYearFilter.SelectedValue != String.Empty)
                {
                    int selectedYear = Convert.ToInt32(cbYearFilter.SelectedValue);

                    barCodeList = barCodeList.Where(bc => bc.tblSample.DateCollected.Year == selectedYear);
                }

                if (!String.IsNullOrEmpty(txtBoxNumFilter.Text))
                {
                    string selectedBox = txtBoxNumFilter.Text;

                    barCodeList = barCodeList.Where(bc => bc.BoxNumber == selectedBox);
                }

            }

                // fixed to check for null value of innerexception 
                // bwitt Oct 25, 2016
            catch (System.Data.SqlClient.SqlException sex)
            {
                // catch error, log it and display to user bwitt 5 dec 2011
                string innerMessage = ""; 
                string outerMessage = sex.Message;
                if(sex.InnerException != null)
                    innerMessage = sex.InnerException.Message;
                Errors error = new Errors();
                error.LogErrors(outerMessage); // send to data base for record
                error.LogErrors(innerMessage);

                logErrors("Sql Exception outer message = " + outerMessage, "Possible data loss", 8); // notify user
                logErrors("Sql Exception inner message = " + innerMessage, "Possible data loss", 8);
            }

            // can remove this when debug is complete
            DateTime endTime = DateTime.Now;  // added to try to keep watch on time to read data base

            TimeSpan interval = endTime - startTime;

            string mins = interval.Minutes.ToString();
            string secs = interval.Seconds.ToString();
            string msg = string.Format("{2} Barcodes processed in {0} minutes and {1} seconds",
                mins, secs, records);

            MessageBox.Show(msg); 

            statLabel.Text = " Retrieving test results from the TevaICP database.";
            statusStrip1.Update();


            // Get the list of all elementLine data from the TevaICP database
            // XXXX this must be huge, or they are deleted at some point
            TevaICPDataContext tevaIcpCtx = new TevaICPDataContext();
            var elementList = from elem in tevaIcpCtx.ElementLines
                              where elem.ElementSymbol != "Y"
                              select new
                              {
                                  elementSymbol = elem.ElementSymbol,
                                  barCode = elem.Sample.Name,
                                  wavelength = elem.Wavelength,
                                  result = elem.AverageResult,
                                  failFlag = elem.FailFlags,
                                  checkFlag = elem.CheckFlag,
                                  anaDate = elem.Sample.AcquireDate,
                                  lineIndx = elem.LineIndex
                              };

            int multiplier = 100;

            ICPClientDataContext icpClientCtx = new ICPClientDataContext();

            int index = 0;
            int count = 0;
            foreach (var barCode in barCodeList)
            {
                if (index == 0)
                {
                    // Dynamically set the progress bar parameters
                    count = barCodeList.Count();
                    multiplier = resetProgressBar(count);
                    statLabel.Text = count + ": Validating test result data for fail flags and missing elements.";
                    statusStrip1.Update();
                }


                List<LocaltblIntermeditateICP> interICPList = new List<LocaltblIntermeditateICP>();
                var barCodeElements = from elem in elementList
                                      where elem.barCode == barCode.LabID
                                      select elem;

                foreach (var elem in barCodeElements)
                {
                    // Check to see if there is a modified local version of the element result data
                    // If there is use it instead of the values retrieved from the Teva database

                    IEnumerable<LocaltblIntermeditateICP> localVersion =
                            from locInter in icpClientCtx.LocaltblIntermeditateICPs
                            where locInter.BarCode == elem.barCode
                            && locInter.Element == elem.elementSymbol
                            && locInter.IndexLine == elem.lineIndx
                            select locInter;

                    if (localVersion.Count() == 1)
                    {
                        interICPList.Add(localVersion.FirstOrDefault());
                    }
                    else
                    {
                        LocaltblIntermeditateICP interICP = new LocaltblIntermeditateICP();
                        interICP.Anadate = elem.anaDate;
                        interICP.AverageResult = Convert.ToDecimal(elem.result.Value);
                        interICP.BarCode = elem.barCode;
                        interICP.CheckFlag = elem.checkFlag.Value.ToString();   // bwitt dec 6, 2011 changed to .value.tostring()
                        interICP.Element = elem.elementSymbol;
                        interICP.FailFlags = elem.failFlag;
                        interICP.Code = barCode.NumberSample;
                        interICP.Duplicate = barCode.Code;
                        interICP.Wavelength = elem.wavelength;
                        interICP.IndexLine = elem.lineIndx;
                        interICP.DeleteFlag = false;

                        interICPList.Add(interICP);
                    }
                }

                TempICP tempICP = new TempICP();
                tempICP.barCode = barCode.LabID;
                tempICP.code = barCode.NumberSample;
                tempICP.duplicate = barCode.Code;
                tempICP.boxNumber = barCode.BoxNumber;
                tempICP.sampleID = barCode.SampleID.Value;

                if (barCodeElements.Count() > 0)
                {
                    // validate each element
                    validateElements(tempICP, interICPList);

                    // check for missing elements
                    verifyElementsComplete(tempICP);

                    tempIcpList.Add(tempICP);
                }
                else
                {
                    noResultsIcpList.Add(tempICP);
                }


                if (++index % (multiplier) == 0)
                {
                    toolStripProgressBar1.PerformStep();
                }
            }

            statLabel.Text = "Validating pairs, blanks, and duplicates.";
            statusStrip1.Update();


            // reset the progress bar
            index = 0;
            multiplier = resetProgressBar(tempIcpList.Count());

            int barCodesWErrors = 0;
            foreach (TempICP icp in tempIcpList)  
            {
                validateICP(icp);
                if (icp.errorList.Count > 0)
                    barCodesWErrors++;

                if (++index % multiplier == 0)
                {
                    toolStripProgressBar1.PerformStep();
                }
            }

            // reset the status page
            panel1.Visible = true;
            statLabel.Text = string.Empty;
            statusStrip1.Update();
            toolStripProgressBar1.Value = 0;

            lblBarCodeCnt.Text = barCodesWErrors.ToString();
            lblSampleCnt.Text = tempIcpList.Count.ToString();

            lblElementCnt.Text = barCodeList.Count().ToString();

            btnExport.Enabled = true;
            btnStart.Enabled = false;

        }


        /// <summary>
        /// Handles the Click event of the btnClear control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            cbYearFilter.SelectedIndex = 0;
            txtBoxNumFilter.Text = String.Empty;

            //Empty the lists
            tempIcpList.Clear();
            noResultsIcpList.Clear();
            exportList.Clear();

            // Reset the controls on the Detail tab.
            tvBarCodes.Nodes[0].Nodes.Clear();
            clearDetails();

            // Reset the buttons
            btnExport.Enabled = true;
            btnStart.Enabled = true;
            btnExportData.Enabled = true;
        }


        /// <summary>
        /// Handles the Click event of the btnExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnExport_Click(object sender, EventArgs e)
        {
            exportList.Clear();
            warningList.Clear();

            int numSamples = 0;
            int numBarCodes = 0;

            tempIcpList.Sort(new CompareTempICP("Sample"));

            foreach (TempICP icp in tempIcpList)
            {
                if (icp.duplicate.Substring(0, 1) == "0" && icp.duplicate.Substring(1, 1) != "4")
                {
                    List<string> dups = new List<string>();
                    dups.Add("00");

                    bool hasErrors = icp.errorList.Count() > 0;
                    // check for errors
                    SortedList<string, TempICP> others = createSampleList(icp, false);

                    foreach (TempICP relatedBarCode in others.Values)
                    {
                        if (relatedBarCode.errorList.Count() > 0)
                        {
                            hasErrors = true;
                        }

                        if (relatedBarCode.duplicate.Substring(1, 1) == "0")
                        {
                            dups.Add(relatedBarCode.duplicate);
                        }
                    }

                    if (!hasErrors)
                    {

                        List<string> warnings = new List<string>();

                        numSamples++;
                        foreach (string dupValue in dups)
                        {
                            numBarCodes++;
                            TempICP tempIcp = (dupValue.Equals("00")) ? icp : others[dupValue];
                            LocalCooler lc = getCooler(tempIcp);
                            coolerList.Add(lc);

                            bool needPair = tempIcp.duplicate.Substring(1, 1) == "0";


                            tblInboundICP inICP = new tblInboundICP();
                            inICP.CODE = tempIcp.code;
                            inICP.DUPLICATE = tempIcp.duplicate;
                            inICP.ANADATE = tempIcp.anaDate;
                            inICP.Comments = tempIcp.comments;
                            inICP.DATE_SENT = DateTime.Now;
                            inICP.tblSampleID = tempIcp.sampleID;
                            inICP.COMPLETE = true;
                            inICP.Reviewed = false;

                            bool dissolved = (tempIcp.duplicate.Substring(1, 1) == "1" ||
                                    tempIcp.duplicate.Substring(1, 1) == "4");

                            foreach (string elem in tempIcp.elements.Keys)
                            {
                                setElementValue(inICP, elem, tempIcp.elements[elem], dissolved);
                            }


                            warnings.AddRange(tempIcp.warningList);

                            if (needPair)
                            {
                                numBarCodes++;
                                TempICP other = others[tempIcp.duplicate.Substring(0, 1) + "4"];
                                foreach (string elem in other.elements.Keys)
                                {
                                    setElementValue(inICP, elem, other.elements[elem], !dissolved);
                                }
                                warnings.AddRange(other.warningList);
                                LocalCooler otherCooler = getCooler(other);
                                coolerList.Add(otherCooler);
                            }

                            exportList.Add(inICP);
                        }

                        if (!warningList.ContainsKey(icp.code))
                        {
                            warningList.Add(icp.code, warnings);
                        }
                    }
                }
            }

            dgvExportResults.AutoGenerateColumns = false;
            dgvExportResults.DataSource = exportList;

            lblExportSampleCntValue.Text = numSamples.ToString();
            lblExportBarcodeCnt.Text = "This export contains results from " + numBarCodes.ToString() + " barcodes.";

            btnExport.Enabled = false;  // do not allow a reselect of the export without first clearing

            tabICP.SelectTab(tabExport);

        }


        /// <summary>
        /// Create a Local Cooler object from the passed in TempICP object
        /// </summary>
        /// <param name="icp">The icp.</param>
        private LocalCooler getCooler(TempICP icp)
        {
            LocalCooler coolerItem = new LocalCooler();
            coolerItem.BarCode = icp.barCode;
            coolerItem.CODE = icp.code;
            coolerItem.DUPLICATE = icp.duplicate;
            coolerItem.ANADATE = icp.anaDate;
            coolerItem.COMPLETE = true;
            coolerItem.DATE_SENT = DateTime.Now;
            coolerItem.Reviewed = false;
            coolerItem.tblSampleID = icp.sampleID;
            coolerItem.Bottle_Complete = true;
            coolerItem.Cooler = true;

            bool dissolved = (icp.duplicate.Substring(1, 1) == "1" ||
                    icp.duplicate.Substring(1, 1) == "4");

            foreach (string elem in icp.elements.Keys)
            {
                setCoolerElementValue(coolerItem, elem, icp.elements[elem], dissolved);
            }

            return coolerItem;
        }


        /// <summary>
        /// Setter method for assigning element attributes in an inboundICP object.
        /// </summary>
        /// <param name="inboundICP">The inbound ICP.</param>
        /// <param name="symbol">The symbol.</param>
        /// <param name="result">The result.</param>
        /// <param name="dissolved">if set to <c>true</c> [dissolved].</param>
        private void setElementValue(tblInboundICP inboundICP, string symbol,
                decimal? result, bool dissolved)
        {

            switch (symbol)
            {
                case "Al":
                    if (dissolved)
                        inboundICP.AL_D = result;
                    else
                        inboundICP.AL_T = result;
                    break;
                case "As":
                    if (dissolved)
                        inboundICP.AS_D = result;
                    else
                        inboundICP.AS_T = result;
                    break;
                case "Ca":
                    if (dissolved)
                        inboundICP.CA_D = result;
                    else
                        inboundICP.CA_T = result;
                    break;
                case "Cd":
                    if (dissolved)
                        inboundICP.CD_D = result;
                    else
                        inboundICP.CD_T = result;
                    break;
                case "Cu":
                    if (dissolved)
                        inboundICP.CU_D = result;
                    else
                        inboundICP.CU_T = result;
                    break;
                case "Fe":
                    if (dissolved)
                        inboundICP.FE_D = result;
                    else
                        inboundICP.FE_T = result;
                    break;
                case "K":
                    if (dissolved)
                        inboundICP.K_D = result;
                    else
                        inboundICP.K_T = result;
                    break;
                case "Mg":
                    if (dissolved)
                        inboundICP.MG_D = result;
                    else
                        inboundICP.MG_T = result;
                    break;
                case "Mn":
                    if (dissolved)
                        inboundICP.MN_D = result;
                    else
                        inboundICP.MN_T = result;
                    break;
                case "Na":
                    if (dissolved)
                        inboundICP.NA_D = result;
                    else
                        inboundICP.NA_T = result;
                    break;
                case "Pb":
                    if (dissolved)
                        inboundICP.PB_D = result;
                    else
                        inboundICP.PB_T = result;
                    break;
                case "Se":
                    if (dissolved)
                        inboundICP.SE_D = result;
                    else
                        inboundICP.SE_T = result;
                    break;
                case "Zn":
                    if (dissolved)
                        inboundICP.ZN_D = result;
                    else
                        inboundICP.ZN_T = result;
                    break;
            }
        }


        /// <summary>
        /// Setter method for assigning element attributes in an LocalCooler object.
        /// </summary>
        /// <param name="cooler">The LocalCooler object.</param>
        /// <param name="symbol">The symbol.</param>
        /// <param name="result">The result.</param>
        /// <param name="dissolved">if set to <c>true</c> [dissolved].</param>
        private void setCoolerElementValue(LocalCooler cooler, string symbol,
        decimal? result, bool dissolved)
        {

            switch (symbol)
            {
                case "Al":
                    if (dissolved)
                        cooler.AL_D = result;
                    else
                        cooler.AL_T = result;
                    break;
                case "As":
                    if (dissolved)
                        cooler.AS_D = result;
                    else
                        cooler.AS_T = result;
                    break;
                case "Ca":
                    if (dissolved)
                        cooler.CA_D = result;
                    else
                        cooler.CA_T = result;
                    break;
                case "Cd":
                    if (dissolved)
                        cooler.CD_D = result;
                    else
                        cooler.CD_T = result;
                    break;
                case "Cu":
                    if (dissolved)
                        cooler.CU_D = result;
                    else
                        cooler.CU_T = result;
                    break;
                case "Fe":
                    if (dissolved)
                        cooler.FE_D = result;
                    else
                        cooler.FE_T = result;
                    break;
                case "K":
                    if (dissolved)
                        cooler.K_D = result;
                    else
                        cooler.K_T = result;
                    break;
                case "Mg":
                    if (dissolved)
                        cooler.MG_D = result;
                    else
                        cooler.MG_T = result;
                    break;
                case "Mn":
                    if (dissolved)
                        cooler.MN_D = result;
                    else
                        cooler.MN_T = result;
                    break;
                case "Na":
                    if (dissolved)
                        cooler.NA_D = result;
                    else
                        cooler.NA_T = result;
                    break;
                case "Pb":
                    if (dissolved)
                        cooler.PB_D = result;
                    else
                        cooler.PB_T = result;
                    break;
                case "Se":
                    if (dissolved)
                        cooler.SE_D = result;
                    else
                        cooler.SE_T = result;
                    break;
                case "Zn":
                    if (dissolved)
                        cooler.ZN_D = result;
                    else
                        cooler.ZN_T = result;
                    break;
            }
        }


        /// <summary>
        /// Resets the progress bar.
        /// </summary>
        /// <param name="cnt">The count used to initialize the progress bar.</param>
        private int resetProgressBar(int cnt)
        {
            int retVal = 1;

            toolStripProgressBar1.Step = 1;
            toolStripProgressBar1.Value = 0;

            for (; cnt >= 1000; cnt /= 2)
            {
                retVal *= 2;
            }
            toolStripProgressBar1.Maximum = cnt;

            return retVal;
        }


        /// <summary>
        /// Verifies that the passed in icp bar code has results for all thirteen elements.
        /// </summary>
        /// <param name="tempICP">The temp ICP.</param>
        /// <returns></returns>
        private void verifyElementsComplete(TempICP tempICP)
        {
            if (tempICP.elements.Count != elementTypes.Length)
            {
                foreach (string elem in elementTypes)
                {
                    if (!tempICP.elements.Keys.Contains(elem))
                    {
                        tempICP.warningList.Add(tempICP.code + ":  " + tempICP.duplicate + ":  " +
                            elem + ":  Missing test results.");
                    }
                }
            }
        }


        /// <summary>
        /// Creates a listing of associated bar codes that belong to the same sample.
        /// </summary>
        /// <param name="icp">The icp.</param>
        /// <param name="warn">if set to <c>true</c> [warn].</param>
        /// <returns></returns>
        private SortedList<string, TempICP> createSampleList(TempICP icp, bool warn)
        {
            IEnumerable<TempICP> othersInSample =
                    from other in tempIcpList
                    where other.code == icp.code
                    && other.barCode != icp.barCode
                    select other;


            SortedList<string, TempICP> others = new SortedList<string, TempICP>();

            foreach (TempICP other in othersInSample)
            {
                if (others.ContainsKey(other.duplicate))
                {
                    if (warn)
                    {
                        icp.warningList.Add(icp.code + ":   Sample contains multiple barcodes with duplicate = " + other.duplicate);
                    }
                }
                else
                {
                    others.Add(other.duplicate, other);
                }
            }

            return others;
        }


        /// <summary>
        /// Validates the ICP record.
        /// </summary>
        /// <param name="tempICP">The temp ICP.</param>
        private void validateICP(TempICP icp)
        {
            SortedList<string, TempICP> others = createSampleList(icp, true);

            switch (icp.duplicate)
            {

                case "00":
                    if (others.ContainsKey("04"))
                    {
                        validateDissolvedTotal(icp, others["04"]);
                    }
                    else
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '04' pair.");
                    }
                    break;

                case "01":
                    break;

                case "03":
                    break;

                case "04":
                    if (!others.ContainsKey("00"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '00' pair.");
                    }
                    break;

                case "10":
                    if (!others.ContainsKey("14"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '14' pair.");
                    }
                    if (!others.ContainsKey("00"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '00' normal.");
                    }
                    validateBlankElements(icp);
                    break;

                case "11":
                    if (!others.ContainsKey("01"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '01' normal.");
                    }
                    validateBlankElements(icp);
                    break;

                case "13":
                    if (!others.ContainsKey("03"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '03' normal.");
                    }
                    validateBlankElements(icp);
                    break;

                case "14":
                    if (!others.ContainsKey("10"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '10' pair.");
                    }
                    if (!others.ContainsKey("04"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '04' normal.");
                    }
                    validateBlankElements(icp);
                    break;

                case "20":
                    if (!others.ContainsKey("24"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '24' pair.");
                    }
                    if (others.ContainsKey("00"))
                    {
                        validateDupElements(icp, others["00"]);
                    }
                    else
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '00' normal.");
                    }
                    break;

                case "21":
                    if (others.ContainsKey("01"))
                    {
                        validateDupElements(icp, others["01"]);
                    }
                    else
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '01' normal.");
                    }
                    break;

                case "23":
                    if (others.ContainsKey("03"))
                    {
                        validateDupElements(icp, others["03"]);
                    }
                    else
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '03' normal.");
                    }
                    break;

                case "24":
                    if (!others.ContainsKey("20"))
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '20' pair.");
                    }
                    if (others.ContainsKey("04"))
                    {
                        validateDupElements(icp, others["04"]);
                    }
                    else
                    {
                        icp.errorList.Add(icp.barCode + ":   Missing matching '04' normal.");
                    }
                    break;

                default:
                    break;
            }
        }

        #endregion

        private void btnCancelExport_Click(object sender, EventArgs e)
        {
            // who knows what was supposed to happen here... 
            // bwitt Oct 25, 2016
        }


        // XXXX lots of db hits here... 
        // checking for existance of barcode in data base. 
        // should resolve to one call to server, with bar code(s)
        // could return a text messsge as this does
        // or just a flag
        // this is a text box that on each key stroke seems to do a db query

        private void txtBarcodeToCheck_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;

            lblVerifyResults1.Text = string.Empty;
            lblVerifyResults2.Text = string.Empty;

            if (tb != null && !string.IsNullOrEmpty(tb.Text))
            {
                RiverwatchDataContext rwCtx = new RiverwatchDataContext();

                // Check to see if the barCode exists in the Riverwatch Database
                // seems to be assumption that there can be a barcode in metalbarcodes that is not associated with sample table
                // XXXX does not occure at this time
                // Perhaps we should make this uppercase for compare
                IEnumerable<tblMetalBarCode> barCodeExistsList =
                    from bc in rwCtx.tblMetalBarCodes
                    join samp in rwCtx.tblSamples
                    on bc.SampleID equals samp.SampleID
                    where bc.LabID.Equals(tb.Text.Trim())
                    select bc;

                // XXXX does this only run one time and break?
                // should the be an if statement  
                // looks like there will only be one returned result, so it is moot but may save printing same data twice if query is bad
                foreach (var barCode in barCodeExistsList)
                {
                    lblVerifyResults1.ForeColor = System.Drawing.Color.Black;
                    lblSampNumValue.Text = barCode.SampleNumber;
                    lblCodeValue.Text = barCode.Code;
                    lblBoxNumValue.Text = barCode.BoxNumber;
                    lblVerifyResults1.Text = barCode.LabID + ": Barcode exists in the Riverwatch Database.";
                    break;
                }


                // XXXX Check to see if the barCode already has sample associated with it
                // this uses input from the text box
                // seems to only want barcodes that have collection method = 6
                // and assumes metal results 
                IEnumerable<tblMetalBarCode> barCodeHasResultsList =
                    from bc in rwCtx.tblMetalBarCodes
                    join samp in rwCtx.tblSamples
                    on bc.SampleID equals samp.SampleID
                    where bc.LabID.Equals(tb.Text.Trim())
                    && (from cs in rwCtx.tblChemSamps
                        where cs.CollMeth == 6
                        select cs.SampleID).Distinct().Contains(bc.SampleID.Value)
                    select bc;

                foreach (var barCode in barCodeHasResultsList)
                {
                    lblVerifyResults2.ForeColor = System.Drawing.Color.Black;
                    lblSampNumValue.Text = barCode.SampleNumber;
                    lblCodeValue.Text = barCode.Code;
                    lblBoxNumValue.Text = barCode.BoxNumber;
                    lblVerifyResults2.Text = barCode.LabID + ": Barcode has metal results in Riverwatch DB.";
                    break;
                }


                // Check to see if the barCode is in the InboundICP table
                // XXXX these queries are really overkill, 
                // we just want to know if this be is in the inbound table. 
                // this will be slightly different since we will have the icp final table and we can just check it.
                IEnumerable<tblMetalBarCode> barCodeExistsInboundICP =
                    from bc in rwCtx.tblMetalBarCodes
                    join samp in rwCtx.tblSamples
                    on bc.SampleID equals samp.SampleID
                    where bc.LabID.Equals(tb.Text.Trim())
                    && (from inICP in rwCtx.tblInboundICPs
                        select inICP.tblSampleID).Distinct().Contains(bc.SampleID.Value)
                    select bc;

                foreach (var barCode in barCodeExistsInboundICP)
                {
                    lblVerifyResults2.ForeColor = System.Drawing.Color.Black;
                    lblSampNumValue.Text = barCode.SampleNumber;
                    lblCodeValue.Text = barCode.Code;
                    lblBoxNumValue.Text = barCode.BoxNumber;
                    lblVerifyResults2.Text = barCode.LabID + ": Barcode has metal results in InboundICP table.";
                    break;
                }

                if (string.IsNullOrEmpty(lblVerifyResults1.Text))
                {
                    lblVerifyResults1.ForeColor = System.Drawing.Color.Red;
                    lblVerifyResults1.Text = "BARCODE DOES NOT EXIST IN RIVERWATCH";
                }
                else if (string.IsNullOrEmpty(lblVerifyResults2.Text))
                {
                    lblVerifyResults2.ForeColor = System.Drawing.Color.Red;
                    lblVerifyResults2.Text = "Barcode does not have results in Riverwatch DB";
                }

            }
        }

        // bwitt added dec 6, 2011 to help track down possible time out errors from sql server or any other events
        public void logErrors(string msg, string severityDescription, int severityCode)
        {
            // save to data base:
            ICPClientDataContext icpX = new ICPClientDataContext();
            Log l = new Log();
            l.Date = DateTime.Now;
            if (severityDescription.Length > 0)
                l.Severity = severityDescription;
            else
                l.Severity = "unknown";
            l.Msg = msg;
            l.SeverityCode = severityCode;

            icpX.Logs.InsertOnSubmit(l);    // insert into context
            icpX.SubmitChanges(); // update data base
        }

        public void displayErrors(string msg, string severityDescription, int severityCode)
        {
            //  write to popup window for user

            formError fe = new formError();
            var T = fe.Controls.Find("tbErrors", true);
            T[0].Text = msg + Environment.NewLine + severityDescription + Environment.NewLine + severityCode.ToString();
            fe.ShowDialog();
        }

        // now used as cooler filter switch
        private void btnCoolerFilter_Click(object sender, EventArgs e)
        {
            CoolerFilter = !CoolerFilter;
            if (CoolerFilter)
            {
                btnCoolerFilter.Text = "Cooler Filter ON";
                btnCoolerFilter.BackColor = Color.CadetBlue;
            }
            else
            {
                btnCoolerFilter.Text = "Cooler Filter OFF";
                btnCoolerFilter.BackColor = Color.White; 
            }
        }

        /// <summary>
        /// Comparer class for tblInboundICP objects
        /// </summary>
        public class CompareTblInboundICP : IComparer<tblInboundICP>
        {
            private string compareBy = "Sample";

            public CompareTblInboundICP(string cBy)
            {
                compareBy = cBy;
            }

            // XXXX Not sure we care, but this seems wierd
            public int Compare(tblInboundICP icp1, tblInboundICP icp2)
            {
                int retVal = 0;

                switch (compareBy)
                {
                    case "Sample":
                        //                    retVal = string.Compare(icp1.CODE, icp2.CODE);
                        double diff = Convert.ToDouble(icp1.CODE) - Convert.ToDouble(icp2.CODE);
                        retVal = (diff == 0) ? string.Compare(icp1.DUPLICATE, icp2.DUPLICATE) :
                                 (diff > 0) ? 1 : -1;
                        break;
                    default:
                        retVal = 0;
                        break;
                }
                return retVal;
            }
        }
    }
}
