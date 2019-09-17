using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ElectronicBalance
{
    public partial class MainFrm : Form
    {
        public MainFrm()
        {
            InitializeComponent();
        }

        private void MainFrm_Load(object sender, EventArgs e)
        {
            Manager.Instance.Init();
        }

        private void DataReceived(string data)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => { this.tbValue.Text = data; }));
                }
                else
                {
                    this.tbValue.Text = data;
                }
            }
            catch
            {

            }
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void btnCfg_Click(object sender, EventArgs e)
        {
            ConfFrm dlg = new ConfFrm();
            dlg.ShowDialog();
        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.btnSwitch.Text == "开启")
                {
                    Manager.Instance.DataReceived += DataReceived;
                    Manager.Instance.Start();
                    this.btnSwitch.Text = "关闭";
                }
                else
                {
                    Manager.Instance.Stop();
                    this.tbValue.Text = "";
                    this.btnSwitch.Text = "开启";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
