using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Log_Watch
{
    public partial class NewForm : Form
    {
        private FileSystemWatcher _fileWatcher;
        private Timer _rotationTimer;
        private string _currentLogFilePath;
        private string _lastReadContent;
        private int _batchSeconds;
        public NewForm()
        {
            InitializeComponent();
            pnlBatchSeconds.Visible = false;
            _batchSeconds = 15;
            InitializeRotationTimer();
            txtBatchSeconds.KeyPress += TxtBatchSeconds_KeyPress;
        }
        private void TxtBatchSeconds_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only control characters (e.g., backspace), digits, and optionally a single dot for decimals
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Suppress the character
            }
        }
        private void InitializeRotationTimer()
        {
            _rotationTimer = new Timer();
            _rotationTimer.Interval = _batchSeconds;
            _rotationTimer.Tick += OnRotationTimer_Tick;
        }
        
        private void InitializeFileWatcher()
        {
            _fileWatcher = new FileSystemWatcher();
            _fileWatcher.Path = Path.GetDirectoryName(_currentLogFilePath);
            _fileWatcher.Filter = Path.GetFileName(_currentLogFilePath);

            //this will update the change as soon as the file gets modified
            //_fileWatcher.Changed += OnChanged;

            _fileWatcher.EnableRaisingEvents = true;
        }
        private void OnRotationTimer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentLogFilePath) && File.Exists(_currentLogFilePath))
            {
                ChackForChanges();
            }
        }

        private void ChackForChanges()
        {
            try
            {
                string currentContent = File.ReadAllText(_currentLogFilePath);
                if (_lastReadContent != currentContent)
                {
                    ReportChanges(_lastReadContent, currentContent);
                    _lastReadContent = currentContent;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
        private void ReportChanges(string oldContent, string newContent)
        {
            string changes = newContent;
            if (!string.IsNullOrWhiteSpace(changes))
            {
                LogChanges(newContent);
                //DGLogs.Rows.Add(DateTime.Now, changes);
            }
        }

        private void LogChanges(string newContent)
        {
            var oldLines = _lastReadContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var newLines = newContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var addedLines = newLines.Except(oldLines).ToList();
            var removedLines = oldLines.Except(newLines).ToList();

            StringBuilder changeLog = new StringBuilder();
            if (addedLines.Any())
            {
                changeLog.AppendLine("\n Added lines : ");
                foreach (var line in addedLines)
                {
                    changeLog.AppendLine(line);
                }
            }
            if (removedLines.Any())
            {
                changeLog.AppendLine("\n Removed lines : ");
                foreach (var line in removedLines)
                {
                    changeLog.AppendLine(line);
                }
            }

            if (changeLog.Length > 0)
            {
                DGLogs.Rows.Add(DateTime.Now, changeLog.ToString());
            }

            _lastReadContent += newContent; // Update the last read content with new content
        }

        private void LogError(Exception ex)
        {
            File.AppendAllText("E:\\error.txt", $"{DateTime.Now}: {ex.Message}{Environment.NewLine}");
        }

        private void btnSetTargetFile_Click(object sender, EventArgs e)
        {
            _currentLogFilePath = txtTargetPath.Text;
            if (File.Exists(_currentLogFilePath))
            {
                InitializeFileWatcher();
                _lastReadContent = File.ReadAllText(_currentLogFilePath);
                btnSetTargetFile.Visible = false;
                btnStop.Visible = true;
                _rotationTimer?.Start();
            }
            else
            {
                MessageBox.Show("The Specific file does not exist");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _rotationTimer?.Stop();
            btnStop.Visible = false;
            btnSetTargetFile.Visible = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pnlBatchSeconds.Visible = true;
        }

        private void btnSeconds_Click(object sender, EventArgs e)
        {
            pnlBatchSeconds.Visible = false;
            _batchSeconds = Convert.ToInt32(txtBatchSeconds.Text);
            _rotationTimer.Stop();
            _rotationTimer.Start();
        }
    }
}
