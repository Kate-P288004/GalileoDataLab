using System;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using Galileo6;   // Galileo DLL (ReadData)
using System.Linq;


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
        // Matches Appendix pseudo code (i, j, min, ElementAt + Find)
        // Return type: Boolean
        // =========================================================
        private bool SelectionSort(LinkedList<double> list)
        {
            if (list == null || list.Count < 2)
                return false;

            int min = 0;
            int max = NumberOfNodes(list); // Appendix: max => numberOfNodes(list)

            for (int i = 0; i < max - 1; i++)   // for ( i = 0 to max - 1 )
            {
                min = i;

                for (int j = i + 1; j < max; j++) // for ( j = i + 1 to max )
                {
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j;
                }

                // Supplied C# code (Appendix style)
                LinkedListNode<double> currentMin = list.Find(list.ElementAt(min))!;
                LinkedListNode<double> currentI = list.Find(list.ElementAt(i))!;

                // Swap values
                double temp = currentMin.Value;
                currentMin.Value = currentI.Value;
                currentI.Value = temp;
            }

            return true;
        }


        // =========================================================
        // Assessment 4.8 – InsertionSort()
        // Matches Appendix pseudo code (i, j, ElementAt + Find)
        // Return type: Boolean
        // =========================================================
        private bool InsertionSort(LinkedList<double> list)
        {
            if (list == null || list.Count < 2)
                return false;

            int max = NumberOfNodes(list); // integer max = numberOfNodes(list)

            for (int i = 0; i < max - 1; i++) // for ( i = 0 to max – 1 )
            {
                for (int j = i + 1; j > 0; j--) // for ( j = i + 1 to j > 0, j-- )
                {
                    if (list.ElementAt(j - 1) > list.ElementAt(j))
                    {
                        // Supplied C# code (Appendix)
                        LinkedListNode<double> current = list.Find(list.ElementAt(j))!;
                        LinkedListNode<double> previous = list.Find(list.ElementAt(j - 1))!;

                        // Swap previous value with current value
                        double temp = previous.Value;
                        previous.Value = current.Value;
                        current.Value = temp;
                    }
                }
            }

            return true;
        }

        // =========================================================
        // Assessment 4.9 – BinarySearchIterative()
        // Matches Appendix pseudo code exactly
        // =========================================================
        private int BinarySearchIterative(
            LinkedList<double> list,
            double searchValue,
            int minimum,
            int maximum)
        {
            // while (minimum <= maximum - 1)
            while (minimum <= maximum - 1)
            {
                // integer middle = minimum + maximum / 2
                int middle = minimum + (maximum / 2);

                // if (search value = list element(middle))
                if (searchValue == list.ElementAt(middle))
                {
                    // return ++middle
                    return ++middle;
                }
                // else if (search value < list element(middle))
                else if (searchValue < list.ElementAt(middle))
                {
                    // maximum => middle - 1
                    maximum = middle - 1;
                }
                else
                {
                    // minimum => middle + 1
                    minimum = middle + 1;
                }
            }

            // return minimum
            return minimum;
        }
        // =========================================================
        // Assessment 4.10 – BinarySearchRecursive()
        // Matches Appendix pseudo code (minimum, maximum, middle, comparisons)
        // =========================================================
        private int BinarySearchRecursive(
            LinkedList<double> list,
            double searchValue,
            int minimum,
            int maximum)
        {
            // if (minimum <= maximum - 1)
            if (minimum <= maximum - 1)
            {
                // integer middle = minimum + maximum / 2
                int middle = minimum + (maximum / 2);

                // if (search value = list element(middle))
                if (searchValue == list.ElementAt(middle))
                {
                    // return middle
                    return middle;
                }
                // else if (search value < list element(middle))
                else if (searchValue < list.ElementAt(middle))
                {
                    // return binarySearchRecursive(list, search value, minimum, middle - 1)
                    return BinarySearchRecursive(list, searchValue, minimum, middle - 1);
                }
                else
                {
                    // return binarySearchRecursive(list, search value, middle + 1, maximum)
                    return BinarySearchRecursive(list, searchValue, middle + 1, maximum);
                }
            }

            // return minimum
            return minimum;
        }



    }
}

