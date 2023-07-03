using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TaskManagerSimulator
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<ProcessInfo> processes;
        private string connectionString = "YourConnectionString";
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            processes = new ObservableCollection<ProcessInfo>();

            DataContext = processes;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            try
            {
                processes.Clear();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT * FROM Processes";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ProcessInfo process = new ProcessInfo
                        {
                            ProcessId = reader.GetInt32(0),
                            ProcessName = reader.GetString(1),
                            HandleNumber = reader.GetInt32(2),
                            ThreadCount = reader.GetInt32(3),
                            MachineName = reader.GetString(4)
                        };

                        processes.Add(process);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while loading processes: " + ex.Message);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

        private void NewProcessButton_Click(object sender, RoutedEventArgs e)
        {
            NewProcessDialog dialog = new NewProcessDialog();
            dialog.ShowDialog();

            LoadProcesses();
        }

        private void CloseProcessButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInfo selectedProcess = ProcessGrid.SelectedItem as ProcessInfo;

            if (selectedProcess != null)
            {
                try
                {
                    Process.GetProcessById(selectedProcess.ProcessId)?.Kill();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred while closing the process: " + ex.Message);
                }

                LoadProcesses();
            }
        }
    }
}
