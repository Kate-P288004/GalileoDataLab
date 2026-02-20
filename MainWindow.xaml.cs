using Galileo6;   // Assessment requirement: external Galileo DLL
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GalileoDataLab
{
    /// <summary>
    /// =========================================================
    /// Galileo Data Processing Application
    /// ICTPRG535 / ICTPRG547 Assessment Task
    /// Purpose:
    ///   • Load satellite data via Galileo DLL
    ///   • Store data in LinkedList<double>
    ///   • Sort data using Selection / Insertion Sort
    ///   • Search data using Binary Search (Iterative + Recursive)
    /// =========================================================
    /// </summary>
    public partial class MainWindow : Window
    {

        // =========================================================
        // Assessment 4.1
        // GLOBAL DATA STRUCTURES
        //
        // Requirement:
        //   - ONLY TWO global variables allowed.
        //   - Must be LinkedList<double>.
        // =========================================================
        private LinkedList<double> sensorA = new LinkedList<double>();
        private LinkedList<double> sensorB = new LinkedList<double>();


        public MainWindow()
        {
            InitializeComponent();
        }

        // =========================================================
        // Assessment 4.2 — LoadData()
        //
        // Purpose:
        //   Creates Galileo DLL instance.
        //   Generates exactly 400 readings.
        //   Populates Sensor A and Sensor B LinkedLists.
        //
        // Inputs: none
        // Outputs: void
        // =========================================================
        private void LoadData()
        {
            const int SIZE = 400; // Requirement: fixed size

            // Validate Sigma and Mu numeric inputs
            if (!double.TryParse(txtSigma.Text, out double sigma) ||
                !double.TryParse(txtMu.Text, out double mu))
            {
                MessageBox.Show("Invalid numeric input.");
                return;
            }

            // Clear previous data
            sensorA.Clear();
            sensorB.Clear();

            // Required: Galileo DLL created INSIDE method
            ReadData galileo = new ReadData();

            // Populate LinkedLists
            for (int i = 0; i < SIZE; i++)
            {
                sensorA.AddLast(galileo.SensorA(mu, sigma));
                sensorB.AddLast(galileo.SensorB(mu, sigma));
            }
        }

        // =========================================================
        // Assessment 4.5 — NumberOfNodes()
        //
        // Purpose:
        //   Counts total elements in LinkedList.
        //
        // Input: LinkedList<double>
        // Output: integer node count
        // =========================================================
        private int NumberOfNodes(LinkedList<double> list)
        {
            int count = 0;

            // traverse nodes manually (assessment expectation)
            for (var node = list.First; node != null; node = node.Next)
                count++;

            return count;
        }

        // =========================================================
        // Utility Method — DisplayListboxData()
        //
        // Purpose:
        //   Display linked list data visually in UI ListBox.
        // =========================================================
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            target.Items.Clear();

            foreach (double value in list)
                target.Items.Add(value.ToString("F4"));
        }

        // =========================================================
        // Assessment 4.3 — ShowAllSensorData()
        //
        // Purpose:
        //   Display Sensor A + B side-by-side in ListView.
        // =========================================================
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

        // =========================================================
        // Load Button Click
        //
        // Calls:
        //   LoadData()
        //   ShowAllSensorData()
        // =========================================================
        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            LoadData();

            DisplayListboxData(sensorA, lbSensorA);
            DisplayListboxData(sensorB, lbSensorB);

            ShowAllSensorData();

            txtStatus.Text = "Status: Data loaded successfully";
        }

        // =========================================================
        // Assessment 4.7 — SelectionSort()
        //
        // Matches Appendix pseudo code:
        //   min
        //   max
        //   i loop
        //   j loop
        //   ElementAt + Find swap
        //
        // Output: true if sorted
        // =========================================================
        private bool SelectionSort(LinkedList<double> list)
        {
            if (list.Count < 2) return false;

            int max = NumberOfNodes(list);

            // outer loop
            for (int i = 0; i < max - 1; i++)
            {
                int min = i;

                // find smallest element
                for (int j = i + 1; j < max; j++)
                {
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j;
                }

                // Appendix supplied code style
                LinkedListNode<double> currentMin = list.Find(list.ElementAt(min))!;
                LinkedListNode<double> currentI = list.Find(list.ElementAt(i))!;

                // swap values
                double temp = currentMin.Value;
                currentMin.Value = currentI.Value;
                currentI.Value = temp;
            }

            return true;
        }

        // =========================================================
        // Assessment 4.8 — InsertionSort()
        //
        // Matches Appendix pseudo code.
        // =========================================================
        private bool InsertionSort(LinkedList<double> list)
        {
            if (list.Count < 2) return false;

            int max = NumberOfNodes(list);

            for (int i = 0; i < max - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    if (list.ElementAt(j - 1) > list.ElementAt(j))
                    {
                        LinkedListNode<double> current =
                            list.Find(list.ElementAt(j))!;

                        LinkedListNode<double> previous =
                            list.Find(list.ElementAt(j - 1))!;

                        // swap values
                        double temp = previous.Value;
                        previous.Value = current.Value;
                        current.Value = temp;
                    }
                }
            }

            return true;
        }

        // =========================================================
        // Assessment 4.9 — BinarySearchIterative()
        //
        // Appendix structure:
        // while → middle → compare → adjust bounds
        //
        // Returns index of found or nearest value.
        // =========================================================
        private int BinarySearchIterative(
            LinkedList<double> list,
            double searchValue,
            int minimum,
            int maximum)
        {
            while (minimum <= maximum)
            {
                // safe midpoint calculation
                int middle = minimum + ((maximum - minimum) / 2);

                double midValue = Math.Round(list.ElementAt(middle));

                if (searchValue == midValue)
                    return middle;

                else if (searchValue < midValue)
                    maximum = middle - 1;
                else
                    minimum = middle + 1;
            }

            return minimum;
        }

        // =========================================================
        // Assessment 4.10 — BinarySearchRecursive()
        //
        // Appendix recursive implementation.
        // =========================================================
        public int BinarySearchRecursive(
            LinkedList<double> list,
            double searchValue,
            int minimum,
            int maximum)
        {
            if (minimum <= maximum)
            {
                int middle = minimum + ((maximum - minimum) / 2);

                double midValue =
                    Math.Round(list.ElementAt(middle));

                if (searchValue == midValue)
                    return middle;

                else if (searchValue < midValue)
                    return BinarySearchRecursive(list, searchValue, minimum, middle - 1);

                else
                    return BinarySearchRecursive(list, searchValue, middle + 1, maximum);
            }

            return minimum;
        }
        // =========================================================
        // SORT BUTTONS — Stopwatch timing (Assessment Requirement)
        //
        // Requirement:
        //  • Run sorting algorithm when button clicked
        //  • Measure execution time using Stopwatch
        //  • Display processing time in MILLISECONDS
        //  • Refresh ListBox and ListView after sorting  
        // =========================================================

        // =========================================================
        // Sensor A — Selection Sort (Assessment 4.7)
        // =========================================================
        private void btnSelSortA_Click(object sender, RoutedEventArgs e)
        {
            // Start Stopwatch BEFORE algorithm execution
            // Measures algorithm performance time
            var sw = Stopwatch.StartNew();

            // Run Selection Sort algorithm on Sensor A data
            SelectionSort(sensorA);

            // Stop timing immediately after sorting completes
            sw.Stop();

            // Display elapsed time in milliseconds (assessment requirement)
            txtMsSelA.Text = sw.ElapsedMilliseconds.ToString();

            // Refresh Sensor A ListBox to show sorted values
            DisplayListboxData(sensorA, lbSensorA);

            // Refresh combined ListView display (A + B side-by-side)
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor A Selection Sort completed";
        }



        // =========================================================
        // Sensor A — Insertion Sort (Assessment 4.8)
        // =========================================================
        private void btnInsSortA_Click(object sender, RoutedEventArgs e)
        {
            // Start timing
            var sw = Stopwatch.StartNew();

            // Run Insertion Sort on Sensor A
            InsertionSort(sensorA);

            // Stop timing
            sw.Stop();

            // Display execution time in milliseconds
            txtMsInsA.Text = sw.ElapsedMilliseconds.ToString();

            // Update UI so user can see sorted data
            DisplayListboxData(sensorA, lbSensorA);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor A Insertion Sort completed";
        }



        // =========================================================
        // Sensor B — Selection Sort (Assessment 4.7)
        // =========================================================
        private void btnSelSortB_Click(object sender, RoutedEventArgs e)
        {
            // Start Stopwatch timing
            var sw = Stopwatch.StartNew();

            // Run Selection Sort on Sensor B
            SelectionSort(sensorB);

            // Stop timing
            sw.Stop();

            // Display elapsed milliseconds
            txtMsSelB.Text = sw.ElapsedMilliseconds.ToString();

            // Refresh visual displays
            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor B Selection Sort completed";
        }



        // =========================================================
        // Sensor B — Insertion Sort (Assessment 4.8)
        // =========================================================
        private void btnInsSortB_Click(object sender, RoutedEventArgs e)
        {
            // Start performance timer
            var sw = Stopwatch.StartNew();

            // Execute Insertion Sort
            InsertionSort(sensorB);

            // Stop timer after algorithm completes
            sw.Stop();

            // Show sorting time in milliseconds
            txtMsInsB.Text = sw.ElapsedMilliseconds.ToString();

            // Refresh UI displays
            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor B Insertion Sort completed";
        }

        // =========================================================
        // SEARCH BUTTONS — Stopwatch timing (Assessment Requirement)
        //
        // Requirement:
        //  • User enters an integer search value
        //  • Binary search must run (Iterative or Recursive)
        //  • Stopwatch must measure processing time
        //  • Time displayed in TICKS (not milliseconds)
        //  • Highlight the found value in the ListBox
        //
        // Note:
        //  Binary Search only works correctly on SORTED data.
        // =========================================================


        // =========================================================
        // Sensor A — Binary Search (Iterative)
        // =========================================================
        private void btnSearchAIter_Click(object sender, RoutedEventArgs e)
        {
            // Validate integer input (assessment requires numeric search value)
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            // Binary search requires valid index range.
            // Last valid index = numberOfNodes - 1
            int max = NumberOfNodes(sensorA) - 1;

            // Start stopwatch BEFORE algorithm execution
            // Required to measure processing time (ticks)
            var sw = Stopwatch.StartNew();

            // Call Assessment 4.9 – BinarySearchIterative()
            int index = BinarySearchIterative(sensorA, value, 0, max);

            // Stop stopwatch immediately after algorithm completes
            sw.Stop();

            // Display processing time in ticks (Assessment requirement)
            txtTicksAIter.Text = sw.ElapsedTicks.ToString();

            // Refresh ListBox display (ensures latest data shown)
            DisplayListboxData(sensorA, lbSensorA);

            // Highlight the found (or nearest neighbour) index
            // SelectedIndex uses 0-based indexing
            lbSensorA.SelectedIndex = index;

            txtStatus.Text = "Status: Sensor A Iterative Search completed";
        }


        // =========================================================
        // Sensor A — Binary Search (Recursive)
        // =========================================================
        private void btnSearchARec_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            int max = NumberOfNodes(sensorA) - 1;

            var sw = Stopwatch.StartNew();

            // Call Assessment 4.10 – BinarySearchRecursive()
            int index = BinarySearchRecursive(sensorA, value, 0, max);

            sw.Stop();

            txtTicksARec.Text = sw.ElapsedTicks.ToString();

            DisplayListboxData(sensorA, lbSensorA);

            lbSensorA.SelectedIndex = index;

            txtStatus.Text = "Status: Sensor A Recursive Search completed";
        }


        // =========================================================
        // Sensor B — Binary Search (Iterative)
        // =========================================================
        private void btnSearchBIter_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            int max = NumberOfNodes(sensorB) - 1;

            var sw = Stopwatch.StartNew();

            int index = BinarySearchIterative(sensorB, value, 0, max);

            sw.Stop();

            txtTicksBIter.Text = sw.ElapsedTicks.ToString();

            DisplayListboxData(sensorB, lbSensorB);

            lbSensorB.SelectedIndex = index;

            txtStatus.Text = "Status: Sensor B Iterative Search completed";
        }


        // =========================================================
        // Sensor B — Binary Search (Recursive)
        // =========================================================
        private void btnSearchBRec_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            int max = NumberOfNodes(sensorB) - 1;

            var sw = Stopwatch.StartNew();

            int index = BinarySearchRecursive(sensorB, value, 0, max);

            sw.Stop();

            txtTicksBRec.Text = sw.ElapsedTicks.ToString();

            DisplayListboxData(sensorB, lbSensorB);

            lbSensorB.SelectedIndex = index;

            txtStatus.Text = "Status: Sensor B Recursive Search completed";
        }

    }
}