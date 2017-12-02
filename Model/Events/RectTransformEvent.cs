using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public enum Curs { SizeNESW, SizeNS, SizeNWSE, SizeWE, Hand, Default };

    public class RectTransformEvent : EventArgs
    {
        public string TypeCursors { set; get; }
        public RectTransformEvent(Curs c)
        {
            TypeCursors = c.ToString();
        }
    }
}
