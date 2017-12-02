using System;
using System.Drawing;

namespace Model
{
    public class EventCutFace : EventArgs
    {
        public Bitmap Btmap { set; get; }
        public bool Cut { set; get; }


        public EventCutFace(Bitmap bm, bool cut)
        {
            Btmap = bm;
            Cut = cut;
        }
    }
}
