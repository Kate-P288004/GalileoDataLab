using System;
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
        // Declare TWO global LinkedList<double> data structures only.
        // No arrays, List<T>, or other data structures are used.
        // =========================================================
        private LinkedList<double> sensorA = new LinkedList<double>(); // Sensor A data
        private LinkedList<double> sensorB = new LinkedList<double>(); // Sensor B data

        public MainWindow()
        {
            InitializeComponent();
        }

        // =========================================================
        // Assessment 4.2 – LoadData()
        // Purpose:
        //   • Create Galileo DLL object inside this method
        //   • Load exactly 400 readings for Sensor A and Sensor B
        // Constraints:
        //   • No parameters, return type void
        //   • Store data in the 2 global LinkedList<double> only
        // =========================================================
        private void LoadData()
        {
            const int SIZE = 400; // Required number of readings

            // Validate Sigma and Mu input
            if (!double.TryParse(txtSigma.Text, out double sigma) ||
                !double.TryParse(txtMu.Text, out double mu))
            {
                MessageBox.Show("Invalid numeric input for Sigma or Mu.");
                return;
            }

            // Clear old data before reloading
            sensorA.Clear();
            sensorB.Clear();

            // Galileo DLL instance (required inside LoadData)
            ReadData galileo = new ReadData();

            // Generate and store readings in LinkedLists
            for (int i = 0; i < SIZE; i++)
            {
                sensorA.AddLast(galileo.SensorA(mu, sigma));
                sensorB.AddLast(galileo.SensorB(mu, sigma));
            }
        }


        // =========================================================
        // Assessment 4.5 – NumberOfNodes()
        // Returns the number of nodes in a LinkedList
        // =========================================================
        private int NumberOfNodes(LinkedList<double> list)
        {
            int count = 0;
            for (var node = list.First; node != null; node = node.Next)
                count++;
            return count;
        }

        // =========================================================
        // DisplayListboxData()
        // Displays LinkedList<double> values in a ListBox
        // =========================================================
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            target.Items.Clear();

            foreach (double value in list)
            {
                target.Items.Add(value.ToString("F4"));
            }
        }

        // =========================================================
        // ShowAllSensorData()
        // Displays Sensor A and Sensor B side-by-side in the ListView
        // =========================================================
        private void ShowAllSensorData()
        {
            lvSensors.Items.Clear();

            LinkedListNode<double>? a = sensorA.First;
            LinkedListNode<double>? b = sensorB.First;

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

        // =========================================================
        // Load button click:
        // Runs LoadData and refreshes all three displays
        // =========================================================
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

        // =========================================================
        // Assessment 4.7 – SelectionSort()
        // ---------------------------------------------------------
        // Sorts a LinkedList<double> in ascending order using
        // the Selection Sort algorithm.
        // 
        // Input:
        //   • LinkedList<double> list
        // Return:
        //   • Boolean (true if sort completed, false if not required)
        //
        // Constraints:
        //   • No arrays or additional data structures
        //   • Operates directly on LinkedList nodes
        // =========================================================
        private bool SelectionSort(LinkedList<double> list)
        {
            // If the list has fewer than two elements, sorting is not required
            if (list == null || list.Count < 2)
                return false;

            // Outer loop moves the boundary of the unsorted section
            for (LinkedListNode<double>? current = list.First;
                 current != null;
                 current = current.Next)
            {
                // Assume the current node contains the minimum value
                LinkedListNode<double>? minNode = current;

                // Inner loop searches for the smallest value in the remaining list
                for (LinkedListNode<double>? scan = current.Next;
                     scan != null;
                     scan = scan.Next)
                {
                    if (scan.Value < minNode.Value)
                    {
                        minNode = scan;
                    }
                }

                // Swap the values of the current node and the minimum node
                double temp = current.Value;
                current.Value = minNode.Value;
                minNode.Value = temp;
            }

            return true;
        }



    }
}
