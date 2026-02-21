using Galileo6;   // Assessment requirement: external Galileo6 DLL
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GalileoDataLab
{
    /// <summary>
    /// =========================================================
    /// Student: Kate Odabas P288004
    /// Galileo Data Processing Application
    /// Units: ICTPRG535 / ICTPRG547
    ///
    /// Purpose:
    /// - Load satellite data via Galileo6 DLL
    /// - Store data in LinkedList<double> (Sensor A and Sensor B)
    /// - Sort using Selection Sort and Insertion Sort
    /// - Search using Binary Search (Iterative and Recursive)
    /// - Measure performance (Sort: milliseconds, Search: ticks)
    /// =========================================================
    /// </summary>
    public partial class MainWindow : Window
    {
        // =========================================================
        // Q4.1 – Global Data Structures
        // Only 2 global variables allowed (Sensor A + Sensor B)
        // =========================================================
        private LinkedList<double> sensorA = new LinkedList<double>();
        private LinkedList<double> sensorB = new LinkedList<double>();


        // =========================================================
        // UI Setup (Constructor)
        // =========================================================
        public MainWindow()
        {
            InitializeComponent();

            // Addede to enable multiple selection for neighbour highlighting
            lbSensorA.SelectionMode = SelectionMode.Extended;
            lbSensorB.SelectionMode = SelectionMode.Extended;
        }


        // =========================================================
        // Q4.2 – LoadData()
        // Assessment Requirement:
        // - Method name must be LoadData
        // - No input parameters
        // - Return type = void
        // - Create Galileo6 DLL instance inside method
        // - Populate both LinkedLists (Sensor A + Sensor B)
        // - Hardcoded dataset size = 400
        // - Validate Sigma range (10–20)
        // - Validate Mu range (35–75)
        // =========================================================
        private void LoadData()
        {
            const int SIZE = 400;   //Fixed dataset size

            // -----------------------------------------------------
            // Validate numeric input (Sigma & Mu)
            // -----------------------------------------------------
            if (!double.TryParse(txtSigma.Text, out double sigma) ||
                !double.TryParse(txtMu.Text, out double mu))
            {
                MessageBox.Show("Invalid numeric input for Sigma or Mu.");
                return;
            }

            // -----------------------------------------------------
            // Sigma validation 
            // Range: 10 to 20
            // -----------------------------------------------------
            if (sigma < 10 || sigma > 20)
            {
                MessageBox.Show("Sigma must be between 10 and 20.");
                return;
            }

            // -----------------------------------------------------
            // Mu validation 
            // Range: 35 to 75
            // -----------------------------------------------------
            if (mu < 35 || mu > 75)
            {
                MessageBox.Show("Mu must be between 35 and 75.");
                return;
            }

            // -----------------------------------------------------
            // Clear existing sensor data before loading new values
            // -----------------------------------------------------
            sensorA.Clear();
            sensorB.Clear();

            // -----------------------------------------------------
            // Galileo6 DLL instance created
            // -----------------------------------------------------
            ReadData galileo = new ReadData();

            // -----------------------------------------------------
            // Generate exactly 400 readings for each sensor
            // SensorA -to LinkedList sensorA
            // SensorB -to LinkedList sensorB
            // -----------------------------------------------------
            for (int i = 0; i < SIZE; i++)
            {
                sensorA.AddLast(galileo.SensorA(mu, sigma));
                sensorB.AddLast(galileo.SensorB(mu, sigma));
            }
        }

        // =========================================================
        // Q4.5 – NumberOfNodes()
        // Assessment Requirement:
        // - Method name must be NumberOfNodes
        // - Input parameter: LinkedList<double>
        // - Return type: int
        // - Must manually count nodes (no direct Count usage)
        // Purpose:
        // - Returns total number of elements in the LinkedList
        // - Used by sorting and searching algorithms
        // =========================================================
        private int NumberOfNodes(LinkedList<double> list)
        {
            int count = 0;

            for (var node = list.First; node != null; node = node.Next)
                count++;

            return count;
        }
        // =========================================================
        // HELPER METHOD: GetNodeAt()
        // ---------------------------------------------------------
        // Purpose:
        // - Returns node at a specific index position
        // - Fixes sorting/search issues when duplicate values exist
        // Used in:
        // - Selection Sort
        // - Insertion Sort
        // - Binary Search 
        // =========================================================
        private LinkedListNode<double> GetNodeAt(LinkedList<double> list, int index)
        {
            // Safety check to prevent invalid index access
            if (index < 0 || index >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Start from first node
            var node = list.First;

            // Move forward until required index is reached
            for (int i = 0; i < index; i++)
                node = node!.Next;

            // Return node at requested position
            return node!;
        }
        // =========================================================
        // Q4.6 – DisplayListboxData()
        // Assessment Requirement:
        // - Method name must be DisplayListboxData
        // - Input parameters:
        //     1. LinkedList<double>
        //     2. ListBox target
        // - Return type: void
        //
        // Purpose:
        // - Displays LinkedList values inside the selected ListBox
        // - Used after sorting and searching operations
        // - Formats values to 4 decimal places (sensor display)
        // =========================================================
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            // Clear previous display content
            target.Items.Clear();

            // Add each LinkedList value to ListBox
            foreach (double value in list)
                target.Items.Add(value.ToString("F4"));
        }

        // =========================================================
        // Q4.3 – ShowAllSensorData()
        // Assessment Requirement:
        // - Method name must be ShowAllSensorData
        // - No input parameters
        // - Return type: void
        // - Display BOTH sensors in ListView
        // - Columns required: Sensor A + Sensor B
        //
        // Purpose:
        // - Displays paired sensor data side by side
        // - Used after LoadData()
        // - Used after sorting operations
        // =========================================================
        private void ShowAllSensorData()
        {
            // Clear previous ListView rows
            lvSensors.Items.Clear();

            // Start traversal at first node of each sensor list
            var a = sensorA.First;
            var b = sensorB.First;

            // Traverse both LinkedLists together
            while (a != null && b != null)
            {
                // Add combined row into ListView
                lvSensors.Items.Add(new
                {
                    SensorA = a.Value.ToString("F4"),
                    SensorB = b.Value.ToString("F4")
                });

                // Move to next nodes
                a = a.Next;
                b = b.Next;
            }
        }
        // =========================================================
        // HELPER METHOD – IsSorted()
        // Purpose:
        // - Checks whether LinkedList data is sorted
        // - Binary search requires ascending order data
        //
        // Important:
        // - Binary search compares rounded values (Math.Round)
        // - Sorted check must follow the same comparison rule
        // =========================================================
        private bool IsSorted(LinkedList<double> list)
        {
            // Empty or single-node list is always sorted
            if (list == null || list.Count < 2)
                return true;

            // Store first value as starting comparison
            double previous = Math.Round(list.First!.Value);

            // Start from second node
            var node = list.First!.Next;

            // Traverse list node-by-node
            while (node != null)
            {
                // Round current value to match binary search rule
                double current = Math.Round(node.Value);

                // If current value is smaller → list NOT sorted
                if (current < previous)
                    return false;

                // Move forward for next comparison
                previous = current;
                node = node.Next;
            }

            // No ordering issues detected
            return true;
        }

        // =========================================================
        // HELPER METHOD – HighlightIndexWithNeighbours()
        // Purpose:
        // - Highlights search result and neighbouring values (index ±2)
        // - Required for assessment search display behaviour
        //
        // Behaviour:
        // - Clamps index within valid range
        // - Highlights up to 5 values total
        // =========================================================
        private void HighlightIndexWithNeighbours(ListBox listBox, int index)
        {
            // Enable multiple highlighted items
            listBox.SelectionMode = SelectionMode.Extended;

            // Clear previous selection
            listBox.SelectedItems.Clear();

            // Safety check if list is empty
            if (listBox.Items.Count == 0)
                return;

            // Clamp index to valid bounds
            if (index < 0) index = 0;
            if (index >= listBox.Items.Count) index = listBox.Items.Count - 1;

            // Calculate neighbour range (±2)
            int start = Math.Max(0, index - 2);
            int end = Math.Min(listBox.Items.Count - 1, index + 2);

            // Highlight target value and surrounding nodes
            for (int i = start; i <= end; i++)
                listBox.SelectedItems.Add(listBox.Items[i]);

            // Scroll list so highlighted item is visible
            listBox.ScrollIntoView(listBox.Items[index]);

            // Set focus for clear UI feedback
            listBox.Focus();
        }

        // =========================================================
        // Q4.4 – Load Data Button Method
        // Assessment Requirement:
        // - Button click must call LoadData()
        // - Display data in both ListBoxes
        // - Display data in ListView
        //
        // Steps:
        // 1. Load data using Galileo DLL
        // 2. Display Sensor A list
        // 3. Display Sensor B list
        // 4. Show combined sensor view
        // 5. Update status message
        // =========================================================
        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Populate LinkedLists with sensor data
            LoadData();

            // Step 2: Show Sensor A values in ListBox
            DisplayListboxData(sensorA, lbSensorA);

            // Step 3: Show Sensor B values in ListBox
            DisplayListboxData(sensorB, lbSensorB);

            // Step 4: Display both sensors in ListView columns
            ShowAllSensorData();

            // Step 5: User feedback message
            txtStatus.Text = "Status: Data loaded successfully";
        }

        // =========================================================
        // Q4.7 – SelectionSort()
        // Assessment Requirement:
        // - Implement Selection Sort using Appendix pseudocode
        // - Return type: bool
        // - Sort LinkedList in ascending order
        // =========================================================
        private bool SelectionSort(LinkedList<double> list)
        {
            // If list has fewer than 2 items, sorting is not required
            if (list.Count < 2) return false;

            // Get total number of nodes 
            int max = NumberOfNodes(list);

            // Outer loop: moves sorted boundary forward
            for (int i = 0; i < max - 1; i++)
            {
                // Assume current index is minimum value
                int min = i;

                // Inner loop: search for smallest value in remaining list
                for (int j = i + 1; j < max; j++)
                {
                    // Compare current value against known minimum
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j; // Update minimum index
                }

                // prevents duplicate issues
                var nodeMin = GetNodeAt(list, min);
                var nodeI = GetNodeAt(list, i);

                // Swap values between current position and minimum
                double temp = nodeMin.Value;
                nodeMin.Value = nodeI.Value;
                nodeI.Value = temp;
            }

            // Sorting completed successfully
            return true;
        }

        // =========================================================
        // Q4.8 – InsertionSort()
        // Assessment Requirement:
        // - Implement Insertion Sort using Appendix pseudocode
        // - Return type: bool
        // - Sort LinkedList in ascending order
        // =========================================================
        private bool InsertionSort(LinkedList<double> list)
        {
            // If list has fewer than 2 items, sorting not required
            if (list.Count < 2) return false;

            // Get total number of nodes in list
            int max = NumberOfNodes(list);

            // Outer loop controls sorted portion growth
            for (int i = 0; i < max - 1; i++)
            {
                // Inner loop moves current item backwards into correct position
                for (int j = i + 1; j > 0; j--)
                {
                    // Compare adjacent values
                    if (list.ElementAt(j - 1) > list.ElementAt(j))
                    {
                        // Retrieve nodes safely using helper method
                        var current = GetNodeAt(list, j);
                        var previous = GetNodeAt(list, j - 1);

                        // Swap adjacent values
                        double temp = previous.Value;
                        previous.Value = current.Value;
                        current.Value = temp;
                    }
                }
            }

            // Sorting finished
            return true;
        }

        // =========================================================
        // Q4.9 – BinarySearchIterative()
        // Assessment Requirement:
        // - Iterative Binary Search implementation
        // - Parameters: list, searchValue, minimum, maximum
        // - maximum must be the last index (Count - 1)
        // - Returns found index or nearest neighbour index
        // =========================================================
        private int BinarySearchIterative(
            LinkedList<double> list,
            double searchValue,
            int minimum,
            int maximum)
        {
            // Continue searching while search range is valid
            while (minimum <= maximum)
            {
                // Calculate middle index safely
                int middle = minimum + ((maximum - minimum) / 2);

                // Round value to match assessment comparison rule
                double middleValue = Math.Round(list.ElementAt(middle));

                // Exact match found
                if (searchValue == middleValue)
                    return middle;

                // Search left half if value is smaller
                if (searchValue < middleValue)
                    maximum = middle - 1;
                else
                    // Otherwise search right half
                    minimum = middle + 1;
            }

            // Value not found to return nearest neighbour index
            return minimum;
        }

        // =========================================================
        // Q4.10 – BinarySearchRecursive()
        // Assessment Requirement:
        // - Recursive Binary Search implementation
        // - Parameters: list, searchValue, minimum, maximum
        // - maximum must be last index (Count - 1)
        // - Returns found index or nearest neighbour index
        // =========================================================
        public int BinarySearchRecursive(
            LinkedList<double> list,
            double searchValue,
            int minimum,
            int maximum)
        {
            // if value not found, return nearest position
            if (minimum > maximum)
                return minimum;

            // Calculate middle index safely
            int middle = minimum + ((maximum - minimum) / 2);

            // Round value to match search comparison rule
            double middleValue = Math.Round(list.ElementAt(middle));

            // Exact match found
            if (searchValue == middleValue)
                return middle;

            // Search left half recursively
            if (searchValue < middleValue)
                return BinarySearchRecursive(list, searchValue, minimum, middle - 1);

            // Otherwise search right half recursively
            return BinarySearchRecursive(list, searchValue, middle + 1, maximum);
        }
        // =========================================================
        // Q4.11 – Sort Button Methods
        // Assessment Requirement:
        // - Start stopwatch before sort
        // - Execute Selection or Insertion Sort
        // - Stop stopwatch after sort
        // - Display processing time (milliseconds)
        // - Refresh ListBox and ListView displays
        // =========================================================

        // =========================================================
        // SENSOR A – Selection Sort Button
        // Purpose:
        // - Executes SelectionSort() on Sensor A
        // - Measures execution time in milliseconds
        // - Refreshes UI after sorting
        // =========================================================
        private void btnSelSortA_Click(object sender, RoutedEventArgs e)
        {
            // Start stopwatch BEFORE sorting 
            var sw = Stopwatch.StartNew();

            // Execute Selection Sort algorithm
            SelectionSort(sensorA);

            // Stop timing after sort completes
            sw.Stop();

            // Display elapsed time in milliseconds
            txtMsSelA.Text = sw.ElapsedMilliseconds + " ms";

            // Refresh Sensor A ListBox display
            DisplayListboxData(sensorA, lbSensorA);

            // Refresh combined Sensor A + B ListView
            ShowAllSensorData();
            txtStatus.Text = "Status: Sensor A Selection Sort completed";
        }

        // =========================================================
        // SENSOR A - Insertion Sort Button
        // Measures execution time in milliseconds
        // Updates UI after sorting
        // =========================================================
        private void btnInsSortA_Click(object sender, RoutedEventArgs e)
        {
            // Start stopwatch before sorting
            var sw = Stopwatch.StartNew();

            // Call InsertionSort algorithm
            InsertionSort(sensorA);

            // Stop timing
            sw.Stop();

            // Display elapsed milliseconds
            txtMsInsA.Text = sw.ElapsedMilliseconds + " ms";

            // Update Sensor A ListBox
            DisplayListboxData(sensorA, lbSensorA);

            // Update ListView with both sensors
            ShowAllSensorData();
            txtStatus.Text = "Status: Sensor A Insertion Sort completed";
        }

        // =========================================================
        // SENSOR B - Selection Sort Button
        // Executes SelectionSort on Sensor B
        // Records processing time
        // =========================================================
        private void btnSelSortB_Click(object sender, RoutedEventArgs e)
        {
            // Start timing before sort
            var sw = Stopwatch.StartNew();

            // Execute selection sort
            SelectionSort(sensorB);

            // Stop timing
            sw.Stop();

            // Display milliseconds in textbox
            txtMsSelB.Text = sw.ElapsedMilliseconds + " ms";

            // Refresh Sensor B ListBox
            DisplayListboxData(sensorB, lbSensorB);

            // Refresh paired sensor display
            ShowAllSensorData();
            txtStatus.Text = "Status: Sensor B Selection Sort completed";
        }

        // =========================================================
        // SENSOR B - Insertion Sort Button
        // Executes insertion sort and records time
        // Updates UI components after completion
        // =========================================================
        private void btnInsSortB_Click(object sender, RoutedEventArgs e)
        {
            // Start stopwatch
            var sw = Stopwatch.StartNew();

            // Execute insertion sort algorithm
            InsertionSort(sensorB);

            // Stop timing
            sw.Stop();

            // Display sort time in milliseconds
            txtMsInsB.Text = sw.ElapsedMilliseconds + " ms";

            // Refresh Sensor B ListBox data
            DisplayListboxData(sensorB, lbSensorB);

            // Refresh ListView display
            ShowAllSensorData();
            txtStatus.Text = "Status: Sensor B Insertion Sort completed";
        }

        // =========================================================
        // Q4.12 – Search Button Methods (ticks)
        // - Validate integer input
        // - Ensure list sorted before binary search
        // - Measure ticks
        // - Highlight index ±2
        // =========================================================

        // SENSOR A – Binary Search Iterative
        private void btnSearchAIter_Click(object sender, RoutedEventArgs e)
        {
            // Validate numeric integer input
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            // Binary search requires sorted data
            if (!IsSorted(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // Determine maximum index in list
            int max = NumberOfNodes(sensorA) - 1;

            // Start stopwatch before search
            var sw = Stopwatch.StartNew();

            // Perform iterative binary search
            int index = BinarySearchIterative(sensorA, value, 0, max);

            // Stop timing after search completes
            sw.Stop();

            // Display execution time in ticks
            txtTicksAIter.Text = sw.ElapsedTicks + " ticks";

            // Refresh list display
            DisplayListboxData(sensorA, lbSensorA);

            // Highlight result and neighbour values
            HighlightIndexWithNeighbours(lbSensorA, index);

            // Update status message
            txtStatus.Text = "Status: Sensor A Iterative Search completed";
        }
        // =========================================================
        // SENSOR A - Binary Search Recursive
        // Uses recursive binary search algorithm
        // Displays elapsed ticks
        // =========================================================
        private void btnSearchARec_Click(object sender, RoutedEventArgs e)
        {
            // Validate integer input
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            // Ensure data is sorted before search
            if (!IsSorted(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // Maximum index for search range
            int max = NumberOfNodes(sensorA) - 1;

            // Start timer
            var sw = Stopwatch.StartNew();

            // Execute recursive binary search
            int index = BinarySearchRecursive(sensorA, value, 0, max);

            // Stop timer
            sw.Stop();

            // Display search time (ticks)
            txtTicksARec.Text = sw.ElapsedTicks + " ticks";

            // Refresh ListBox display
            DisplayListboxData(sensorA, lbSensorA);

            // Highlight result up down 2 neighbours
            HighlightIndexWithNeighbours(lbSensorA, index);

            // Status feedback
            txtStatus.Text = "Status: Sensor A Recursive Search completed";
        }
        // =========================================================
        // SENSOR B - Binary Search Iterative
        // Performs iterative search on Sensor B
        // Measures execution in ticks
        // =========================================================
        private void btnSearchBIter_Click(object sender, RoutedEventArgs e)
        {
            // Validate input value
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            // Check sorting requirement
            if (!IsSorted(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // Define search upper limit
            int max = NumberOfNodes(sensorB) - 1;

            // Start stopwatch before search
            var sw = Stopwatch.StartNew();

            // Execute iterative binary search
            int index = BinarySearchIterative(sensorB, value, 0, max);

            // Stop timing
            sw.Stop();

            // Display ticks in UI
            txtTicksBIter.Text = sw.ElapsedTicks + " ticks";

            // Refresh display
            DisplayListboxData(sensorB, lbSensorB);

            // Highlight target area
            HighlightIndexWithNeighbours(lbSensorB, index);

            // Update status
            txtStatus.Text = "Status: Sensor B Iterative Search completed";
        }

        // =========================================================
        // SENSOR B - Binary Search Recursive
        // ---------------------------------------------------------
        // Performs recursive search on Sensor B
        // Shows elapsed ticks
        // =========================================================
        private void btnSearchBRec_Click(object sender, RoutedEventArgs e)
        {
            // Validate integer input
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            // Ensure data is sorted
            if (!IsSorted(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // Set maximum index
            int max = NumberOfNodes(sensorB) - 1;

            // Start stopwatch
            var sw = Stopwatch.StartNew();

            // Execute recursive search
            int index = BinarySearchRecursive(sensorB, value, 0, max);

            // Stop stopwatch
            sw.Stop();

            // Show execution ticks
            txtTicksBRec.Text = sw.ElapsedTicks + " ticks";

            // Refresh display data
            DisplayListboxData(sensorB, lbSensorB);

            // Highlight result + neighbours
            HighlightIndexWithNeighbours(lbSensorB, index);

            // User feedback message
            txtStatus.Text = "Status: Sensor B Recursive Search completed";
        }

        // =========================================================
        // Q4.13 – Input Validation
        // Allow int input only
        // Block invalid typing and paste actions
        // Allows digits only (0–9)
        // =========================================================
        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // If character is not a digit to block input
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // =========================================================
        // IntegerOnly_Pasting()
        // ---------------------------------------------------------
        // Triggered when user pastes text into textbox
        // Ensures pasted content contains digits only
        // =========================================================
        private void IntegerOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            // Check if pasted data contains text
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                // Cancel paste if any character is not numeric
                if (!text.All(char.IsDigit))
                    e.CancelCommand();
            }
            else
            {
                // Cancel paste if data format is not text
                e.CancelCommand();
            }
        }
    }
}