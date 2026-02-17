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
        // ---------------------------------------------------------
        // Sorts a LinkedList<double> in ascending order using
        // the Insertion Sort algorithm.
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
        private bool InsertionSort(LinkedList<double> list)
        {
            // If list is null or has fewer than two elements, no sort needed
            if (list == null || list.Count < 2)
                return false;

            // Start from the second node
            LinkedListNode<double>? node = list.First?.Next;

            while (node != null)
            {
                // Save next node so we don't lose our place after shifting values
                LinkedListNode<double>? nextNode = node.Next;
                double key = node.Value;

                // Scan left to find insertion point, shifting values to the right
                LinkedListNode<double>? scan = node.Previous;
                while (scan != null && scan.Value > key)
                {
                    // Move scan's value one step right
                    scan.Next!.Value = scan.Value;
                    scan = scan.Previous;
                }

                // Place key after scan (if scan is null, place at head)
                if (scan == null)
                    list.First!.Value = key;
                else
                    scan.Next!.Value = key;

                // Continue from the saved next node
                node = nextNode;
            }

            return true;
        }

        // =========================================================
        // Assessment 4.9 – BinarySearchIterative()
        // ---------------------------------------------------------
        // Parameters:
        //   • LinkedList<double> list
        //   • double searchValue
        //   • int minimum
        //   • int maximum
        //
        // Returns:
        //   • int index of exact match, OR nearest neighbour index
        // Notes:
        //   • List must be sorted ascending before calling
        //   • Uses indices because LinkedList has no random access
        // =========================================================
        private int BinarySearchIterative(LinkedList<double> list, double searchValue, int minimum, int maximum)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (list.Count == 0) return -1;

            // Clamp bounds to valid index range
            if (minimum < 0) minimum = 0;
            if (maximum >= list.Count) maximum = list.Count - 1;

            int low = minimum;
            int high = maximum;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                double midVal = GetValueAtIndex(list, mid);

                if (midVal == searchValue)
                    return mid;

                if (searchValue < midVal)
                    high = mid - 1;
                else
                    low = mid + 1;
            }

            // Not found: choose nearest neighbour index
            // low is the insertion position, so candidates are low and low-1
            if (low <= minimum) return minimum;
            if (low >= maximum + 1) return maximum;

            double rightVal = GetValueAtIndex(list, low);
            double leftVal = GetValueAtIndex(list, low - 1);

            double diffRight = Math.Abs(rightVal - searchValue);
            double diffLeft = Math.Abs(leftVal - searchValue);

            return (diffLeft <= diffRight) ? (low - 1) : low;
        }

        // =========================================================
        // Helper – GetValueAtIndex()
        // Returns the value at a specific index in a LinkedList
        // =========================================================
        private double GetValueAtIndex(LinkedList<double> list, int index)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (index < 0 || index >= list.Count) throw new ArgumentOutOfRangeException(nameof(index));

            // Traverse from the closer end to reduce iterations
            if (index <= list.Count / 2)
            {
                LinkedListNode<double>? node = list.First;
                int i = 0;
                while (node != null && i < index)
                {
                    node = node.Next;
                    i++;
                }
                return node!.Value;
            }
            else
            {
                LinkedListNode<double>? node = list.Last;
                int i = list.Count - 1;
                while (node != null && i > index)
                {
                    node = node.Previous;
                    i--;
                }
                return node!.Value;
            }
        }

    }
}

