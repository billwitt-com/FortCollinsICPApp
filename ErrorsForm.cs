﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ICPClientLinq
{
    public partial class formError : Form
    {
        public formError()
        {
            InitializeComponent();
        }

        private void btnCloseErrorForm_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void btnPrintErrorForm_Click(object sender, EventArgs e)
        {
            // do later, if at all
            
            
        }
    }
}
