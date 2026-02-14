using System.Windows;
using System.Collections.Generic;
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


    }
}