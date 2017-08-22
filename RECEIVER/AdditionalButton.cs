using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MataDewa4
{
    public partial class AdditionalButton : Form
    {
        public AdditionalButton()
        {
            InitializeComponent();
        }
       
        private void Additional_btnArming_Click(object sender, EventArgs e)
        {
            Program.m_MainInstance.ExternalBtn_Click((Int32) c_EXTERNAL_BUTTON.Arming);
        }

        private void Additional_btnTakeOFf_Click(object sender, EventArgs e)
        {
            Program.m_MainInstance.ExternalBtn_Click((Int32) c_EXTERNAL_BUTTON.TakeOff);
        }

        private void Additional_btnParachute_Click(object sender, EventArgs e)
        {
            Program.m_MainInstance.ExternalBtn_Click((Int32) c_EXTERNAL_BUTTON.Parachute);
        }

        private void Additional_btnDisArming_Click(object sender, EventArgs e)
        {
            Program.m_MainInstance.ExternalBtn_Click((Int32) c_EXTERNAL_BUTTON.Disarming);
        }
    }
}
