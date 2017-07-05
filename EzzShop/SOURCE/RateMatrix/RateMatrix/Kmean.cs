using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RateMatrix
{
    public partial class Kmean : Form
    {
        KmeanClustering kmen = new KmeanClustering();
        public Kmean()
        {
            InitializeComponent();
            txtResult.Text = kmen.ExcuteClustering();
        }
    }
}
