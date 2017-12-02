using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Model
{
    public delegate void RectagleTransform(Object sender, RectTransformEvent cusor);

    public class CutSquare: IDisposable
    {

            /* Поля */
            protected Rectangle r;
            //protected CutForm Cutform;
            protected Bitmap varmap;
            protected SolidBrush boardPenBrush = new SolidBrush(Color.FromArgb(50, 0, 123, 143));
            protected Pen boardPen = new Pen(Color.FromArgb(50, 0, 123, 143), 1);
            protected SolidBrush ErrorboardPenBrush = new SolidBrush(Color.FromArgb(50, 150, 0, 0));
            protected Pen ErrorboardPen = new Pen(Color.FromArgb(50, 130, 0, 0), 1);
            protected Point rHold = new Point(0, 0);

            protected int sourceWidth;
            protected int sourceHeight;

            const int moveField = 15;

            bool disposed = false;

            /* Властивості */
            public int FixWidth { set; get; }
            public int FixHeight { set; get; }

            public int X
            {
                set { r.X = value; }
                get { return r.X; }
            }
            public int Y
            {
                set { r.Y = value; }
                get { return r.Y; }
            }
            public int Width
            {
                set { r.Width = value; }
                get { return r.Width; }
            }
            public int Height
            {
                set { r.Height = value; }
                get { return r.Height; }
            }

            public bool rTransformaing { get; private set; }                        //!!!!!!!!!!!!!!!!!!!!!!!!
            public bool rMoving { get; private set; }                               //!!!!!!!!!!!!!!!!!!!!!!!!
            public bool rCuttin { get; private set; }                               //!!!!!!!!!!!!!!!!!!!!!!!!

            /* Конструктори */
            public CutSquare() : this(0, 0) { }
            public CutSquare(int fixwidth, int fixheight)
            {
                FixWidth = fixwidth;
                FixHeight = fixheight;
                r = new Rectangle(0, 0, 0, 0);
                rCuttin = false;

                //Cutform = new CutForm();
                //Cutform.FaceCutted += Cutform_FaceCutted; 
            }
            public CutSquare(Point p)
            {
                r = new Rectangle(p, new Size(0,0));
                rCuttin = true;
            }
            public CutSquare(Point p, Bitmap bitmap)
            {
                varmap = bitmap;
                r = new Rectangle(p, new Size(0, 0));
                rCuttin = true;
        }
            public CutSquare(Bitmap bitmap) : this(new Point(0, 0))
            {
                varmap = bitmap;
                sourceWidth = bitmap.Width;
                sourceHeight = bitmap.Height;
            }

        /* Події */
        public event RectagleTransform CursorChange;
            public event Action<Object, Bitmap> CutFace;

            public void Cutform_FaceCutted(Object sender, EventCutFace evc)
            {
                if (evc.Cut && evc.Btmap != null)
                {                 // bool Cut = true; про всяк випадок перевірка Bitmap на NULL
                    if (CutFace != null)
                        CutFace(this, evc.Btmap);
                }
                rTransformaing = false;
                rCuttin = false;
                rMoving = false;
            }

            /* Методи */
            void Reset()
            {
                r.X = 0;
                r.Y = 0;
                r.Width = 0;
                r.Height = 0;
            }
            public void SetStartPoint(int x, int y)
            {
                r.X = x - FixWidth;
                r.Y = y - FixHeight;
            }
            public void SetSize(int width, int height)
            {
                if (r.Width + r.X <= sourceWidth && r.Height + r.Y <= sourceHeight && r.X >= 0 && r.Y >= 0) // лише для квадратного виділення
                {
                    r.Width = width - FixWidth;
                    r.Height = height - FixHeight;
                }

                if (r.Width > r.Height) r.Height = r.Width; else r.Width = r.Height;      // квадратне виділення

                rCuttin = true;
            }
            public bool InFocus(int eX, int eY, int fixW, int fixH)
            {
                if (eX > r.X + fixW + moveField && eX < r.X + r.Width - moveField + fixW && eY > r.Y + fixH + moveField && eY < r.Y + r.Height + fixH - moveField)
                {
                    rMoving = true;
                    rHold.X = eX - r.X;
                    rHold.Y = eY - r.Y;
                    if (CursorChange != null)
                        CursorChange(this, new RectTransformEvent(Curs.Hand));
                    return true;
                }
                else if ((eX > r.X + fixW && eX < r.X + moveField + fixW) ||
                         (eX > r.X + r.Width + fixW - moveField && eX < r.X + r.Width + fixW) ||
                         (eY > r.Y + fixH && eY < r.Y + fixH + moveField) ||
                         (eY > r.Y + r.Height + fixH - moveField && eY < r.Y + r.Height + fixH))
                {
                    rTransformaing = true;
                    rCuttin = true;
                    rHold.X = eX - r.X;
                    rHold.Y = eY - r.Y;
                    return true;
                }
                rMoving = false;
                rTransformaing = false;
                return false;
            }
            public void Move(int eX, int eY)
            {
                if (rMoving)
                {
                    r.X = eX - rHold.X;
                    r.Y = eY - rHold.Y;
                    if (CursorChange != null)
                        CursorChange(this, new RectTransformEvent(Curs.Default));
                    //Cutform.Invalidate();

                }
            }
            public void Transform(int eX, int eY)
            {
                if (rTransformaing)
                {
                    rCuttin = true;
                    if (eX > r.X + FixWidth && eX < r.X + moveField + FixWidth
                        && eY > r.Y + moveField + FixHeight && eY < r.Y + r.Height - moveField + FixHeight)
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.SizeWE));
                    }
                    else if (eX > r.X + r.Width - moveField + FixWidth && eX < r.X + r.Width + FixWidth
                        && eY > r.Y + moveField + FixHeight && eY < r.Y + r.Height - moveField + FixHeight)
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.SizeWE));
                    }

                    else if (eY > r.Y + FixHeight && eY < r.Y + moveField + FixHeight
                        && eX > r.X + moveField + FixWidth && eX < r.X + r.Width - moveField + FixWidth)
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.SizeNS));
                    }
                    else if (eY > r.Y + r.Height - moveField + FixHeight && eY < r.Y + r.Height + FixHeight
                        && eX > r.X + moveField + FixWidth && eX < r.X + r.Width - moveField + FixWidth)
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.SizeNS));
                    }


                    else if (eX > r.X + FixWidth && eX < r.X + moveField + FixWidth &&
                        eY > r.Y + FixHeight && eY < r.Y + moveField + FixHeight)
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.SizeNWSE));
                    }

                    else if (eX > r.X + r.Width - moveField + FixWidth && eX < r.X + r.Width + FixWidth &&
                        eY > r.Y + r.Height - moveField + FixHeight && eY < r.Y + r.Height + FixHeight)
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.SizeNWSE));
                    }





                    else if (eX > r.X + r.Width - moveField + FixWidth && eX < r.X + r.Width + FixWidth &&
                        eY > r.Y + FixHeight && eY < r.Y + moveField + FixHeight)
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.SizeNESW));
                    }

                    else if (eX > r.X + FixWidth && eX < r.X + moveField + FixWidth &&
                        eY > r.Y + r.Height - moveField + FixHeight && eY < r.Y + r.Height + FixHeight)
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.SizeNESW));
                    }


                    else
                    {
                        if (CursorChange != null)
                            CursorChange(this, new RectTransformEvent(Curs.Default));
                    }

                }
            }
            public Bitmap DrawRect(Bitmap source, Bitmap image)
            {
                if (!rCuttin && !rMoving) return source;
                varmap = image;
                sourceWidth = source.Width;
                sourceHeight = source.Height;

                    if (X < 0) X = 0;
                    if (Y < 0) Y = 0;
                    if (X + Width > sourceWidth) X = sourceWidth - Width;
                    if (Y + Height > sourceHeight) Y = sourceHeight - Height;

                Bitmap bitmap = new Bitmap(source);
                using (Graphics b = Graphics.FromImage(bitmap))
                {
                    b.Clear(Color.Black);
                    b.DrawImage(image, 0, 0);              // обов'язково

                    if (r.Width + r.X < sourceWidth+1 && r.Height + r.Y < sourceHeight+1 && r.X >= 0 && r.Y >= 0)
                    {
                        b.DrawRectangle(boardPen, r.X, r.Y, r.Width - 1, r.Height - 1);
                        b.FillRectangle(boardPenBrush, r.X, r.Y, r.Width, r.Height);
                    }
                    else
                    {
                        b.DrawRectangle(ErrorboardPen, r.X, r.Y, r.Width - 1, r.Height - 1);
                        b.FillRectangle(ErrorboardPenBrush, r.X, r.Y, r.Width, r.Height);                      
                    }
                }
                return bitmap;
            }
            public Bitmap DrawRect(Bitmap source)
            {
                return DrawRect(source, varmap);
            }
            public void HoldRect()
            {
                if (X < 0) X = 0;
                if (Y < 0) Y = 0;
                if (X + Width > sourceWidth) X = sourceWidth - Width;
                if (Y + Height > sourceHeight) Y = sourceHeight - Height;

                rCuttin = false;
                rMoving = false;
                rTransformaing = false;
            }
            public void ChartReflection(Chart chart, PictureBox pictureBox)
            {
                double xMin = chart.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                double xMax = chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
                double yMin = chart.ChartAreas[0].AxisY.ScaleView.ViewMinimum;
                double yMax = chart.ChartAreas[0].AxisY.ScaleView.ViewMaximum;
                double absoluteWidth = chart.ChartAreas[0].AxisX.Maximum - chart.ChartAreas[0].AxisX.Minimum;
                double absoluteHeight = chart.ChartAreas[0].AxisY.Maximum - chart.ChartAreas[0].AxisY.Minimum;

                rCuttin = true;
                this.SetStartPoint((int)(xMin / absoluteWidth * pictureBox.Width), (int)((absoluteHeight - yMax) / absoluteHeight * pictureBox.Width));
                this.SetSize((int)((xMax - xMin) / (absoluteWidth) * pictureBox.Width), (int)((yMax - yMin) / (absoluteHeight) * pictureBox.Height));
                pictureBox.Image = this.DrawRect(new Bitmap(pictureBox.Image), varmap);
                this.HoldRect();
            }
            public Bitmap Clean()
            {
                return varmap;
            }

            /* Деструктори */
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            protected void Dispose(bool disposing)
            {
                if (disposed) return;

                if (disposing)
                {


                }
                disposed = true;
            }
            ~CutSquare()
            {
                Dispose(false);
            }
        }
    }

