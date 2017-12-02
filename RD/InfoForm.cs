using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RD
{

    interface IInfoForm
    {
        void ShowInfo();
    }

    public partial class InfoForm : Form, IInfoForm
    {
        public InfoForm()
        {
            InitializeComponent();

            textLabel.Text = 
@" 
The recurrence quantification analysis (RQA) is a method of nonlinear
data analysis which quantifies the number and duration of recurrences
of a dynamical system presented by its state space trajectory.

This application is made by BNDCTN. 
";

            closeLabel.Click += CloseLabel_Click;

        }

        public void ShowInfo()
        {
            this.ShowDialog();
        }

        private void CloseLabel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
