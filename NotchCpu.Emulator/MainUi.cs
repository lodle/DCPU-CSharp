using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace NotchCpu.Emulator
{
    partial class MainUi : Form
    {
        IProgEmulator _Emulator;
        Thread _Thread;

        int _LastReg = 0;
        bool _IgnoreHightlight;
        bool _ShowSelect;

        int _CurLogList;
        List<String>[] _LogList = new List<String>[2];

        String[] _RegNames = new string[] {"A", "B", "C", "X", "Y", "Z", "I", "J", "PC", "SP", "O" };
        double[] _Speeds = new double[] 
        { 
            0.5,    // 200 khz
            1.0,    // 100 khz
            2.0,    // 50 khz
            10.0,   // 10 khz
            100.0,  // 1 khz
            200.0,  // 500 hz 
            10000.0 // 10 hz
        };

        public MainUi()
        {
            _LogList[0] = new List<string>();
            _LogList[1] = new List<string>();

            _Emulator = Mef.GetExportedValue<IProgEmulator>();
            _Emulator.LogEvent += new LogHandler(OnLog);
            _Emulator.ErrorEvent += new ErrorHandler(OnError);
            _Emulator.CompleteEvent += new CompleteHandler(FinishedEmulation);
            _Emulator.MemUpdateEvent += new MemUpdateHandler(OnMemUpdate);
            _Emulator.RegUpdateEvent += new MemUpdateHandler(OnRegUpdate);

            InitializeComponent();
            InitDisplayGrids();

            foreach (var reg in _RegNames)
            {
                var item = new System.Windows.Forms.ListViewItem(new string[] { reg, "0x0000" }, -1);
                this.listView1.Items.Add(item);
            }

            CBSpeed.SelectedIndex = _Speeds.Length-1;
            _Emulator.SpeedMultiplier = _Speeds[CBSpeed.SelectedIndex];
            InitProg();

            _Thread = new Thread(new ThreadStart(UpdateThread));
        }

        private void InitDisplayGrids()
        {
            for (int x = 0; x < 32; x++)
            {
                var col = new DataGridViewTextBoxColumn();

                col.HeaderText = (x + 1).ToString();
                col.MaxInputLength = 1;
                col.MinimumWidth = 15;
                col.Name = "C" + (x + 1).ToString();
                col.ReadOnly = true;
                col.Resizable = System.Windows.Forms.DataGridViewTriState.False;
                col.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
                col.Width = 15;

                TextGridView.Columns.Add(col);
            }

            for (int x = 0; x < 12; x++)
            {
                var row = new DataGridViewRow();

                for (int y = 0; y < 32; y++)
                {
                    var cell = new DataGridViewTextBoxCell();

                    cell.Style.ForeColor = GetColor((char)0);
                    cell.Style.BackColor = GetColor((char)0);
                    cell.Style.Font = new Font(new FontFamily("Courier New"), 11, FontStyle.Regular);

                    row.Cells.Add(cell);
                }

                TextGridView.Rows.Add(row);
            }
        }

        private void MainUi_Load(object sender, EventArgs e)
        {
            _Emulator.StartEmulation();
            _Thread.Start();
        }

        private void MainUi_Close(object sender, FormClosedEventArgs e)
        {
            _Emulator.StopEmulation();
            _Thread.Abort();
        }

        delegate void ConsoleTextHandler(ushort x, ushort y, ushort value);
        public void SetConsoleText(ushort x, ushort y, ushort val)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ConsoleTextHandler(SetConsoleText), new object[] { x, y, val });
            }
            else
            {
                if (y * x >= 0x180)
                    return;

                var cell = TextGridView.Rows[x].Cells[y];

                if (val != 0)
                    cell.Value = "" + (Char)(val & 0x00FF);
                else
                    cell.Value = "";

                cell.Style.ForeColor = GetColor((char)((val & 0xF000) >> 12));
                cell.Style.BackColor = GetColor((char)((val & 0x0F00) >> 8));

                if (_ShowSelect)
                {
                    TextGridView.ClearSelection();
                    cell.Selected = true;
                }
            }
        }

        private Color GetColor(char p)
        {
            var b = (p & 0x01) > 0 ? 255 : 0;
            var g = (p & 0x02) > 0 ? 255 : 0;
            var r = (p & 0x04) > 0 ? 255 : 0;

            return Color.FromArgb(r, g, b);
        }

        private void ClearConsoleText()
        {
            for (ushort x = 0; x < 12; x++)
            {
                for (ushort y = 0; y < 32; y++)
                    SetConsoleText(x, y, 0);
            }

            TextGridView.ClearSelection();
        }

        private void InitProg()
        {
            _LastReg = 0;

            TBLog.Text = "";

            OnSpeedChanged(null, null);
            ClearConsoleText();

            if (_LastReg != -1)
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    item.BackColor = Color.White;
                    item.SubItems[1].Text = String.Format("0x{0,4:X4}", 0);
                }
            }
        }

        void OnMemUpdate(ushort loc, ushort value)
        {
            if (loc < 0x8000 || loc > 0x8180)
                return;

            var x = (loc - 0x8000)/32;
            var y = (loc - 0x8000)%32;

            SetConsoleText((ushort)x, (ushort)y, value);
        }

        void OnRegUpdate(ushort loc, ushort value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MemUpdateHandler(OnRegUpdate), new object[] { loc, value });
            }
            else
            {
                if (_LastReg == -1)
                {
                    foreach (ListViewItem item in listView1.Items)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", 0);
                }

                if (!_IgnoreHightlight)
                {
                    if (_LastReg != -1)
                        listView1.Items[_LastReg].BackColor = Color.White;

                    listView1.Items[loc].BackColor = Color.LightBlue;
                    _LastReg = loc;
                }

                listView1.Items[loc].SubItems[1].Text = String.Format("0x{0,4:X4}", value);
            }
        }

        delegate void FinishedHandler();
        private void FinishedEmulation()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new FinishedHandler(FinishedEmulation));
            }
            else
            {
                int x = 0;
                foreach (ListViewItem item in listView1.Items)
                {
                    if (x < 8)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", _Emulator.Registers.Reg[x]);
                    else if (x == 8)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", _Emulator.Registers.PC);
                    else if (x == 9)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", _Emulator.Registers.SP);
                    else if (x == 10)
                        item.SubItems[1].Text = String.Format("0x{0,4:X4}", _Emulator.Registers.O);

                    x++;
                }

                ButStartToggle.Text = "Start";
            }
        }

        private void Log(String msg)
        {
            _LogList[_CurLogList].Add(msg);
        }

        private void ButStartToggle_Click(object sender, EventArgs e)
        {
            if (ButStartToggle.Text == "Stop")
            {
                _Emulator.RunMode = RunMode.Step;
                ButStartToggle.Text = "Start";
            }
            else
            {
                ButStartToggle.Text = "Stop";
                _Emulator.RunMode = RunMode.Run;
            }
        }

        private void ButStep_Click(object sender, EventArgs e)
        {
            if (_Emulator == null)
                InitProg();

            _Emulator.Step();

            _IgnoreHightlight = true;

            OnRegUpdate((ushort)8, _Emulator.Registers.PC);
            OnRegUpdate((ushort)7, _Emulator.Registers.SP);
            OnRegUpdate((ushort)9, _Emulator.Registers.O);

            _IgnoreHightlight = false;
        }

        private void ButReset_Click(object sender, EventArgs e)
        {
            _Emulator.Reset();
            InitProg();
        }

        private void OnSpeedChanged(object sender, EventArgs e)
        {
            _Emulator.SpeedMultiplier = _Speeds[CBSpeed.SelectedIndex];
            _ShowSelect = CBSpeed.SelectedIndex >= _Speeds.Length - 2;

            if (_ShowSelect)
                return;

            if (_LastReg != -1)
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    item.BackColor = Color.White;
                    item.SubItems[1].Text = "";
                }

                _LastReg = -1;
            }
        }

        private void OnLog(String message)
        {
            Log(message);
        }

        private void OnError(Exception e)
        {
            Log("Emulator threw exception: " + e.Message);
        }

        private void UpdateThread()
        {
            while (true)
            {
                //update twice a second fps
                Thread.Sleep(500);
                OnRefresh();
            }
        }

        delegate void RefreshHandler();
        private void OnRefresh()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new RefreshHandler(OnRefresh));
            }
            else
            {
                int hertz = _Emulator.AvgHertz;

                if (hertz > 1000)
                    TBSpeed.Text = String.Format("{0,2} kHz", hertz / 1000.0);
                else
                    TBSpeed.Text = String.Format("{0} Hz", hertz);

                String logOutput = "";

                var oldLog = _CurLogList;
                _CurLogList = _CurLogList == 1 ? 0 : 1;

                foreach (var log in _LogList[oldLog])
                    logOutput += log + Environment.NewLine;

                _LogList[oldLog].Clear();

                if (!String.IsNullOrEmpty(logOutput))
                    TBLog.AppendText(logOutput);
            }
        }
    }
}
