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

        public MainWindow()
        {
            InitializeComponent();
        }

        // =========================================================
        // Q4.2 – LoadData()
        // - Name must be LoadData
        // - No parameters, return void
        // - Create Galileo instance inside method
        // - Hardcoded size 400
        // - Validate Sigma (10–20) and Mu (35–75)
        // =========================================================
        private void LoadData()
        {
            const int SIZE = 400; // Required fixed dataset size

            // Parse Sigma and Mu from UI
            if (!double.TryParse(txtSigma.Text, out double sigma) ||
                !double.TryParse(txtMu.Text, out double mu))
            {
                MessageBox.Show("Invalid numeric input for Sigma or Mu.");
                return;
            }

            // Sigma range check (10–20)
            if (sigma < 10 || sigma > 20)
            {
                MessageBox.Show("Sigma must be between 10 and 20.");
                return;
            }

            // Mu range check (35–75)
            if (mu < 35 || mu > 75)
            {
                MessageBox.Show("Mu must be between 35 and 75.");
                return;
            }

            // Clear previous readings before loading new ones
            sensorA.Clear();
            sensorB.Clear();

            // Galileo6 DLL instance created inside method 
            ReadData galileo = new ReadData();

            // Load exactly 400 values into each LinkedList 
            for (int i = 0; i < SIZE; i++)
            {
                sensorA.AddLast(galileo.SensorA(mu, sigma));
                sensorB.AddLast(galileo.SensorB(mu, sigma));
            }
        }

        // =========================================================
        // Q4.5 – NumberOfNodes()
        // - Input: LinkedList<double>
        // - Output: int
        // - Must count manually 
        // =========================================================
        private int NumberOfNodes(LinkedList<double> list)
        {
            int count = 0;

            // Walk nodes one by one and count them
            for (var node = list.First; node != null; node = node.Next)
                count++;

            return count;
        }

        // =========================================================
        // Q4.6 – DisplayListboxData()
        // - Input: LinkedList<double>, ListBox
        // - Output: void
        // =========================================================
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            // Clear old UI values
            target.Items.Clear();

            // Show values formatted to 4 decimals (matches Galileo spec)
            foreach (double value in list)
                target.Items.Add(value.ToString("F4"));
        }

        // =========================================================
        // Q4.3 – ShowAllSensorData()
        // - No parameters, return void
        // - Must show BOTH sensors in ListView (2 columns)
        // =========================================================
        private void ShowAllSensorData()
        {
            // Clear existing rows
            lvSensors.Items.Clear();

            // Start at first node in each LinkedList
            var a = sensorA.First;
            var b = sensorB.First;

            // Walk both lists together and display side-by-side
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
        // Q4.4 – Load Data Button
        // Must call:
        // - LoadData()
        // - DisplayListboxData() for both sensors
        // - ShowAllSensorData()
        // =========================================================
        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            // 1) Load sensor values
            LoadData();

            // 2) Display each sensor in its ListBox
            DisplayListboxData(sensorA, lbSensorA);
            DisplayListboxData(sensorB, lbSensorB);

            // 3) Display both sensors in ListView
            ShowAllSensorData();

            txtStatus.Text = "Status: Data loaded successfully";
        }

        // =========================================================
        // Q4.7 – SelectionSort() (Appendix Pseudocode)
        // Return type must be bool
        // =========================================================
        private bool SelectionSort(LinkedList<double> list)
        {
            if (list == null) return false;

            int max = NumberOfNodes(list);
            if (max < 2) return false;

            for (int i = 0; i < max - 1; i++)
            {
                int min = i;

                for (int j = i + 1; j < max; j++)
                {
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j;
                }

                // Get node at index i (manual walk, no helpers)
                LinkedListNode<double>? nodeI = list.First;
                for (int k = 0; k < i && nodeI != null; k++)
                    nodeI = nodeI.Next;

                // Get node at index min (manual walk, no helpers)
                LinkedListNode<double>? nodeMin = list.First;
                for (int k = 0; k < min && nodeMin != null; k++)
                    nodeMin = nodeMin.Next;

                if (nodeI == null || nodeMin == null) return false;

                // Swap values
                double temp = nodeMin.Value;
                nodeMin.Value = nodeI.Value;
                nodeI.Value = temp;
            }

            return true;
        }

        // =========================================================
        // Q4.8 – InsertionSort() (Appendix Pseudocode)
        // Return type must be bool
        // =========================================================
        private bool InsertionSort(LinkedList<double> list)
        {
            if (list == null) return false;

            int max = NumberOfNodes(list);
            if (max < 2) return false;

            for (int i = 0; i < max - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    if (list.ElementAt(j - 1) > list.ElementAt(j))
                    {
                        // Get node at index (j-1)
                        LinkedListNode<double>? prevNode = list.First;
                        for (int k = 0; k < j - 1 && prevNode != null; k++)
                            prevNode = prevNode.Next;

                        if (prevNode == null || prevNode.Next == null) return false;

                        // Node at index j is the next node
                        LinkedListNode<double> curNode = prevNode.Next;

                        // Swap adjacent values
                        double temp = prevNode.Value;
                        prevNode.Value = curNode.Value;
                        curNode.Value = temp;
                    }
                }
            }

            return true;
        }

        // =========================================================
        // Q4.9 – BinarySearchIterative() (Appendix)
        // Parameters: list, searchValue, minimum, maximum
        // Returns: found index OR nearest neighbour position
        //
        // Appendix note: it returns ++middle when found 
        // =========================================================
        private int BinarySearchIterative(LinkedList<double> list, int searchValue, int minimum, int maximum)
        {
            // while (minimum <= maximum - 1)
            while (minimum <= maximum - 1)
            {
                // middle = (minimum + maximum) / 2 
                int middle = (minimum + maximum) / 2;

                // Compare using rounded integer 
                int middleValue = (int)Math.Round(list.ElementAt(middle));

                // if found to return ++middle 
                if (searchValue == middleValue)
                    return ++middle;

                // else if smaller to search left
                else if (searchValue < middleValue)
                    maximum = middle - 1;
                else
                    // else to search right
                    minimum = middle + 1;
            }

            // not found then return minimum (nearest neighbour / insertion position)
            return minimum;
        }

        // =========================================================
        // Q4.10 – BinarySearchRecursive() (Appendix)
        // Returns: found index OR nearest neighbour position
        // =========================================================
        public int BinarySearchRecursive(LinkedList<double> list, int searchValue, int minimum, int maximum)
        {
            // if (minimum <= maximum - 1)
            if (minimum <= maximum - 1)
            {
                int middle = (minimum + maximum) / 2;

                int middleValue = (int)Math.Round(list.ElementAt(middle));

                if (searchValue == middleValue)
                    return middle;

                else if (searchValue < middleValue)
                    return BinarySearchRecursive(list, searchValue, minimum, middle - 1);
                else
                    return BinarySearchRecursive(list, searchValue, middle + 1, maximum);
            }

            return minimum;
        }

        // =========================================================
        // Q4.11 – Sort Button Methods 
        // =========================================================
        private void btnSelSortA_Click(object sender, RoutedEventArgs e)
        {
            var sw = Stopwatch.StartNew();     // Start timing before algorithm
            SelectionSort(sensorA);            // Run algorithm
            sw.Stop();                         // Stop timing sfter algorithm

            txtMsSelA.Text = sw.ElapsedMilliseconds + " ms"; // Display milliseconds

            DisplayListboxData(sensorA, lbSensorA); // Refresh ListBox
            ShowAllSensorData();                    // Refresh ListView

            txtStatus.Text = "Status: Sensor A Selection Sort completed";
        }

        // =========================================================
        // Q4.11 – SENSOR A Insertion Sort Button
        // - Measure time (milliseconds)
        // - Run Insertion Sort
        // - Refresh UI displays
        // =========================================================
        private void btnInsSortA_Click(object sender, RoutedEventArgs e)
        {
            // Start timing
            var sw = Stopwatch.StartNew();

            // Execute sort
            InsertionSort(sensorA);

            // Stop timing
            sw.Stop();

            // Display milliseconds
            txtMsInsA.Text = sw.ElapsedMilliseconds + " ms";

            // Refresh UI
            DisplayListboxData(sensorA, lbSensorA);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor A Insertion Sort completed";
        }

        // =========================================================
        // Q4.11 – SENSOR B Selection Sort Button
        // - Measure time (milliseconds)
        // - Run Selection Sort
        // - Refresh UI displays
        // =========================================================
        private void btnSelSortB_Click(object sender, RoutedEventArgs e)
        {
            // Start timing
            var sw = Stopwatch.StartNew();

            // Execute sort
            SelectionSort(sensorB);

            // Stop timing
            sw.Stop();

            // Display milliseconds
            txtMsSelB.Text = sw.ElapsedMilliseconds + " ms";

            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            // Status message
            txtStatus.Text = "Status: Sensor B Selection Sort completed";
        }

        // =========================================================
        // Q4.11 – SENSOR B Insertion Sort Button
        // - Measure time (milliseconds)
        // - Run Insertion Sort
        // - Refresh UI displays
        // =========================================================
        private void btnInsSortB_Click(object sender, RoutedEventArgs e)
        {
            // Start timing
            var sw = Stopwatch.StartNew();

            // Execute sort
            InsertionSort(sensorB);

            // Stop timing
            sw.Stop();

            // Display milliseconds
            txtMsInsB.Text = sw.ElapsedMilliseconds + " ms";

            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor B Insertion Sort completed";
        }

        // =========================================================
        // Q4.12 – Search Button Methods (ticks)
        // Requirements:
        // - Validate integer input
        // - Ensure list sorted before searching 
        // - Start stopwatch before search, stop after
        // - Display ticks
        // - Highlight target and neighbours (2) inline
        // =========================================================

        private void btnSearchAIter_Click(object sender, RoutedEventArgs e)
        {
            // Validate integer input
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            // Ensure data exists
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Inline sorted check (rounded int order)
            var firstNode = sensorA.First;
            if (firstNode == null)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            bool sorted = true;
            int prev = (int)Math.Round(firstNode.Value);
            var node = firstNode.Next;

            // Check list is sorted in ascending order 
            while (node != null)
            {
                int cur = (int)Math.Round(node.Value);

                // If current value is smaller so data is not sorted
                if (cur < prev)
                {
                    sorted = false;
                    break;
                }

                // Move forward in list
                prev = cur;
                node = node.Next;
            }

            // Stop search if data is not sorted
            if (!sorted)
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // Maximum passed is "number of nodes" as per appendix 
            int max = NumberOfNodes(sensorA);

            // Time the search in ticks
            var sw = Stopwatch.StartNew();
            int result = BinarySearchIterative(sensorA, value, 0, max);
            sw.Stop();

            txtTicksAIter.Text = sw.ElapsedTicks + " ticks";

            // Refresh list
            DisplayListboxData(sensorA, lbSensorA);

            // Convert result to ListBox index:
            // - If found do result is 1-based (because ++middle), so index = result - 1
            // - If not found show result is minimum (0-based insertion position)
            int index;
            if (result >= 1 && result <= lbSensorA.Items.Count)
                index = result - 1;
            else
                index = result;

            // Highlight found value only
            if (lbSensorA.Items.Count > 0)
            {
                // Keep index inside valid bounds
                if (index < 0) index = 0;
                if (index >= lbSensorA.Items.Count)
                    index = lbSensorA.Items.Count - 1;

                // Clear previous selection
                // lbSensorA.SelectedItems.Clear();

                // Highlight only target item
                lbSensorA.SelectedItem = lbSensorA.Items[index];

                // Scroll selected item into view
                lbSensorA.ScrollIntoView(lbSensorA.Items[index]);


            }

            txtStatus.Text = "Status: Sensor A Iterative Search completed";
        }

        private void btnSearchARec_Click(object sender, RoutedEventArgs e)
        {
            // =========================================================
            // Q4.12 – SENSOR A Recursive Binary Search Button
            //
            // Requirement:
            // - Validate integer input
            // - Ensure data exists
            // - Ensure data is sorted before binary search
            // - Start stopwatch BEFORE search
            // - Stop stopwatch AFTER search
            // - Display elapsed ticks
            // - Highlight found value and neighbours (±2)
            // =========================================================

            // ---------------------------------------------------------
            // Step 1: Validate integer input from textbox
            // Binary search requires numeric integer input
            // ---------------------------------------------------------
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            // ---------------------------------------------------------
            // Step 2: Ensure sensor data exists
            // Cannot search an empty LinkedList
            // ---------------------------------------------------------
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // ---------------------------------------------------------
            // Step 3: Inline sorted check
            // Binary search only works on ascending sorted data
            // Uses rounded integer comparison 
            // ---------------------------------------------------------
            var firstNode = sensorA.First;

            if (firstNode == null)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            bool sorted = true; // Assume sorted until proven otherwise

            // Store first value as baseline comparison
            int prev = (int)Math.Round(firstNode.Value);

            // Start checking from the second node
            var node = firstNode.Next;

            // Traverse entire list and verify ascending order
            while (node != null)
            {
                int cur = (int)Math.Round(node.Value);

                // If current value is smaller so data is not sorted
                if (cur < prev)
                {
                    sorted = false;
                    break;
                }

                // Move forward in list
                prev = cur;
                node = node.Next;
            }

            // Stop search if data is not sorted
            if (!sorted)
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // ---------------------------------------------------------
            // Step 4: Determine maximum range for search
            // Appendix requires number of nodes as maximum argument
            // ---------------------------------------------------------
            int max = NumberOfNodes(sensorA);

            // ---------------------------------------------------------
            // Step 5: Start stopwatch before recursive search
            // Measure execution time in ticks 
            // ---------------------------------------------------------
            var sw = Stopwatch.StartNew();

            // Execute recursive binary search
            int index = BinarySearchRecursive(sensorA, value, 0, max);

            // Stop timing immediately after algorithm completes
            sw.Stop();

            // Display elapsed ticks in textbox
            txtTicksARec.Text = sw.ElapsedTicks + " ticks";

            // ---------------------------------------------------------
            // Step 6: Refresh ListBox display after search
            // ---------------------------------------------------------
            DisplayListboxData(sensorA, lbSensorA);

            // ---------------------------------------------------------
            // Step 7: Highlight found value only
            // ---------------------------------------------------------
            if (lbSensorA.Items.Count > 0)
            {
                // Clamp index to valid ListBox bounds
                if (index < 0) index = 0;
                if (index >= lbSensorA.Items.Count)
                    index = lbSensorA.Items.Count - 1;

                // Clear previous selection
                // lbSensorA.SelectedItems.Clear();

                // Highlight only the target item
                lbSensorA.SelectedItem = lbSensorA.Items[index];

                // Scroll ListBox so selected item is visible
                lbSensorA.ScrollIntoView(lbSensorA.Items[index]);


            }

            // ---------------------------------------------------------
            // Step 8: Display completion status
            // ---------------------------------------------------------
            txtStatus.Text = "Status: Sensor A Recursive Search completed";
        }

        private void btnSearchBIter_Click(object sender, RoutedEventArgs e)
        {
            // =========================================================
            // Q4.12 – SENSOR B Iterative Binary Search Button
            //
            // Requirement:
            // - Validate integer input
            // - Ensure data exists
            // - Ensure data is sorted before binary search
            // - Start stopwatch before search
            // - Stop stopwatch after search
            // - Display elapsed ticks
            // - Highlight found value and neighbours (2)
            // =========================================================

            // ---------------------------------------------------------
            // Step 1: Validate integer input from textbox
            // Binary search requires numeric integer input
            // ---------------------------------------------------------
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            // ---------------------------------------------------------
            // Step 2: Ensure sensor data exists
            // Cannot search an empty LinkedList
            // ---------------------------------------------------------
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            // ---------------------------------------------------------
            // Step 3: Inline sorted check
            // Binary search requires ascending sorted data
            // Uses rounded integer comparison 
            // ---------------------------------------------------------
            var firstNode = sensorB.First;

            if (firstNode == null)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            bool sorted = true; // Assume sorted until proven otherwise

            // Store first value as baseline
            int prev = (int)Math.Round(firstNode.Value);

            // Start comparison from second node
            var node = firstNode.Next;

            // Traverse list to confirm ascending order
            while (node != null)
            {
                int cur = (int)Math.Round(node.Value);

                // If current value is smaller is not sorted
                if (cur < prev)
                {
                    sorted = false;
                    break;
                }

                // Move forward in list
                prev = cur;
                node = node.Next;
            }

            // Stop search if data is not sorted
            if (!sorted)
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // ---------------------------------------------------------
            // Step 4: Determine maximum range for search
            // Appendix requires number of nodes
            // ---------------------------------------------------------
            int max = NumberOfNodes(sensorB);

            // ---------------------------------------------------------
            // Step 5: Start stopwatch before iterative search
            // Measure execution time in ticks
            // ---------------------------------------------------------
            var sw = Stopwatch.StartNew();

            // Execute iterative binary search
            int result = BinarySearchIterative(sensorB, value, 0, max);

            // Stop timing immediately after search completes
            sw.Stop();

            // Display elapsed ticks in textbox
            txtTicksBIter.Text = sw.ElapsedTicks + " ticks";

            // ---------------------------------------------------------
            // Step 6: Refresh ListBox display
            // ---------------------------------------------------------
            DisplayListboxData(sensorB, lbSensorB);

            // ---------------------------------------------------------
            // Step 7: Convert returned index
            // Iterative search returns:
            // - ++middle (1-based) when found
            // - insertion position when not found
            // ---------------------------------------------------------
            int index;
            if (result >= 1 && result <= lbSensorB.Items.Count)
                index = result - 1;   // Convert to 0-based index
            else
                index = result;

            // ---------------------------------------------------------
            // Step 8: Highlight found value only
            // ---------------------------------------------------------
            if (lbSensorB.Items.Count > 0)
            {
                // Clamp index to valid bounds
                if (index < 0) index = 0;
                if (index >= lbSensorB.Items.Count)
                    index = lbSensorB.Items.Count - 1;

                // Clear old selection
                // lbSensorB.SelectedItems.Clear();

                // Highlight only the target item
                lbSensorB.SelectedItem = lbSensorB.Items[index];

                // Scroll so selected item is visible
                lbSensorB.ScrollIntoView(lbSensorB.Items[index]);

            }

            // ---------------------------------------------------------
            // Step 9: Display completion status
            // ---------------------------------------------------------
            txtStatus.Text = "Status: Sensor B Iterative Search completed";
        }

        private void btnSearchBRec_Click(object sender, RoutedEventArgs e)
        {
            // =========================================================
            // Q4.12 – SENSOR B Recursive Binary Search Button
            //
            // Requirement:
            // - Validate integer input
            // - Ensure data exists
            // - Ensure data is sorted before binary search
            // - Start stopwatch before search
            // - Stop stopwatch after search
            // - Display elapsed ticks
            // - Highlight found value and neighbours (2)
            // =========================================================

            // ---------------------------------------------------------
            // Step 1: Validate integer input from textbox
            // Binary search requires numeric integer value
            // ---------------------------------------------------------
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            // ---------------------------------------------------------
            // Step 2: Ensure LinkedList contains data
            // Binary search cannot run on empty list
            // ---------------------------------------------------------
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            // ---------------------------------------------------------
            // Step 3: Inline sorted check (required)
            // Binary search only works on sorted data
            // Comparison uses rounded values 
            // ---------------------------------------------------------
            var firstNode = sensorB.First;

            if (firstNode == null)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            bool sorted = true; // Assume sorted until proven otherwise

            // Store first value for comparison baseline
            int prev = (int)Math.Round(firstNode.Value);

            // Start checking from second node
            var node = firstNode.Next;

            // Traverse list and verify ascending order
            while (node != null)
            {
                int cur = (int)Math.Round(node.Value);
                if (cur < prev)
                {
                    sorted = false;
                    break;
                }

                // Move comparison forward
                prev = cur;
                node = node.Next;
            }

            // If list is not sorted, stop search
            if (!sorted)
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // ---------------------------------------------------------
            // Step 4: Determine maximum range for search
            // Appendix requires passing number of nodes
            // ---------------------------------------------------------
            int max = NumberOfNodes(sensorB);

            // ---------------------------------------------------------
            // Step 5: Start stopwatch before recursive search
            // Measure processing time in ticks 
            // ---------------------------------------------------------
            var sw = Stopwatch.StartNew();

            // Execute recursive binary search
            int index = BinarySearchRecursive(sensorB, value, 0, max);

            // Stop stopwatch immediately after algorithm completes
            sw.Stop();

            // Display elapsed ticks in textbox
            txtTicksBRec.Text = sw.ElapsedTicks + " ticks";

            // ---------------------------------------------------------
            // Step 6: Refresh ListBox display after search
            // ---------------------------------------------------------
            DisplayListboxData(sensorB, lbSensorB);

            // ---------------------------------------------------------
            // Step 7: Highlight search result and neighbours (2)
            // ---------------------------------------------------------
            // Refresh ListBox display
            DisplayListboxData(sensorB, lbSensorB);

            // Highlight the target item 
            if (lbSensorB.Items.Count > 0)
            {
                // Clamp index within valid ListBox range
                if (index < 0) index = 0;
                if (index >= lbSensorB.Items.Count)
                    index = lbSensorB.Items.Count - 1;

                // Clear previous selection
                //lbSensorB.SelectedItems.Clear();

                // Select only the target item
                lbSensorB.SelectedItem = lbSensorB.Items[index];

                // Scroll so the selected item is visible
                lbSensorB.ScrollIntoView(lbSensorB.Items[index]);

            }

            // ---------------------------------------------------------
            // Step 8: Display status message to user
            // ---------------------------------------------------------
            txtStatus.Text = "Status: Sensor B Recursive Search completed";
        }

        // =========================================================
        // Q4.13 – Input Validation
        // Allow int input only (digits 0–9)
        // =========================================================
        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Block non-digit characters
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // =========================================================
        // IntegerOnly_Pasting()
        // Blocks pasting non-numeric content
        // =========================================================
        private void IntegerOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            object? data = e.DataObject.GetData(typeof(string));
            string? text = data as string;

            // Cancel paste if null/empty or contains non-digits
            if (string.IsNullOrEmpty(text) || !text.All(char.IsDigit))
                e.CancelCommand();
        }
    }
}