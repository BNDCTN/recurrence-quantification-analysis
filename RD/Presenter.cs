using System;
using System.Linq;
using System.Text;
using Model;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using NCalc;
using Newtonsoft.Json;
using System.Drawing.Printing;

namespace RD
{
    class Presenter
    {
        string appName = " - Recurrent";

        readonly IMainWindow _view;
        readonly IMessageService _message;

        IInfoForm _info;
        Project project;
        CutSquare squareFild;
        Bitmap varmap;

        Encoding Code = Encoding.GetEncoding(1251);

        public Presenter(IMainWindow view, IMessageService message)
        {
            _view = view;
            _message = message;

            project = new Project();
            project.Name = "New project";
            _view.FormName = project.Name + " - Recurrent";

            _view.E = "1";
            _view.N = "100";

            _view.RowStart = "0";
            _view.RowEnd = "100";
            _view.RowStep = "1";

            _view.StatusProgress.Visible = false;
            _view.StartBtnEnable = false;
            _view.StopBtnEnable = false;

            #region Підписки на події
            _view.NewClick += _view_NewClick;
            _view.OpenClick += _view_OpenClick;
            _view.SaveClick += _view_SaveClick;
            _view.SaveAsClick += _view_SaveAsClick;
            _view.PrintClick += _view_PrintClick;

            _view.RandomRowClick += _view_RandomRowClick;
            _view.ExceptClick += _view_ExceptClick;
            _view.FxButtonClick += _view_FxButtonClick;
            _view.InsertButtonClick += _view_InsertButtonClick;

            _view.NChanged += _view_NChanged;
            _view.EChanged += _view_EChanged;

            _view.ChartPointMouseEnter += _view_ChartPointMouseEnter;
            _view.ChartPointMouseLeave += _view_ChartPointMouseLeave;
            _view.ChartPointMouseWheel += _view_ChartPointMouseWheel;
            _view.ChartPointScrollChanged += _view_ChartPointScrollChanged;

            _view.DiagramClickDown += _view_DiagramClickDown;
            _view.DiagramClickUp += _view_DiagramClickUp;
            _view.DiagramMouseMove += _view_DiagramMouseMove;

            _view.DiagramContextItemClick += _view_DiagramContextItemClick;
            _view.GridContextItemClick += _view_GridContextItemClick;

            RowGridDataChanged += Presenter_RowGridDataChanged;
            ProgressAction += Presenter_ProgressAction;
            Audio.AudioProgress += Audio_AudioProgress;


            _view.ExitClick += _view_ExitClick;
            _view.InfoClick += _view_InfoClick;
            
            #endregion
        }

        #region Оголошення локальних подій
        event Action<int, int> ProgressAction;
        event EventHandler RowGridDataChanged;
        #endregion

