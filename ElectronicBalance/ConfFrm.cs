using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectronicBalance
{
    public partial class ConfFrm : Form
    {
        public ConfFrm()
        {
            InitializeComponent();
        }

        private void CfgFrm_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cbCom.Items.AddRange(ports);
            cbCom.SelectedItem=CommonConf.Instance.Com;
            tbPort.Text = CommonConf.Instance.Port.ToString();
        }

        private void CfgFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CommonConf.Instance.Com = cbCom.SelectedItem.ToString();
            if (Int32.TryParse(tbPort.Text, out int port))
            {
                CommonConf.Instance.Port = port;
            }
            CommonConf.Instance.SaveConf();
        }
    }
}
