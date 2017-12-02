using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Model
{
    public static class ChartExtention
    {
        public static Chart Copy(this Chart current)
        {
            MemoryStream stream = new MemoryStream();
            //Chart copy = current;
            current.Serializer.Save(stream);
            Chart copy = new Chart();
            copy.Serializer.Load(stream);
            return copy;
        }
        public static void Reset(this Chart chart)
        {
            double aboluteXmax = chart.ChartAreas[0].AxisX.Maximum;
            double aboluteYmax = chart.ChartAreas[0].AxisY.Maximum;
            double viewXmax = chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
            double viewYmax = chart.ChartAreas[0].AxisY.ScaleView.ViewMaximum;
            if (aboluteXmax > viewXmax || viewYmax < aboluteYmax)
            {
                chart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                chart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            }
            chart.Series[0].MarkerSize = chart.ChartAreas[0].AxisX.ScaleView.Size > 99 ? 2 : (int)(chart.Width / chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum * 0.9);
        }
        public static Chart Zoom(this Chart chart, MouseEventArgs e)
        {
            try
            {
                int markerSize = chart.ChartAreas[0].AxisX.ScaleView.Size > 99 ? 2 : (int)(chart.Width / chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum * 0.5);

                if (e.Delta < 0)
                {
                    chart.Reset();
                }
                else if (e.Delta > 0)
                {
                    double xMin = chart.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                    double xMax = chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
                    double yMin = chart.ChartAreas[0].AxisY.ScaleView.ViewMinimum;
                    double yMax = chart.ChartAreas[0].AxisY.ScaleView.ViewMaximum;

                    double posXStart = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 3;
                    double posXFinish = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 3;
                    double posYStart = chart.ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 3;
                    double posYFinish = chart.ChartAreas[0].AxisY.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 3;

                    double start = posXStart > posYStart ? posXStart : posYStart;
                    double finish = posXFinish > posYFinish ? posXFinish : posYFinish;

                    chart.ChartAreas[0].AxisX.ScaleView.Zoom(start, finish);
                    chart.ChartAreas[0].AxisY.ScaleView.Zoom(start, finish);
                    markerSize = chart.ChartAreas[0].AxisX.ScaleView.Size > 99 ? 2 : (int)(chart.Width / chart.ChartAreas[0].AxisX.ScaleView.Size * 0.8);
                }
                chart.Series[0].MarkerSize = markerSize;
            }
            catch
            {
                throw new Exception("Під час операції масштабування, щось пішло не так");
            }
            return chart;
        }
        public static Chart Zoom(this Chart chart, PictureBox pictureBox, CutSquare squareFild)
        {
            try
            {
                double x = (double)squareFild.X / pictureBox.Width;       //20 / 100 = 0.20
                double x1 = (double)(squareFild.Width + squareFild.X) / pictureBox.Width;
                double y1 = (double)(pictureBox.Height - squareFild.Y) / pictureBox.Height;
                double y = y1 - (double)squareFild.Height / pictureBox.Height;

                chart.ChartAreas[0].AxisX.ScaleView.ZoomReset();
                chart.ChartAreas[0].AxisY.ScaleView.ZoomReset();

                double xMin = chart.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
                double xMax = chart.ChartAreas[0].AxisX.ScaleView.ViewMaximum;
                double yMin = chart.ChartAreas[0].AxisY.ScaleView.ViewMinimum;
                double yMax = chart.ChartAreas[0].AxisY.ScaleView.ViewMaximum;

                chart.ChartAreas[0].AxisX.ScaleView.Zoom(xMax * x, xMax * x1);
                chart.ChartAreas[0].AxisY.ScaleView.Zoom(xMax * y, xMax * y1);

                chart.Series[0].MarkerSize = chart.ChartAreas[0].AxisX.ScaleView.Size > 100 ? 2 : (int)(chart.Width / chart.ChartAreas[0].AxisX.ScaleView.Size * 0.7);    
            }
            catch
            {
                throw new Exception("Під час операції масштабування, щось пішло не так");
            }
            return chart;
        }
        
    }
}