        #region Реалізація локальних методів
        private void Presenter_ProgressAction(int max, int value)
        {
            Action action = () =>
            {
                _view.StatusProgress.Maximum = max;
                if (value > 0 && value < max) _view.StatusProgress.Value = value+1;
                else _view.StatusProgress.Value = 0;
            };
            _view.View_Invoke(action);
        }
        private void Audio_AudioProgress(int max, int value)
        {
            this.Presenter_ProgressAction(max, value);
        }
        void ManageDiagrams(TimeRow row, double epsilon, ref Image diagramRasterized, ref Image diagramDistance, ref Image scaleDistance, ref Chart pointDiagram, ref Chart lineDiagram, ref string AnalysisResult)
        {
            DiagramManager diagram = new DiagramManager(row, (float)epsilon);
            diagram.GenerateReccurentMatrix();
            diagramRasterized = diagram.GetReccurentDiagram(_view.DiagramBox.Width, _view.DiagramBox.Height);
            diagramDistance = diagram.GetDistanceDiagram(_view.DistanceBox.Width, _view.DistanceBox.Height);
            scaleDistance = diagram.GetDistancePalette(_view.DistanceScaleBox.Width);

            bool[,] matrix = diagram.GetReccurentMatrix();

            int side = matrix.GetLength(0);
            pointDiagram.Series[0].MarkerSize = side > 100 ? 2 : (int)(pointDiagram.Height / side * 0.9);
            pointDiagram.Series[0].Points.Clear();

            int move = 0;
            int max = side * side + row.Row.Length-1;
            int invokeVal = max / 10;

            for (int i = 0; i < side; i++)
            {
                for (int j = 0; j < side; j++)
                {
                    if (matrix[i, j] == true)
                    {
                        pointDiagram.Series[0].Points.AddXY(i + 1, j + 1);
                    }
                    if ((move % invokeVal) == 0)
                        ProgressAction?.Invoke(10, move/invokeVal);
                    move++;
                }
            }

            lineDiagram.Series[0].Points.Clear();
            for (int i = 0; i < row.Row.Length - 1; i++)
            {
                lineDiagram.Series[0].Points.AddXY((row[i]), row[i + 1]);
                if ((move % invokeVal) == 0)
                    ProgressAction?.Invoke(10, move / invokeVal);
                move++;
            }
            //if (move < max) ProgressAction?.Invoke(10, move / invokeVal);

            AnalysisResult = "\n Analysis: \n RR: " + diagram.RR.ToString() + "\n DET: " + diagram.DET.ToString() + 
            "\n L: " + diagram.L.ToString() + "\n DIV: " + diagram.DIV.ToString() + 
            "\n ENTR: " + diagram.ENTR.ToString() + "\n RATIO: " + diagram.RATIO.ToString();

            row.Dispose();
            diagram.Dispose();
            matrix = null;
            GC.Collect();
        }

        async void SaveAsProject(bool SaveAs)
        {
            _view.Status = "Проект зберігається. Будь ласка, зачекайте...";

            project.Epsilon = (float)Convert.ToDouble(_view.E);
            float[] currentRow = new float[_view.RowGrid.RowCount - 1];

            for (int i = 0; i < _view.RowGrid.RowCount - 1; i++)
                currentRow[i] = (float)Convert.ToDouble(_view.RowGrid.Rows[i].Cells[1].Value);

            project.Row = currentRow;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Recurrent plot project|*.rpp|Text file|*.txt";
            dialog.Title = "Save an project File";

            if (!SaveAs)
            {
                if (project.Path == null)
                {
                    if (project.Name != null) dialog.FileName = project.Name;
                    dialog.ShowDialog();
                }
                else dialog.FileName = project.Path;
            }
            else dialog.ShowDialog();

            Func<bool> func = () =>
            {
                try
                {
                    char[] array = dialog.FileName.ToCharArray();
                    project.Name = String.Concat(array.Reverse().Skip(4).TakeWhile((p) => p != '\\').Reverse());
                    project.Path = dialog.FileName;
                    string json = JsonConvert.SerializeObject(project);
                    if (dialog.FileName != "")
                    {
                        FileStream fs = (FileStream)dialog.OpenFile();
                        fs.Write(new UnicodeEncoding().GetBytes(json), 0, new UnicodeEncoding().GetByteCount(json));
                        fs.Close();
                    }
                    return true;
                }
                catch (Exception exception)
                {
                    _message.ShowMessage(exception.Message);
                    return false;
                }
            };

            bool task = await Task<bool>.Factory.StartNew(func);
            string message = task ? "Готово!" : "Невдача!";
            _view.Status = message;
            _view.FormName = project.Name + appName;
        }
        private void PD_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font TitleFont = new Font("Times New Roman", 7, FontStyle.Bold, GraphicsUnit.Millimeter);
            Font Textfont = new Font("Times New Roman", 5, FontStyle.Regular, GraphicsUnit.Millimeter);

            string Title = project.Name == "" ? "Blank project": project.Name;
            Image recurrent;
            Image dіstance;
            StringBuilder Text = new StringBuilder();
            Text.Append("Row: [ \r\n");
            int lines = 5;

            try
            {
                for (var i = 0; i < _view.RowGrid.RowCount; i++)
                {
                    if (i > 16) { Text.Append(" ... "); break; }
                    Text.Append(" " + Convert.ToString((double)_view.RowGrid.Rows[i].Cells[1].Value) + "  ");   //Current row. Not project
                    if ((i + 1) % 8 == 0) { Text.Append("\r\n"); lines++; }
                }
            }
            catch (Exception)
            {
                _message.ShowMessage("Не вдалось отримати числовий ряд.");
            }

