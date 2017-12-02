using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface ITimeRow
    {
        float[] Row { get; set; }
        float this[int i] { get; set; }

        int GetSize();
        void GenerateRandomRow(int size, int c);
    }

    public class TimeRow: ITimeRow, IEnumerable, IDisposable
    {
        private float[] row;

        bool disposed = false;
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        #region Властивості
        public float[] Row
        {
            set { this.row = value; }
            get { return this.row; }
        }

        public float this[int i]
        {
            set { this.row[i] = value; }
            get { return this.row[i]; }
        }
#endregion

#region Конструктори
        public TimeRow() { }      
        public TimeRow(float[] row)
        {
            this.row = row;
        }
        public TimeRow(int size, int c)
        {
            GenerateRandomRow(size, c);
        }
        #endregion

        #region Методи
        public int GetSize()
        {
            return row.Length;
        }

        public void GenerateRandomRow(int size, int c)
        {
            Random rand = new Random();
            float[] randArray = new float[size];
            float[] timeRow = new float[size];
            for (int i = 0; i < size; i++)
                randArray[i] = (float)(rand.NextDouble());

            var min = randArray.Min();
            for (int i = 0; i < size; i++)
                timeRow[i] = (randArray[i] + Math.Abs(min)) * c;

            this.row = timeRow;
            randArray = null;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var element in row)
            yield return element;
        }

        #endregion

        #region Destructor
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
            }

            disposed = true;
        }
        #endregion

    }
}
