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
    public partial class MainForm : Form
    {
        private FileSystemWatcher _fileWatcher;
        private Timer _rotationTimer;
        private string _currentLogFilePath;
        private string _lastReadContent;

        public MainForm()
        {
            InitializeComponent();
            InitializeRotationTimer();
        }

        private void InitializeRotationTimer()
        {
            _rotationTimer = new Timer();
            _rotationTimer.Interval = 30000;
            _rotationTimer.Tick += OnRotationTimer_Tick;
        }


        private void btnSetTargetFle_Click(object sender, EventArgs e)
        {
            _currentLogFilePath = txtTargetFile.Text;
            if (File.Exists(_currentLogFilePath))
            {
                InitializeFileWatcher();
                _lastReadContent = File.ReadAllText(_currentLogFilePath);
                _rotationTimer?.Start();
            }
            else
            {
                MessageBox.Show("The Specific file does not exist");
            }
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

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == _currentLogFilePath)
            {
                //_lastReadContent = File.ReadAllText(_currentLogFilePath);
            }
        }

        private void OnRotationTimer_Tick(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(_currentLogFilePath) && File.Exists(_currentLogFilePath))
            {
                ChackForChanges();
            }
        }

        private void ChackForChanges()
        {
            try
            {
                string currentContent = File.ReadAllText("E:\\LogWatch.txt");
                if(_lastReadContent != currentContent)
                {
                    ReportChanges(_lastReadContent, currentContent);
                    _lastReadContent = currentContent;
                }
            }
            catch(Exception ex)
            {
                LogError(ex);
            }
        }

        private void ReportChanges(string oldContent, string newContent)
        {
            //string changes = getChanges(oldContent, newContent);
            string changes = newContent;
            if (!string.IsNullOrWhiteSpace(changes))
            {
                dataGridView.Rows.Add(DateTime.Now, changes);
                //lstChanges.Items.Add($"Changes detected at {DateTime.Now}:\n {changes}");
            }

        }

        private string getChanges(string oldContent, string newContent)
        {
            return newContent;
        }



        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void LogError(Exception ex)
        {
            File.AppendAllText("E:\\error.txt", $"{DateTime.Now}: {ex.Message}{Environment.NewLine}");
        }
    }
}
