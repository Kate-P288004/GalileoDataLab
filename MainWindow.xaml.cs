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
    /// Student: Kate Odabas P288004
    /// Galileo Data Processing Application
    /// Units: ICTPRG535 / ICTPRG547
    ///
    /// Purpose:
    /// - Load satellite data via Galileo DLL
    /// - Store data in LinkedList<double> (Sensor A and Sensor B)
    /// - Sort using Selection Sort and Insertion Sort
    /// - Search using Binary Search (Iterative and Recursive)
    /// - Measure performance (Sort: milliseconds, Search: ticks)
    /// =========================================================
    /// </summary>
    public partial class MainWindow : Window
    {
        // =========================================================
        // Assessment Q4: GLOBAL DATA STRUCTURES
        //
        // Requirement:
        // - Only 2 global variables allowed
        // - Must be LinkedList<double>
        // =========================================================
        private LinkedList<double> sensorA = new LinkedList<double>();
        private LinkedList<double> sensorB = new LinkedList<double>();

        public MainWindow()
        {
            InitializeComponent();

            // Requirement for search highlighting range (target ±2):
            // allow multi-selection in ListBoxes.
            lbSensorA.SelectionMode = SelectionMode.Extended;
            lbSensorB.SelectionMode = SelectionMode.Extended;
        }

        // =========================================================
        // Assessment Q4.2 — LoadData()
        //
        // Purpose:
        // - Creates Galileo DLL instance INSIDE this method
        // - Generates exactly 400 readings
        // - Populates Sensor A and Sensor B LinkedLists
        //
        // Inputs: none
        // Output: void
        // =========================================================
        private void LoadData()
        {
            const int SIZE = 400; // Requirement: fixed size

            // Validate Sigma and Mu numeric inputs
            if (!double.TryParse(txtSigma.Text, out double sigma) ||
                !double.TryParse(txtMu.Text, out double mu))
            {
                MessageBox.Show("Invalid numeric input for Sigma or Mu.");
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
        // Assessment Q4.5 — NumberOfNodes()
        //
        // Purpose:
        // - Counts total elements in LinkedList
        //
        // Input: LinkedList<double>
        // Output: int node count
        // =========================================================
        private int NumberOfNodes(LinkedList<double> list)
        {
            int count = 0;

            // Traverse nodes manually (assessment expectation)
            for (var node = list.First; node != null; node = node.Next)
                count++;

            return count;
        }

        // =========================================================
        // Utility Method — DisplayListboxData()
        //
        // Purpose:
        // - Display LinkedList data in a ListBox
        //
        // Inputs:
        // - list: LinkedList<double>
        // - target: ListBox
        // =========================================================
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            target.Items.Clear();

            foreach (double value in list)
                target.Items.Add(value.ToString("F4"));
        }

        // =========================================================
        // Assessment Q4.3 — ShowAllSensorData()
        //
        // Purpose:
        // - Display Sensor A + Sensor B side-by-side in ListView
        // - Requires two columns: SensorA and SensorB
        //
        // Inputs: none
        // Output: void
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
        // Utility: IsSorted()
        //
        // Requirement:
        // - Binary Search must only run on sorted data
        // - No additional global variables are allowed
        //
        // Returns:
        // - true if sorted ascending
        // - false if any element is out of order
        // =========================================================
        private bool IsSorted(LinkedList<double> list)
        {
            if (list == null || list.Count < 2)
                return true;

            double previous = list.First.Value;
            var node = list.First.Next;

            while (node != null)
            {
                if (node.Value < previous)
                    return false;

                previous = node.Value;
                node = node.Next;
            }

            return true;
        }

        // =========================================================
        // Utility: HighlightIndexWithNeighbours()
        //
        // Requirement:
        // - Highlight the search target and two values on each side
        // - Works for range index-2 .. index+2
        // - Uses ListBox multi-selection (Extended)
        // =========================================================
        private void HighlightIndexWithNeighbours(ListBox listBox, int index)
        {
            listBox.SelectionMode = SelectionMode.Extended;
            listBox.SelectedItems.Clear();

            if (listBox.Items.Count == 0)
                return;

            // Clamp index into valid range
            if (index < 0) index = 0;
            if (index >= listBox.Items.Count) index = listBox.Items.Count - 1;

            int start = Math.Max(0, index - 2);
            int end = Math.Min(listBox.Items.Count - 1, index + 2);

            for (int i = start; i <= end; i++)
                listBox.SelectedItems.Add(listBox.Items[i]);

            listBox.ScrollIntoView(listBox.Items[index]);
            listBox.Focus();
        }

        // =========================================================
        // Load Data Button
        //
        // Calls:
        // - LoadData()
        // - DisplayListboxData()
        // - ShowAllSensorData()
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
        // Assessment Q4.7 — SelectionSort()
        //
        // Matches Appendix pseudo code:
        // - min => i
        // - max => numberOfNodes(list)
        // - for i 0..max-1
        // - for j i+1..max
        // - Find() + ElementAt() and swap
        //
        // Return: bool
        // =========================================================
        private bool SelectionSort(LinkedList<double> list)
        {
            if (list.Count < 2) return false;

            int max = NumberOfNodes(list);

            for (int i = 0; i < max - 1; i++)
            {
                int min = i;

                for (int j = i + 1; j < max; j++)
                {
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j;
                }

                // Appendix supplied style
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
        // Assessment Q4.8 — InsertionSort()
        //
        // Matches Appendix pseudo code:
        // - max = numberOfNodes(list)
        // - for i 0..max-1
        // - for j i+1 down to > 0
        // - compare (j-1) and (j)
        // - Find() + ElementAt() and swap
        //
        // Return: bool
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
                        // Appendix supplied node style
                        LinkedListNode<double> current = list.Find(list.ElementAt(j))!;
                        LinkedListNode<double> previous = list.Find(list.ElementAt(j - 1))!;

                        // Swap values (previous with current)
                        double temp = previous.Value;
                        previous.Value = current.Value;
                        current.Value = temp;
                    }
                }
            }

            return true;
        }

        // =========================================================
        // Assessment Q4.9 — BinarySearchIterative()
        //
        // IMPORTANT: Appendix rules:
        // - Calling code passes maximum = numberOfNodes(list) (COUNT)
        // - while (minimum <= maximum - 1)
        // - middle = minimum + maximum / 2
        // - return ++middle when found (1-based index)
        // - return minimum when not found
        // =========================================================
        // =========================================================
        // BinarySearchIterative (safe and correct)
        // - Works on sorted data only
        // - maximum must be the LAST INDEX (Count - 1)
        // - Returns found index, or nearest neighbour index
        // =========================================================
        private int BinarySearchIterative(
            LinkedList<double> list,
            double searchValue,
            int minimum,
            int maximum)
        {
            while (minimum <= maximum)
            {
                int middle = minimum + ((maximum - minimum) / 2);

                double middleValue = Math.Round(list.ElementAt(middle));

                if (searchValue == middleValue)
                    return middle;

                if (searchValue < middleValue)
                    maximum = middle - 1;
                else
                    minimum = middle + 1;
            }

            return minimum; // nearest neighbour position
        }

        // =========================================================
        // Assessment Q4.10 — BinarySearchRecursive()
        //
        // IMPORTANT: Appendix rules:
        // - Calling code passes maximum = numberOfNodes(list) (COUNT)
        // - if (minimum <= maximum - 1)
        // - middle = minimum + maximum / 2
        // - return middle when found
        // - return minimum when not found
        // =========================================================
        public int BinarySearchRecursive(
             LinkedList<double> list,
             double searchValue,
             int minimum,
             int maximum)
        {
            // Base case: range finished, return nearest neighbour position
            if (minimum > maximum)
                return minimum;

            // Correct midpoint calculation (prevents infinite recursion)
            int middle = minimum + ((maximum - minimum) / 2);

            double middleValue = Math.Round(list.ElementAt(middle));

            if (searchValue == middleValue)
                return middle;

            if (searchValue < middleValue)
                return BinarySearchRecursive(list, searchValue, minimum, middle - 1);

            return BinarySearchRecursive(list, searchValue, middle + 1, maximum);
        }

        // =========================================================
        // SORT BUTTONS — Stopwatch timing
        //
        // Requirement:
        // - Measure sort performance in MILLISECONDS
        // - Refresh ListBox and ListView after sorting
        // =========================================================

        private void btnSelSortA_Click(object sender, RoutedEventArgs e)
        {
            var sw = Stopwatch.StartNew();
            SelectionSort(sensorA);
            sw.Stop();

            txtMsSelA.Text = sw.ElapsedMilliseconds + " ms";
            DisplayListboxData(sensorA, lbSensorA);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor A Selection Sort completed";
        }

        private void btnInsSortA_Click(object sender, RoutedEventArgs e)
        {
            var sw = Stopwatch.StartNew();
            InsertionSort(sensorA);
            sw.Stop();

            // Fix: write Insertion Sort time into the correct textbox
            txtMsInsA.Text = sw.ElapsedMilliseconds + " ms";

            DisplayListboxData(sensorA, lbSensorA);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor A Insertion Sort completed";
        }

        private void btnSelSortB_Click(object sender, RoutedEventArgs e)
        {
            var sw = Stopwatch.StartNew();
            SelectionSort(sensorB);
            sw.Stop();

            txtMsSelB.Text = sw.ElapsedMilliseconds + " ms";
            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor B Selection Sort completed";
        }

        private void btnInsSortB_Click(object sender, RoutedEventArgs e)
        {
            var sw = Stopwatch.StartNew();
            InsertionSort(sensorB);
            sw.Stop();

            txtMsInsB.Text = sw.ElapsedMilliseconds + " ms";
            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor B Insertion Sort completed";
        }

        // =========================================================
        // SEARCH BUTTONS — Stopwatch timing
        //
        // Requirement:
        // - User enters integer value
        // - Must check list is sorted BEFORE searching
        // - Measure search performance in TICKS
        // - Highlight target + 2 neighbours each side
        // =========================================================

        private void btnSearchAIter_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            if (!IsSorted(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            int max = NumberOfNodes(sensorA) - 1;

            var sw = Stopwatch.StartNew();
            int index = BinarySearchIterative(sensorA, value, 0, max);
            sw.Stop();

            txtTicksAIter.Text = sw.ElapsedTicks + " ticks";

            DisplayListboxData(sensorA, lbSensorA);
            HighlightIndexWithNeighbours(lbSensorA, index);

            txtStatus.Text = "Status: Sensor A Iterative Search completed";
        }
        // =========================================================
        // Sensor A — Binary Search (Recursive)
        // Requirement:
        // - Validate integer input
        // - Check data is sorted before Binary Search
        // - Measure processing time in TICKS
        // - Highlight found value + 2 neighbours on each side
        // =========================================================
        private void btnSearchARec_Click(object sender, RoutedEventArgs e)
        {
            // 1) Validate integer input (Sensor A textbox)
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            // 2) Binary Search only works on sorted data
            if (!IsSorted(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // 3) Set correct maximum index (last valid index)
            int max = NumberOfNodes(sensorA) - 1;

            // 4) Measure performance in ticks
            var sw = Stopwatch.StartNew();
            int index = BinarySearchRecursive(sensorA, value, 0, max);
            sw.Stop();

            // 5) Display ticks (required)
            txtTicksARec.Text = sw.ElapsedTicks + " ticks";

            // 6) Refresh ListBox contents
            DisplayListboxData(sensorA, lbSensorA);

            // 7) Highlight target ±2 neighbours (clamped inside helper)
            HighlightIndexWithNeighbours(lbSensorA, index);

            txtStatus.Text = "Status: Sensor A Recursive Search completed";
        }

        private void btnSearchBIter_Click(object sender, RoutedEventArgs e)
        {
            // 1) Validate integer input
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            // 2) Binary search only works on sorted data
            if (!IsSorted(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // 3) Last valid index
            int max = NumberOfNodes(sensorB) - 1;

            // 4) Measure performance in ticks
            var sw = Stopwatch.StartNew();
            int index = BinarySearchIterative(sensorB, value, 0, max);
            sw.Stop();

            // 5) Display ticks
            txtTicksBIter.Text = sw.ElapsedTicks + " ticks";

            // 6) Refresh ListBox
            DisplayListboxData(sensorB, lbSensorB);

            // 7) Highlight target ±2 neighbours
            HighlightIndexWithNeighbours(lbSensorB, index);

            txtStatus.Text = "Status: Sensor B Iterative Search completed";
        }

        // =========================================================
        // Sensor B — Binary Search (Recursive)
        // Requirement:
        // - Validate integer input
        // - Check data is sorted before Binary Search
        // - Measure processing time in TICKS
        // - Highlight found value + 2 neighbours on each side
        // =========================================================
        private void btnSearchBRec_Click(object sender, RoutedEventArgs e)
        {
            // 1) Validate integer input (Sensor B textbox)
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            // 2) Binary Search only works on sorted data
            if (!IsSorted(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // 3) Set correct maximum index (last valid index)
            int max = NumberOfNodes(sensorB) - 1;

            // 4) Measure performance in ticks
            var sw = Stopwatch.StartNew();
            int index = BinarySearchRecursive(sensorB, value, 0, max);
            sw.Stop();

            // 5) Display ticks (required)
            txtTicksBRec.Text = sw.ElapsedTicks + " ticks";

            // 6) Refresh ListBox contents
            DisplayListboxData(sensorB, lbSensorB);

            // 7) Highlight target ±2 neighbours (clamped inside helper)
            HighlightIndexWithNeighbours(lbSensorB, index);

            txtStatus.Text = "Status: Sensor B Recursive Search completed";
        }


        // =========================================================
        // Input Validation: Integer-only TextBox (blocks letters/spaces)
        // Works for typing and for paste
        // =========================================================
        private void IntegerOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only digits 0-9
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void IntegerOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!text.All(char.IsDigit))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}