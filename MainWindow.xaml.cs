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
    }
}
