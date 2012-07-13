using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace SolrKit
{
    public partial class MappingCode : Form
    {
        public MappingCode(string code)
        {
            InitializeComponent();
            this.txtMappingCode.Text = code;
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtMappingCode.Text);
            txtMappingCode.BackColor = Color.LightGoldenrodYellow;
        }
    }
}