            try
            {
                if (_view.DiagramBox.Image == null || _view.DistanceBox.Image == null) throw new Exception();
                recurrent = _view.DiagramBox.Image;
                dіstance = _view.DistanceBox.Image;
            }
            catch (Exception)
            {
                Bitmap b = new Bitmap(200, 200);
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.Clear(Color.FromArgb(200, 200, 200));
                    g.DrawString("Plot error", Textfont, Brushes.White, new Point(63, 85));
                }
                recurrent = b;
                dіstance = b;
            }

            Text.Append("]; \r\n\r\n Recurrent Plot [Epsilon = " + _view.E + "]:");     //Epsilon current
            string PlotTitle = " Distance Plot:";

            e.Graphics.DrawString(Title, TitleFont, Brushes.Black, new PointF(50, 45));
            e.Graphics.DrawString(Text.ToString(), Textfont, Brushes.Black, new PointF(50, 100));
            e.Graphics.DrawString(_view.Analysis, Textfont, Brushes.Black, new PointF(610, lines * 30 - 20));
            e.Graphics.DrawImage(recurrent, 200, ++lines * 30, 400, 400);
            e.Graphics.DrawString(PlotTitle, Textfont, Brushes.Black, new PointF(50, ++lines * 30 + 400));
            e.Graphics.DrawImage(dіstance, 200, ++lines * 30 + 400, 400, 400);
        }           //Print

        private void _view_ExitClick(object sender, EventArgs e)
        {
            if (_message.ShowQuestion("Покинути програму?") == DialogResult.Yes)
                _view.Exit();
        }
        private void _view_InfoClick(object sender, EventArgs e)
        {
            _info = new InfoForm();
            _info.ShowInfo();
        }

        #endregion

        #region DiagramBox події
        private void _view_DiagramMouseMove(object sender, MouseEventArgs e)
        {
            if (squareFild != null)
            {
                if (squareFild.rCuttin) squareFild.SetSize(e.X - squareFild.X, e.Y - squareFild.Y);
                if (squareFild.rMoving) squareFild.Move(e.X, e.Y);
                if (squareFild.rTransformaing) squareFild.Transform(e.X, e.Y);
                _view.DiagramBox.Image = squareFild.DrawRect(new Bitmap(_view.DiagramBox.Image));
            }
        }
        private void _view_DiagramClickUp(object sender, MouseEventArgs e)
        {
            if (squareFild != null && (squareFild.rCuttin || squareFild.rMoving))
            {
                squareFild.HoldRect();

                try
                {
                    _view.PointDiagram.Zoom(_view.DiagramBox, squareFild);
                }
            catch (Exception exception)
                {
                    _message.ShowError(exception.Message);
                }
            }
        }
        private void _view_DiagramClickDown(object sender, MouseEventArgs e)
        {
            if (squareFild == null)
            {
                //varmap = new Bitmap(_view.DiagramBox.Image);
                squareFild = new CutSquare(e.Location, new Bitmap(_view.DiagramBox.Image));
                //squareFild.SetStartPoint(e.X, e.Y);
                //squareFild.SetSize(0, 0);
                //squareFild.CursorChange += ..;
                //squareFild.CutFace += ..;
            }
            else squareFild.InFocus(e.X, e.Y, 0, 0);    
        }

        private void _view_DiagramContextItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.ToString() == "Reset")
                {
                    _view.DiagramBox.Image = squareFild.Clean();
                    _view.PointDiagram.Reset();
                    squareFild = null;
                }
            }
            catch(Exception exeption)
            {
                _message.ShowError(exeption.Message);
            }           
        }
        #endregion

        #region ChartPoint події
        private void _view_ChartPointMouseWheel(object sender, MouseEventArgs e)
        {       
            try
            {
               _view.PointDiagram.Zoom(e);

               if (squareFild == null) squareFild = new CutSquare(new Bitmap(_view.DiagramBox.Image));      
               squareFild.ChartReflection(_view.PointDiagram, _view.DiagramBox);

               if (squareFild.Width >= _view.DiagramBox.Width - 1)
               {
                    _view.DiagramBox.Image=squareFild.Clean();
                    squareFild = null;
               }
            }
            catch (Exception exception)
            {
                _message.ShowError(exception.Message);
            }
        }
        private void _view_ChartPointMouseLeave(object sender, EventArgs e)
        {
            this._view.PointDiagram.Parent.Focus();
        }
        private void _view_ChartPointMouseEnter(object sender, EventArgs e)
        {
            this._view.PointDiagram.Focus();
        }
        private void _view_ChartPointScrollChanged(object sender, EventArgs e)
        {
            if (squareFild != null)
            {
                squareFild.ChartReflection(_view.PointDiagram, _view.DiagramBox);
            }
        }
        #endregion

        #region TextBoxes події 
        private void _view_EChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }
        private void _view_NChanged(object sender, EventArgs e)
        {
            int n = 100;
            try
            {
                n = Convert.ToInt32(_view.N);
                if (n < 1 || n > 10000)
                {
                    n = 100;
                    throw new FormatException("N, приймає лише цілі чила в інтервалі від 1 до 10000.");
                }
            }
            catch (FormatException format_exception)
            {
                _message.ShowMessage(format_exception.Message);
            }
            catch (Exception exception)
            {
                _message.ShowExclamation(exception.Message/* "Будь ласка, перевірте формат введених даних."*/);
            }
            _view.N = n.ToString();
        }
        #endregion

        #region Buttons події
        private void _view_NewClick(object sender, EventArgs e)
        {
            if (_message.ShowQuestion("Очистити?") == DialogResult.Yes)
            {
                project = new Project();
                project.Name = "New project";
                _view.RowGrid.Rows.Clear();
                _view.PointDiagram.Series[0].Points.Clear();
                _view.SplineDiagram.Series[0].Points.Clear();
                _view.RowDiagram.Series[0].Points.Clear();
                Bitmap b = new Bitmap(10, 10);
                using (Graphics g = Graphics.FromImage(b))
                {
                    g.Clear(Color.White);
                }
                _view.DistanceBox.Image = b;
                _view.DiagramBox.Image = b;
                _view.Analysis = "";
                _view.FormName = project.Name + appName;
            }
            GC.Collect();
        }
        private async void _view_OpenClick(object sender, EventArgs e)
        {
            _view.Status = "Завантажується проект. Будь ласка, зачекайте...";

            Image diagramRasterized = new Bitmap(_view.DiagramBox.Width, _view.DiagramBox.Height);
            Image diagramDistance = new Bitmap(_view.DistanceBox.Width, _view.DistanceBox.Height);
            Image scaleDistance = new Bitmap(_view.DistanceScaleBox.Width, _view.DistanceScaleBox.Height);
            _view.PointDiagram.Series[0].Points.Clear();
            _view.SplineDiagram.Series[0].Points.Clear();
            Chart pointDiagram = _view.PointDiagram.Copy();
            Chart lineDiagram = _view.SplineDiagram.Copy();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.txt)|*.txt|wav files (*.wav)|*.wav|Recurrent plot projects (*.rpp)|*.rpp";
            dialog.FilterIndex = 3;
            dialog.RestoreDirectory = true;

            string path = "";
            string AnalysisResult = "";
            Project loaded = new Project();

            float epsilon = 1;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                    if (dialog.OpenFile() != null)
                    {
                        try
                        {
                            path = dialog.FileName;
                        }
                        catch (Exception exception)
                        {
                            _message.ShowMessage(exception.Message);
                        }
                    }
            }

            _view.StatusProgress.Visible = true;

            Func<bool> func = () =>
            {
                try
                {
                    if (dialog.FilterIndex == 2)
                    {                                                           //Sound
                        loaded.Epsilon = 0.1F;
                        loaded.Row = Audio.prepare(path);
                        char[] charArray = path.ToCharArray();
                        loaded.Name = String.Concat(charArray.Reverse().Skip(4).TakeWhile((p) => p != '\\').Reverse());
                        charArray = null;
                    }
                    else if (dialog.FilterIndex == 1)
                    {
                        char spliter = ' ';
                        StreamReader reader = new StreamReader(path);
                        string Row = reader.ReadToEnd();

                        for (int i = 0; i < Row.Length; i++)
                        {
                            if (!(Char.IsDigit(Row[i])) && Row[i] != ',' && Row[i] != '-')
                            {
                                if (Row[i - 1] != ' ') Row = Row.Replace(Row[i], ' ');
                                else { Row = Row.Remove(i, 1); i--; }
                            }
                            ProgressAction?.Invoke(Row.Length, i);
                        }
                        if (!(Char.IsDigit(Row[Row.Length - 1]))) Row = Row.Remove(Row.Length - 1, 1);

                        float[] array = Row.Split(spliter).Select((x) => (float)(Convert.ToDouble(x))).ToArray();
                        loaded.Epsilon = 1;
                        loaded.Row = array;
                        char[] charArray = path.ToCharArray();
                        loaded.Name = String.Concat(charArray.Reverse().Skip(4).TakeWhile((p) => p != '\\').Reverse());
                    }                       //Numbers
                    else
                    {                                                          //Project
                        StreamReader reader = new StreamReader(path, Encoding.Unicode);
                        string json = reader.ReadToEnd();
                        loaded = JsonConvert.DeserializeObject<Project>(json);
                        reader.Close();
                        
                        ManageDiagrams(new TimeRow(loaded.Row), loaded.Epsilon, ref diagramRasterized, 
                        ref diagramDistance, ref scaleDistance, ref pointDiagram, ref lineDiagram, ref AnalysisResult);
                    }
                    return true;
                }
                catch (Exception exception)
                {
                    _message.ShowMessage(exception.Message);
                    return false;
                }
            };

            bool task = await Task<bool>.Factory.StartNew(func);
            string message = task ? "Готово!" : "Невдача!";
            _view.StatusProgress.Visible = false;
            _view.Status = message;

            _view.PointDiagram.Series[0] = pointDiagram.Series[0];
            _view.SplineDiagram.Series[0] = lineDiagram.Series[0];
            _view.DiagramBox.Image = diagramRasterized;
            _view.DistanceBox.Image = diagramDistance;
            _view.DistanceScaleBox.Image = scaleDistance;
            _view.Analysis = AnalysisResult;

            _view.E = epsilon.ToString();
            _view.SetRowGrid(loaded.Row);
            
            RowGridDataChanged?.Invoke(this, EventArgs.Empty);
            _view.StartBtnEnable = task;

            /*** project reload ***/
            project = loaded;
            _view.FormName = project.Name + appName;

            loaded = null;
            AnalysisResult = null;
            diagramRasterized = null;
            diagramDistance = null;
            scaleDistance = null;
            pointDiagram = null;
            lineDiagram = null;

            GC.Collect();
        }
        private void _view_SaveClick(object sender, EventArgs e)
        {
            SaveAsProject(false);
        }
        private void _view_PrintClick(object sender, EventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PrintDocument PD = new PrintDocument();
                    PD.PrinterSettings = pd.PrinterSettings; ;
                    PD.PrintPage += PD_PrintPage;
                    PD.Print();
                }
                catch(Exception exception)
                {
                    _message.ShowExclamation(exception.Message);
                }
            }
            pd.Dispose();
        }

        private async void _view_ExceptClick(object sender, EventArgs e)
        {
            GC.Collect();
            _view.Status = "Будується рекурентна діаграма за заданим числовим рядом. Будь ласка, зачекайте...";

            string Row = _view.TextRow;
            DataGridView datagrid = _view.RowGrid;
            string E = _view.E;
            string AnalysisResult = "";
            Image diagramRasterized = new Bitmap(_view.DiagramBox.Width, _view.DiagramBox.Height);
            Image diagramDistance = new Bitmap(_view.DistanceBox.Width, _view.DistanceBox.Height);
            Image scaleDistance = new Bitmap(_view.DistanceScaleBox.Width, _view.DistanceScaleBox.Height);
            _view.PointDiagram.Series[0].Points.Clear();
            _view.SplineDiagram.Series[0].Points.Clear();
            Chart pointDiagram = _view.PointDiagram.Copy();
            Chart lineDiagram = _view.SplineDiagram.Copy();

            _view.StartBtnEnable = false;
            _view.StopBtnEnable = true;
            _view.StatusProgress.Visible = true;
            Func<bool> func = () =>
            {
                try
                {
                    float[] array = new float[datagrid.RowCount - 1];
                    for (short i = 0; i < datagrid.RowCount - 1; i++)
                    {
                        array[i] = (float)Convert.ToDouble((datagrid.Rows[i].Cells[1].Value));
                    }
                    TimeRow row = new TimeRow(array);
                    double epsilon = Convert.ToDouble(E);

                    ManageDiagrams(row, epsilon, ref diagramRasterized, ref diagramDistance, 
                    ref scaleDistance, ref pointDiagram, ref lineDiagram, ref AnalysisResult);

                    return true;
                }
                catch (Exception exception)
                {
                    _message.ShowError(exception.Message);
                    return false;
                }
            };

            bool task = await Task<bool>.Factory.StartNew(func);
            string message = task ? "Готово!" : "Невдача!";
            _view.StatusProgress.Visible = false;
            _view.Status = message;
            _view.PointDiagram.Series[0] = pointDiagram.Series[0];
            _view.SplineDiagram.Series[0] = lineDiagram.Series[0];
            _view.DiagramBox.Image = diagramRasterized;
            _view.DistanceBox.Image = diagramDistance;
            _view.DistanceScaleBox.Image = scaleDistance;
            _view.Analysis = AnalysisResult;

            _view.StartBtnEnable = true;
            _view.StopBtnEnable = false;

            Row = null;
            datagrid = null;
            E = null;
            AnalysisResult = null;
            diagramRasterized = null;
            diagramDistance = null;
            scaleDistance = null;
            pointDiagram = null;
            lineDiagram = null;

            GC.Collect();
        }
        private async void _view_RandomRowClick(object sender, EventArgs e)
        {
            _view.Status = "Генерується ряд випадкових чисел. Будь ласка, зачекайте...";

            string N = _view.N;
            string Row = "";
            TimeRow rowtable = new TimeRow();
            _view.StatusProgress.Visible = true;
            Func<bool> func = () =>
            {
                try
                {
                    TimeRow row = new TimeRow(Convert.ToInt32(N), 100);
                    int max = row.GetSize();
                    int move = 0;
                    int invokeVal = max / 10;
                    rowtable = row;
                    for (int i = 0; i < row.GetSize(); i++)
                    {
                        Row += Math.Round(row[i], 4);
                        if (i < row.GetSize() - 1)
                        {
                            Row += '\r';
                            Row += '\n';
                        }
                        move++;
                        if (move%invokeVal==0)ProgressAction?.Invoke(10, move / invokeVal);               
                    }

                    row.Dispose();
                    return true;
                }
                catch (Exception exception)
                {
                    //_message.ShowError("Невірно задані параметри.");
                    _message.ShowError(exception.Message);
                    return false;
                }
            };

            bool task = await Task<bool>.Factory.StartNew(func);
            string message = task ? "Готово!" : "Невдача!";
            _view.StatusProgress.Visible = false;

            _view.Status = message;
            _view.TextRow = Row;
            _view.SetRowGrid(rowtable.Row);

            RowGridDataChanged?.Invoke(this, EventArgs.Empty);
            _view.StartBtnEnable = task;

            Row = null;
            rowtable.Dispose();
            GC.Collect();
        }
        private async void _view_FxButtonClick(object sender, EventArgs e)
        {
            _view.Status = "Будується числовий ряд функції. Будь ласка, зачекайте...";
            var expression = "";
            var nativeFx = _view.Fx;
            int start = Convert.ToInt32(_view.RowStart);
            int n = Convert.ToInt32(_view.RowEnd);
            float step = (float)Convert.ToDouble(_view.RowStep);
            float[] rowtable = new float[(int)(Math.Round(((n-start)/step)))];
            string Row = "";
            _view.StatusProgress.Visible = true;

            Func<bool> func = () => { 
            try
            {
                int move = 0;
                int invokeVal = n / 10;
                for (float i = start; i < n; i += step)                                  // 0 + step
                {
                    expression = nativeFx;
                    expression = expression.Replace("x", Convert.ToString(i).Replace(',', '.'));
                    Row += Math.Round(Convert.ToDouble(new Expression(expression).Evaluate()), 4);
                    rowtable[move] = (float)(Math.Round(Convert.ToDouble(new Expression(expression).Evaluate()), 4));
                    if (move%invokeVal==0) ProgressAction?.Invoke(10, move / invokeVal);
                    move++;
                    if (i < n - step)
                    Row += "\r\n";
                }
                    if (move < n) ProgressAction?.Invoke(n, n);
                    return true;
            }
            catch (Exception exception)
            {
                _message.ShowExclamation(exception.Message);
                    return false;
            }
            };

            bool task = await Task<bool>.Factory.StartNew(func);
            string message = task ? "Готово!" : "Невдача!";
            _view.StatusProgress.Visible = false;
            _view.Status = message;
            _view.TextRow = Row;
            _view.SetRowGrid(rowtable);
            RowGridDataChanged?.Invoke(this, EventArgs.Empty);

            _view.StartBtnEnable = task;
            rowtable = null;
            GC.Collect();
        }
        private async void _view_InsertButtonClick(object sender, EventArgs e)
        {
            _view.Status = "Виконується формування ряду. Будь ласка, зачекайте...";

            string Row = _view.TextRow;
            DataGridView datagrid = _view.RowGrid;
            TimeRow rowtable = new TimeRow();
            _view.StatusProgress.Visible = true;
            Func<bool> func = () =>
            {
                try
                {
                   char spliter=' ';

                   for (int i = 0; i < Row.Length; i++)
                   {
                       if (!(Char.IsDigit(Row[i])) && Row[i]!=',' && Row[i] != '-')
                       {
                                if (Row[i - 1] != ' ') Row = Row.Replace(Row[i], ' ');
                                else { Row = Row.Remove(i, 1); i--; }
                       }  
                   }
                    if (!(Char.IsDigit(Row[Row.Length - 1]))) Row = Row.Remove(Row.Length - 1, 1);

                    float[] array = Row.Split(spliter).Select((x) => (float)Convert.ToDouble(x)).ToArray();                  
                    rowtable.Row = array;

                    return true;
                }
                catch (Exception exception)
                {
                    _message.ShowMessage(exception.Message);
                    return false;
                }
            };

            bool task = await Task<bool>.Factory.StartNew(func);
            string message = task ? "Готово!" : "Невдача!";
            _view.StatusProgress.Visible = false;
            _view.Status = message;

            _view.TextRow = Row;
            _view.SetRowGrid(rowtable.Row);

            RowGridDataChanged?.Invoke(this, EventArgs.Empty);
            _view.StartBtnEnable = task;

            rowtable.Dispose();
            datagrid = null;
            Row = null;
            GC.Collect();
        }
        #endregion

        #region Menu події
        private void _view_SaveAsClick(object sender, EventArgs e)
        {
            SaveAsProject(true);
        }
        #endregion

        #region Grid події
        private void Presenter_RowGridDataChanged(object sender, EventArgs e)
        {
            DataGridView datagrid = _view.RowGrid;
            _view.RowDiagram.Series[0].Points.Clear();
            for (int i = 0; i < datagrid.Rows.Count - 1; i++)
                _view.RowDiagram.Series[0].Points.AddXY(i + 1, Convert.ToDouble(datagrid.Rows[i].Cells[1].Value));
            datagrid=null;
        }
        private void _view_GridContextItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.ToString() == "Check Path")
                {
                    _view.SplineDiagram.Series[0].Points.Clear();
                    for (int i = 0; i < _view.RowGrid.SelectedCells.Count - 1; i++)
                        _view.SplineDiagram.Series[0].Points.AddXY((_view.RowGrid.SelectedCells[i].Value), _view.RowGrid.SelectedCells[i+1].Value);
                }
            }
            catch (Exception exeption)
            {
                _message.ShowError(exeption.Message);
            }
        }
        #endregion

    }
}
