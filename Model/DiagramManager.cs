using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Model
{
    public interface IDiagramManager
    {
        TimeRow Row { get; set; }
        float E { get; set; }
        bool[,] RecurrentMatrix { get; }

        float RR { get; }
        float DET { get; }
        float L { get; }
        float DIV { get; }
        float ENTR { get; }
        float RATIO { get; }

        IEnumerable<bool> Calculate();
        void GenerateReccurentMatrix();
        bool[,] GetReccurentMatrix();
        Bitmap GetReccurentDiagram();
        Bitmap GetDistanceDiagram();
        Image GetReccurentDiagram(int width, int height);
        Image GetDistanceDiagram(int width, int height);
        Image GetDistancePalette(int width);
  
    }

    public class DiagramManager: IDiagramManager, IDisposable
    {
        private TimeRow row;
        private float e = 0;
        private bool[,] resultMatrix;

        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        #region Властивості
        public TimeRow Row
        {
            set { this.row = value; }
            get { return this.row; }
        }
        public float E
        {
            set { this.e = value; }
            get { return this.e; }
        }     
        public bool[,] RecurrentMatrix
        {
            get { return this.resultMatrix; }
        }

        List<int> Pl = new List<int>();
        Color[] DistancePalette
        {
            get
            {
                return new Color[]
              {
                  Color.FromArgb(0, 0, 255),
                  Color.FromArgb(0, 100, 200),
                  Color.FromArgb(0, 150, 150),
                  Color.FromArgb(0, 200, 100),
                  Color.FromArgb(150, 255, 50),
                  Color.FromArgb(200, 255, 0),
                  Color.FromArgb(255, 255, 0),
                  Color.FromArgb(255, 200, 0),
                  Color.FromArgb(255, 100, 0),
                  Color.FromArgb(255, 0, 0)    //червоний - max
              };
            }
        }

        int AmountOfPoints { get; set; }
        int AmountOfLines { get; set; }

        public float RR { get; private set; }
        public float DET { get; private set; }
        public float L { get; private set; }
        public float DIV { get; private set; }
        public float ENTR { get; private set; }
        public float RATIO { get; private set; }
        #endregion

        #region Конструктори
        public DiagramManager() { }
        public DiagramManager(TimeRow row)
        {
            this.row = row;
        }
        public DiagramManager(TimeRow row, float e)
        {
            this.row = row;
            this.e = e;
        }
        #endregion

        #region Методи

        public IEnumerable<bool> Calculate()
        {
            bool res = false;
            for (short i = 0; i < row.GetSize()-1; i++)
                for (short j = 0; j < row.GetSize()-1; j++)
                {
                    res = ((Math.Sqrt(Math.Pow((row[i] - row[j]), 2) + Math.Pow((row[i+1] - row[j+1]), 2)) < e) ? true : false);
                    yield return res;
                }
        }
        public void GenerateReccurentMatrix()
        {
            if (row.GetSize() < 2) throw new Exception("Для побудови матриці рекурентності, часовий ряд повинен містити більше, ніж одне значення.");
            int size = row.GetSize() - 1;
            int i = 0;
            int j = 0;
            AmountOfPoints = 0;
            resultMatrix = new bool[size, size];
            foreach (var d in Calculate())
            {
                if (i == size)
                {
                    i = 0;
                    j++;
                }
                resultMatrix[i++, j] = d;
                if (d == true) AmountOfPoints++;
            }
            RQA(2);
        }
        public bool[,] GetReccurentMatrix()
        {
            if (resultMatrix == null) throw new Exception("Рекурентна матриця ще не побудована");
            return resultMatrix;
        }        
        public Image GetReccurentDiagram(int width, int height)
        {
            Bitmap b = GetReccurentDiagram();
            Image image = b.DiscretImage(width, height);
            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return image;
        }
        public Image GetDistanceDiagram(int width, int height)
        {
            Bitmap b = GetDistanceDiagram();
            b.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return b;
        }
        public Image GetDistancePalette(int width)
        {
            Bitmap b = new Bitmap(width, DistancePalette.Length);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < DistancePalette.Length; j++) 
                    b.SetPixel(i, j, DistancePalette[DistancePalette.Length-1-j]);
            return b;
        }
        public Bitmap GetReccurentDiagram()
        {
            int size = row.GetSize() - 1;
            Bitmap b = new Bitmap(row.GetSize(), row.GetSize());

            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);

            for (int i = 1; i <= size; i++)
                for (int j = 1; j <= size; j++)
                    if (resultMatrix[i - 1, j - 1] == true) b.SetPixel(i, j, Color.Black);
                    else b.SetPixel(i, j, Color.White);
            //b.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return b;
        }
        public Bitmap GetDistanceDiagram()
        {
            int size = row.GetSize() - 1;
            Bitmap b = new Bitmap(row.GetSize(), row.GetSize());
            float res = 0;
            float max = 0;
            float min = 0;
            float[,] distance = new float[size, size];
            for (short i = 0; i < size; i++)
                for (short j = 0; j < size; j++)
                {
                    distance[i,j] = (float)(Math.Sqrt(Math.Pow((row[i] - row[j]), 2) + Math.Pow((row[i + 1] - row[j + 1]), 2)));
                    if (distance[i, j] > max) max = distance[i, j];
                    if (distance[i, j] < min) min = distance[i, j];
                }
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);
            float interval = max - min;
            Color[] Palette = DistancePalette.Reverse().ToArray();
            float step = (float)1 / DistancePalette.Length;
            for (short i = 0; i < size; i++)
                for (short j = 0; j < size; j++)
                {
                    if (distance[i, j] >= 0 && distance[i, j] < min + interval * step) b.SetPixel(i, j, Palette[Palette.Length - 1]);
                    for (double p = 0; p <= 1; p += step)
                    {
                        if (distance[i, j] >= min + interval * step && distance[i, j] < min + interval * (step + p))
                        {
                            b.SetPixel(i, j, Palette[(byte)(Palette.Length - (p * Palette.Length))]);
                            break;
                        }
                    }
                }
            return b;
        }

        void RQA(int lmin)
        {
            Pl.Clear();
            //Pl.Add(Row.GetSize() - 1);
            int points = 0;
            int linepoints = 0;
            for (short route = 1; route < Row.GetSize() - 1; route++)
            {
                for (short j = 0; j < Row.GetSize() - 1; j++)
                    for (int i = j + route; i < Row.GetSize() - 1; i++)
                    {
                        if (RecurrentMatrix[i, j] == true) points++;
                        else
                        {
                            if (points >= lmin)
                            {
                                Pl.Add(points);
                                Pl.Add(points);
                                AmountOfLines++;
                                linepoints += points;
                            }

                            points = 0;
                        }
                        j++;
                    }
                if (points >= lmin)
                {
                    Pl.Add(points);
                    Pl.Add(points);
                    AmountOfLines++;
                    linepoints += points;
                }
                points = 0;
            }
            Pl.Sort();
            //List<int> PlUniq = Pl.Distinct().ToList<int>();
            Dictionary<int, int> HashPl = new Dictionary<int, int>();
            for (int i=0; i< Pl.Count; i++)
            {
                if (HashPl.Keys.Contains(Pl[i])) HashPl[Pl[i]]++;
                else HashPl.Add(Pl[i],1);
            }
            List<int> PlUniq = HashPl.Keys.ToList();
            float linePoints = linepoints + linepoints;// + Row.GetSize() - 1;
            AmountOfPoints -= Row.GetSize() - 1;
            RR = (float)AmountOfPoints / (RecurrentMatrix.GetLength(0) * RecurrentMatrix.GetLength(1));
            DET = AmountOfPoints> Row.GetSize() - 1 ? linePoints / AmountOfPoints : 0;

            if (Pl.Count > 0) {
                L = linePoints / Pl.Count;
                DIV = 1 / (float)Pl.Last();
                RATIO = DET / RR;
            }
            else
            {
                L = 0;
                DIV = 0;
                RATIO =0;
            }
            //L = (Pl.Count > 1) ? linePoints / Pl.Count:0;
            //DIV = Pl.Count>1 ? (1 / (float)Pl.Count): 0;
            ENTR = 0.0F;//(float)(2 / 9 * Math.Log10((double)2 / 9) + 3 / 9 * Math.Log10((double)3 / 9) + 19 / 9 * Math.Log10((double)19 / 9)) * (-1);
            float temp = 0;
            for (short i = 0; i < HashPl.Count; i++)
            {
                temp = (float)HashPl[PlUniq[i]] / Pl.Count;
                ENTR += temp * (float)Math.Log10(temp);
            }

            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            GC.Collect();
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                row.Dispose();
            }

            disposed = true;
        }
        #endregion
    }
}
