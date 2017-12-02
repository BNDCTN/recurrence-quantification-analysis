using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Model
{
    [Serializable]
    public class Project
    {
        public String Name { get; set; }
        public String Path { get; set; }
        
        public float Epsilon { get;set; }
        public float[] Row { get; set; }
    }
}
