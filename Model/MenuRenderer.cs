using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Model
{
    public class MenuRenderer : ToolStripProfessionalRenderer {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected)
            {
                base.OnRenderMenuItemBackground(e);
                e.Item.ForeColor = Color.SlateGray;
                e.Item.BackColor = SystemColors.InactiveCaption;
            }
            else
            {
                Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
                Brush brush = new SolidBrush(Color.LightSlateGray);
                Pen pen = new Pen(SystemColors.InactiveCaption);
                e.Graphics.FillRectangle(brush, rc);
                e.Graphics.DrawRectangle(pen, 1, 0, rc.Width - 2, rc.Height - 1);

                e.Item.ForeColor = SystemColors.InactiveCaption;
                e.Item.BackColor = SystemColors.InactiveCaption;
            }
        }
    }
}
