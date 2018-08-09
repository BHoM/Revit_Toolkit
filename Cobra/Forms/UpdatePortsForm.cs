using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BH.UI.Cobra.Forms
{
    public partial class UpdatePortsForm : Form
    {
        public UpdatePortsForm()
        {
            InitializeComponent();
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            RevitListener listener = RevitListener.Listener;

            int inPort = (int)Math.Round(InputPort.Value);
            int outPort = (int)Math.Round(OutputPort.Value);

            if (inPort == outPort || inPort < 3000 || inPort > 65000 || outPort < 3000 || outPort > 65000)
            {
                MessageBox.Show("Input port and output port must have values between 3000 and 65000 and can not be the same", "Port number error");
                return;
            }

            listener.SetPorts(inPort, outPort);
            this.Close();

        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
