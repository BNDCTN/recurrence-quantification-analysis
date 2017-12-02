using Model;
using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace RD
{
    public interface IMainWindow
    {
        string FormName { get; set; }

        string N { get; set; }
        string E { get; set; }
        string TextRow { get; set; }
        string Fx { get; set; }
        string RowStep { get; set; }
        string RowStart { get; set; }
        string RowEnd { get; set; }
        string Analysis { get; set; }
        string Status { get; set; }

        bool StartBtnEnable { get; set; }
        bool StopBtnEnable { get; set; }

        DataGridView RowGrid { get; set; }
        PictureBox DiagramBox { get; set; }
        PictureBox DistanceBox { get; set; }
        PictureBox DistanceScaleBox { get; set; }
        Chart PointDiagram { get; set; }
        Chart SplineDiagram { get; set; }
        Chart RowDiagram { get; set; }
        ToolStripProgressBar StatusProgress { get; set; }

        event EventHandler NewClick;
        event EventHandler OpenClick;
        event EventHandler SaveClick;
        event EventHandler SaveAsClick;
        event EventHandler PrintClick;
        event EventHandler ExitClick;
        event EventHandler InfoClick;

        event EventHandler RandomRowClick;
        event EventHandler ExceptClick;
        event EventHandler FxButtonClick;
        event EventHandler InsertButtonClick;

        event EventHandler NChanged;
        event EventHandler EChanged;

        event EventHandler ChartPointMouseEnter;
        event EventHandler ChartPointMouseLeave;
        event EventHandler ChartPointScrollChanged;
        event MouseEventHandler ChartPointMouseWheel;

        event MouseEventHandler DiagramClickDown;
        event MouseEventHandler DiagramClickUp;
        event MouseEventHandler DiagramMouseMove;

        event ToolStripItemClickedEventHandler DiagramContextItemClick;
        event ToolStripItemClickedEventHandler GridContextItemClick;

        void View_Invoke(Delegate method);
        void SetRowGrid(params float[] row);
        void Exit();
    }

    public partial class MainWindow : Form, IMainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            menuStrip.Renderer = new MenuRenderer();

            btnNew.Click += BtnNew_Click;
            btnOpen.Click += BtnOpen_Click;
            btnSave.Click += BtnSave_Click;
            btnPrint.Click += BtnPrint_Click;

            randButton.Click += RandButton_Click;
            ExceptButton.Click += ExceptButton_Click;
            FxButton.Click += FxButton_Click;
            rowInsertButton.Click += RowInsertButton_Click;

            nBox.TextChanged += NBox_TextChanged;
            theBorder.TextChanged += TheBorder_TextChanged;

            chartPoint.MouseEnter += ChartPoint_MouseEnter;
            chartPoint.MouseLeave += ChartPoint_MouseLeave;
            chartPoint.MouseWheel += ChartPoint_MouseWheel;
            chartPoint.AxisViewChanged += ChartPoint_AxisViewChanged;

            diagramBox.MouseDown += DiagramBox_MouseDown;
            diagramBox.MouseUp += DiagramBox_MouseUp;
            diagramBox.MouseMove += DiagramBox_MouseMove;

            contextMenuDiagram.ItemClicked += ContextMenuDiagram_ItemClicked;
            contextMenuGrid.ItemClicked += ContextMenuGrid_ItemClicked;

            ClientSizeChanged += MainWindow_ClientSizeChanged;
            tabPlots.Click += TabPlots_ClientSizeChanged;

            saveAsToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
            infoToolStripMenuItem.Click += InfoToolStripMenuItem_Click;
        }

        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InfoClick?.Invoke(this, EventArgs.Empty);
        }

        #region Події (оголошення)
        public event EventHandler NewClick;
        public event EventHandler OpenClick;
        public event EventHandler SaveClick;
        public event EventHandler SaveAsClick;
        public event EventHandler PrintClick;
        public event EventHandler ExitClick;
        public event EventHandler InfoClick;

        public event EventHandler RandomRowClick;
        public event EventHandler ExceptClick;
        public event EventHandler FxButtonClick;
        public event EventHandler InsertButtonClick;

        public event EventHandler NChanged;
        public event EventHandler EChanged;

        public event EventHandler ChartPointMouseEnter;
        public event EventHandler ChartPointMouseLeave;
        public event EventHandler ChartPointScrollChanged;
        public event MouseEventHandler ChartPointMouseWheel;

        public event MouseEventHandler DiagramClickDown;
        public event MouseEventHandler DiagramClickUp;
        public event MouseEventHandler DiagramMouseMove;

        public event ToolStripItemClickedEventHandler DiagramContextItemClick;
        public event ToolStripItemClickedEventHandler GridContextItemClick;

        #endregion

        #region Події (реалізація)
        private void TabPlots_ClientSizeChanged(object sender, EventArgs e)
        {
            ResizeComponents();
        }
        private void MainWindow_ClientSizeChanged(object sender, EventArgs e)
        {
            ResizeComponents();
        }

        private void DiagramBox_MouseMove(object sender, MouseEventArgs e)
        {
            DiagramMouseMove?.Invoke(this, e);
        }
        private void DiagramBox_MouseUp(object sender, MouseEventArgs e)
        {
            DiagramClickUp?.Invoke(this, e);
        }
        private void DiagramBox_MouseDown(object sender, MouseEventArgs e)
        {
            DiagramClickDown?.Invoke(this, e);
        }

        private void ChartPoint_MouseWheel(object sender, MouseEventArgs e)
        {
            ChartPointMouseWheel?.Invoke(this, e);
        }
        private void ChartPoint_MouseLeave(object sender, EventArgs e)
        {
            ChartPointMouseLeave?.Invoke(this, EventArgs.Empty);
        }
        private void ChartPoint_MouseEnter(object sender, EventArgs e)
        {
            ChartPointMouseEnter?.Invoke(this, EventArgs.Empty);
        }
        private void ChartPoint_AxisViewChanged(object sender, ViewEventArgs e)
        {
            ChartPointScrollChanged?.Invoke(this, EventArgs.Empty);
        }
        private void TheBorder_TextChanged(object sender, EventArgs e)
        {
            EChanged?.Invoke(this, EventArgs.Empty);
        }
        private void NBox_TextChanged(object sender, EventArgs e)
        {
            NChanged?.Invoke(this, EventArgs.Empty);
        }


        //************ BUTTONS ************//
        private void BtnNew_Click(object sender, EventArgs e)
        {
            NewClick?.Invoke(this, EventArgs.Empty); 
        }
        private void BtnOpen_Click(object sender, EventArgs e)
        {
            OpenClick?.Invoke(this, EventArgs.Empty);
        }
        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveClick?.Invoke(this, EventArgs.Empty);
        }
        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsClick?.Invoke(this, EventArgs.Empty);
        }
        private void BtnPrint_Click(object sender, EventArgs e)
        {
            PrintClick?.Invoke(this, EventArgs.Empty);
        }


        private void ExceptButton_Click(object sender, EventArgs e)
        {
            ExceptClick?.Invoke(this, EventArgs.Empty);
        }
        private void RandButton_Click(object sender, EventArgs e)
        {
            RandomRowClick?.Invoke(this, EventArgs.Empty);
        }
        private void FxButton_Click(object sender, EventArgs e)
        {
            FxButtonClick?.Invoke(this, EventArgs.Empty);
        }
        private void RowInsertButton_Click(object sender, EventArgs e)
        {
            InsertButtonClick?.Invoke(this, EventArgs.Empty);
        }

        //************ CONTEXT MENU ***********//
        private void ContextMenuDiagram_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            DiagramContextItemClick?.Invoke(this, e);
        }
        private void ContextMenuGrid_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            GridContextItemClick?.Invoke(this, e);
        }


        //************* Menu *************//
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitClick?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Властивості
        public string FormName
        {
            get { return Text; }
            set => Text = value;
        }

        public string N
        {
            get { return nBox.Text; }
            set => nBox.Text = value;
        }
        public string E
        {
            get { return theBorder.Text; }
            set => theBorder.Text = value;
        }
        public string TextRow
        {
            get { return timeRow.Text; }
            set => timeRow.Text = value;
        }
        public string Fx
        {
            get { return FxBox.Text; }
            set => FxBox.Text = value;
        }
        public string RowStep
        {
            get { return StepBox.Text; }
            set => StepBox.Text = value;
        }
        public string RowStart
        {
            get { return rowFromBox.Text; }
            set => rowFromBox.Text = value;
        }
        public string RowEnd
        {
            get { return rowToBox.Text; }
            set => rowToBox.Text = value;
        }
        public string Analysis
        {
            get { return labelAnalysis.Text; }
            set => labelAnalysis.Text = value;
        }
        public string Status
        {
            get { return statusLabel.Text; }
            set => statusLabel.Text = value;
        }

        public bool StartBtnEnable
        {
            get { return ExceptButton.Enabled; }
            set => ExceptButton.Enabled = value;
        }
        public bool StopBtnEnable
        {
            get { return btnAbort.Enabled; }
            set => btnAbort.Enabled = value;
        }

        public DataGridView RowGrid
        {
            get { return RowGridView; }
            set => RowGridView = value;
        }
        public PictureBox DiagramBox
        {
            get { return diagramBox; }
            set => diagramBox = value;
        }
        public PictureBox DistanceBox
        {
            get { return distanceBox; }
            set => distanceBox = value;
        }
        public PictureBox DistanceScaleBox
        {
            get { return distanceScaleBox; }
            set => distanceScaleBox = value;
        }
        public Chart PointDiagram
        {
            get { return chartPoint; }
            set => chartPoint = value;
        }
        public Chart SplineDiagram
        {
            get { return splineChart; }
            set => splineChart = value;
        }
        public Chart RowDiagram
        {
            get { return rowChart; }
            set => rowChart = value;
        }
        public ToolStripProgressBar StatusProgress
        {
            get { return statusProgress; }
            set => statusProgress = value;
        }







        #endregion


        #region Локальні методи
        public void View_Invoke(Delegate method)
        {
            Invoke(method);
        }
        public void SetRowGrid(params float[] row)
        {
            try
            {
                RowGridView.Rows.Clear();
                for (int i = 0; i < row.Length; i++)
                    RowGridView.Rows.Add(i + 1, Math.Round(row[i], 4));
            }
            catch { }
        }
        void ResizeComponents()
        {
            if (chartPoint.Height > chartPoint.Parent.Height) chartPoint.Height = splineChart.Height;
            if (splineChart.Height > splineChart.Parent.Height) splineChart.Height = panelDistance.Height;
            if (panelDistance.Height > panelDistance.Parent.Height) panelDistance.Height = chartPoint.Height;
            chartPoint.Width = chartPoint.Height;
            splineChart.Width = splineChart.Height;
            panelDistance.Width = panelDistance.Height;
        }
        public void Exit()
        {
            Close();
        }
        #endregion

    }
}
