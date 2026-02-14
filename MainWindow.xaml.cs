using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using Galileo6;   // Galileo DLL (ReadData)


namespace GalileoDataLab
{
    /// <summary>
    /// Galileo Data Lab - Main Window
    /// </summary>
    public partial class MainWindow : Window
    {
        // =========================================================
        // Assessment 4.1:
        // Two global LinkedList<double> data structures only.
        // No other data structures are used.
        // =========================================================
        private LinkedList<double> sensorA = new LinkedList<double>(); // Sensor A
        private LinkedList<double> sensorB = new LinkedList<double>(); // Sensor B

        public MainWindow()
        {
            InitializeComponent();
        }


        // =========================================================
        // Assessment 4.2 - LoadData()
        // - Uses Galileo DLL
        // - Populates both LinkedList<double> with 400 readings
        // - No parameters, returns void
        // =========================================================
        private void LoadData()
        {
            const int SIZE = 400;

            if (!double.TryParse(txtSigma.Text, out double sigma) ||
                !double.TryParse(txtMu.Text, out double mu))
            {
                MessageBox.Show("Invalid numeric input for Sigma or Mu.");
                return;
            }

            sensorA.Clear();
            sensorB.Clear();

            ReadData galileo = new ReadData();

            for (int i = 0; i < SIZE; i++)
            {
                sensorA.AddLast(galileo.SensorA(mu, sigma));
                sensorB.AddLast(galileo.SensorB(mu, sigma));
            }
        }

        // Display one LinkedList in a ListBox
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            target.Items.Clear();

            foreach (double value in list)
            {
                target.Items.Add(value.ToString("F4"));
            }
        }

        // Display both sensor lists in the left ListView (two columns)
        private void ShowAllSensorData()
        {
            lvSensors.Items.Clear();

            var a = sensorA.First;
            var b = sensorB.First;

            while (a != null && b != null)
            {
                lvSensors.Items.Add(new
                {
                    SensorA = a.Value.ToString("F4"),
                    SensorB = b.Value.ToString("F4")
                });

                a = a.Next;
                b = b.Next;
            }
        }




        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadData();

                DisplayListboxData(sensorA, lbSensorA);
                DisplayListboxData(sensorB, lbSensorB);
                ShowAllSensorData();

                txtStatus.Text = $"Status: Loaded A={sensorA.Count}, B={sensorB.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Load Error");
                txtStatus.Text = "Status: Error loading data";
            }
        }



    }
}