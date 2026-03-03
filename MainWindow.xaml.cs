using Galileo6;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GalileoDataLab
{
    /// <summary>
    /// =========================================================
    /// Student: Kate Odabas – P288004
    /// Units: ICTPRG535 / ICTPRG547
    ///
    /// Purpose:
    /// - Load 400 readings from Galileo6 DLL into LinkedList<double>
    /// - Sort using Selection Sort and Insertion Sort
    /// - Search using Binary Search (Iterative and Recursive)
    /// - Measure performance:
    ///     Sort: milliseconds
    ///     Search: ticks
    ///
    /// Lecturer highlight requirement:
    /// - If target=32 do highlight ALL values 32.xxxx.
    /// - If no 32.xxxx do show NOT FOUND and highlight closest group value(s).
    /// =========================================================
    /// </summary>
    public partial class MainWindow : Window
    {
        // =========================================================
        // Q4.1 – Only 2 global variables allowed
        // =========================================================
        private LinkedList<double> sensorA = new LinkedList<double>();
        private LinkedList<double> sensorB = new LinkedList<double>();

        public MainWindow()
        {
            InitializeComponent();

        }
        // =========================================================
        // Q4.2 – LoadData()
        // =========================================================
        private void LoadData()
        {
            const int SIZE = 400; // fixed dataset size required

            // Step 1: validate sigma and mu input
            if (!double.TryParse(txtSigma.Text, out double sigma) ||
                !double.TryParse(txtMu.Text, out double mu))
            {
                MessageBox.Show("Invalid numeric input for Sigma or Mu.");
                return;
            }

            // Step 2: validate ranges
            if (sigma < 10 || sigma > 20)
            {
                MessageBox.Show("Sigma must be between 10 and 20.");
                return;
            }

            if (mu < 35 || mu > 75)
            {
                MessageBox.Show("Mu must be between 35 and 75.");
                return;
            }

            // Step 3: clear previous data
            sensorA.Clear();
            sensorB.Clear();

            // Step 4: create Galileo instance inside this method (required)
            ReadData g = new ReadData();

            // Step 5: load 400 values into each LinkedList
            for (int i = 0; i < SIZE; i++)
            {
                sensorA.AddLast(g.SensorA(mu, sigma));
                sensorB.AddLast(g.SensorB(mu, sigma));
            }
        }

        // =========================================================
        // Q4.5 – NumberOfNodes()
        // =========================================================
        private int NumberOfNodes(LinkedList<double> list)
        {
            int count = 0;

            // walk node-by-node and count manually
            for (var node = list.First; node != null; node = node.Next)
                count++;

            return count;
        }

        // =========================================================
        // Q4.6 – DisplayListboxData()
        // =========================================================
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            // clear old display
            target.Items.Clear();

            // add formatted values
            foreach (double v in list)
                target.Items.Add(v.ToString("F4"));
        }

        // =========================================================
        // Q4.3 – ShowAllSensorData()
        // =========================================================
        private void ShowAllSensorData()
        {
            lvSensors.Items.Clear();

            var a = sensorA.First;
            var b = sensorB.First;

            // show values side-by-side
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
        // =========================================================
        private void btnLoadData_Click(object sender, RoutedEventArgs e)
        {
            LoadData();

            DisplayListboxData(sensorA, lbSensorA);
            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            ResetUiAfterLoad();

            txtStatus.Text = "Status: Data loaded successfully";
        }

        // =========================================================
        // Q4.7 – SelectionSort()
        // =========================================================
        private bool SelectionSort(LinkedList<double> list)
        {
            if (list == null) return false;

            int max = NumberOfNodes(list);
            if (max < 2) return false;

            // move boundary of unsorted part
            for (int i = 0; i < max - 1; i++)
            {
                int min = i;

                // find smallest in remaining list
                for (int j = i + 1; j < max; j++)
                {
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j;
                }

                // move to node at i
                var nodeI = list.First;
                for (int k = 0; k < i && nodeI != null; k++)
                    nodeI = nodeI.Next;

                // move to node at min
                var nodeMin = list.First;
                for (int k = 0; k < min && nodeMin != null; k++)
                    nodeMin = nodeMin.Next;

                if (nodeI == null || nodeMin == null) return false;

                // swap values
                double temp = nodeMin.Value;
                nodeMin.Value = nodeI.Value;
                nodeI.Value = temp;
            }

            return true;
        }

        // =========================================================
        // Q4.8 – InsertionSort()
        // =========================================================
        private bool InsertionSort(LinkedList<double> list)
        {
            if (list == null) return false;

            int max = NumberOfNodes(list);
            if (max < 2) return false;

            // take each element and insert into sorted portion
            for (int i = 0; i < max - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    // if out of order do swap neighbours
                    if (list.ElementAt(j - 1) > list.ElementAt(j))
                    {
                        var prev = list.First;
                        for (int k = 0; k < j - 1 && prev != null; k++)
                            prev = prev.Next;

                        if (prev == null || prev.Next == null) return false;

                        var cur = prev.Next;

                        double temp = prev.Value;
                        prev.Value = cur.Value;
                        cur.Value = temp;
                    }
                }
            }

            return true;
        }

        // =========================================================
        // Sorted() – Checks sort order using integer group compare
        // Why needed:
        // - Binary Search only works on sorted data.
        // - Search comparesint only, so list must be sorted
        // =========================================================
        private bool Sorted(LinkedList<double> list)
        {
            var first = list.First;
            if (first == null) return false;

            int prev = (int)first.Value;

            for (var n = first.Next; n != null; n = n.Next)
            {
                int cur = (int)n.Value;
                if (cur < prev) return false;

                prev = cur;
            }

            return true;
        }

        // =========================================================
        // Q4.9 – BinarySearchIterative()
        // =========================================================
        private int BinarySearchIterative(LinkedList<double> list, int target, int min, int max)
        {
            while (min <= max - 1)
            {
                int mid = (min + max) / 2;


                int group = (int)list.ElementAt(mid);

                if (target == group)
                    return ++mid; // appendix rule

                if (target < group)
                    max = mid - 1;
                else
                    min = mid + 1;
            }


            return min;
        }

        // =========================================================
        // Q4.10 – BinarySearchRecursive()
        // =========================================================
        public int BinarySearchRecursive(LinkedList<double> list, int target, int min, int max)
        {
            if (min <= max - 1)
            {
                int mid = (min + max) / 2;
                int group = (int)list.ElementAt(mid);

                if (target == group)
                    return mid;

                if (target < group)
                    return BinarySearchRecursive(list, target, min, mid - 1);
                else
                    return BinarySearchRecursive(list, target, mid + 1, max);
            }

            return min;
        }
        // =========================================================
        // Highlight()
        // What this does (lecturer requirement):
        // 1) If the target group exists (example target=32),
        //    highlight every value where the integer part is 32.
        // 2) If the target group does NOT exist,
        //    highlight the closestn value(s) to show the nearest result.
        //    If both sides are equally close, highlight both.
        // =========================================================
        private void Highlight(LinkedList<double> list, ListBox lb, int target, int insertionPos)
        {
            // Safety: if ListBox is empty, nothing to highlight
            if (lb.Items.Count == 0) return;

            // Clear any old highlighted items first
            lb.SelectedItems.Clear();

            // =========================================================
            // PART A: FOUND
            // Walk the LinkedList and select all items where:
            // (int)value == target
            // Example: 32.0001, 32.9999, 32.5432 all become group 32
            // =========================================================
            int idx = 0;              // keeps the same index as the ListBox item
            int firstMatch = -1;      // remember first match so we can scroll to it

            // Loop through the LinkedList nodes in order
            for (var n = list.First; n != null; n = n.Next, idx++)
            {
                // Group = integer part only (32.1234 -> 32)
                int group = (int)n.Value;

                // If this node belongs to the searched group
                if (group == target)
                {
                    // Save the first matching index (for scrolling)
                    if (firstMatch == -1)
                        firstMatch = idx;

                    // Highlight this row in the ListBox
                    lb.SelectedItems.Add(lb.Items[idx]);
                }
            }

            if (firstMatch != -1)
            {
                // Scroll the ListBox to the first matching highlighted value
                lb.ScrollIntoView(lb.Items[firstMatch]);

                // Show status message
                txtStatus.Text = $"Status: Found {target}. Highlighted all {target}.";
                return;
            }

            // =========================================================
            // PART B: NOT FOUND
            // The group does not exist in the data.
            // Highlight the closest value(s) using the insertion position
            // that binary search returned.
            // =========================================================
            int count = lb.Items.Count;

            // Clamp insertion position so it never goes outside the list
            if (insertionPos < 0) insertionPos = 0;
            if (insertionPos > count) insertionPos = count;

            // Neighbours around the insertion position:
            // - "right" is where the target would be inserted
            // - "left" is the item just before that position
            int right = insertionPos;
            int left = insertionPos - 1;

            // Clamp right index into valid range 
            if (right < 0) right = 0;
            if (right > count - 1) right = count - 1;

            // Clamp left index into valid range 
            if (left < 0) left = 0;
            if (left > count - 1) left = count - 1;

            // ElementAt is used here to read the value at that index
            int leftGroup = (int)list.ElementAt(left);
            int rightGroup = (int)list.ElementAt(right);

            // Work out how far each side is from the target
            int leftDiff = Math.Abs(target - leftGroup);
            int rightDiff = Math.Abs(target - rightGroup);

            // Decide which side is closer:
            // - If left is smaller or equal distance, pick left
            // - If right is smaller or equal distance, pick right
            // If the distance is equal, BOTH become true (so we highlight both).
            bool pickLeft = leftDiff <= rightDiff;
            bool pickRight = rightDiff <= leftDiff;

            // Highlight left side if it is the closest 
            if (pickLeft)
                lb.SelectedItems.Add(lb.Items[left]);

            // Highlight right side if it is the closest 
            // Avoid double-selecting same index
            if (pickRight && right != left)
                lb.SelectedItems.Add(lb.Items[right]);

            // Scroll to whichever side we prefer to show first
            lb.ScrollIntoView(lb.Items[pickLeft ? left : right]);

            // Status message:
            // If tie and two different indices, show both closest groups
            if (pickLeft && pickRight && left != right && leftDiff == rightDiff)
                txtStatus.Text = $"Status: NOT FOUND. Closest: {leftGroup} and {rightGroup}.";
            else
                txtStatus.Text = $"Status: NOT FOUND. Closest: {(pickLeft ? leftGroup : rightGroup)}.";
        }

        // =========================================================
        // Q4.11 – SORT BUTTONS (Selection + Insertion)
        // Requirement:
        // - Ensure data exists (at least 2 nodes)
        // - Start stopwatch BEFORE sorting
        // - Perform selected sorting algorithm
        // - Stop stopwatch AFTER sorting
        // - Display elapsed time in milliseconds
        // - Refresh ListBox display
        // - Update overall sensor data view
        // - Update status message
        // =========================================================


        // ---------------------------------------------------------
        // SENSOR A – Selection Sort
        // ---------------------------------------------------------
        private void btnSelSortA_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Validate data exists
            // Sorting requires at least 2 values
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Step 2: Start performance timer
            var sw = Stopwatch.StartNew();

            // Step 3: Perform Selection Sort on Sensor A
            SelectionSort(sensorA);

            // Step 4: Stop performance timer
            sw.Stop();

            // Step 5: Display elapsed time in milliseconds
            txtMsSelA.Text = sw.ElapsedMilliseconds + " ms";

            // Step 6: Refresh ListBox display
            DisplayListboxData(sensorA, lbSensorA);

            // Step 7: Refresh combined sensor view
            ShowAllSensorData();

            // Step 8: Update status
            txtStatus.Text = "Status: Sensor A Selection Sort completed";
        }


        // ---------------------------------------------------------
        // SENSOR A – Insertion Sort
        // ---------------------------------------------------------
        private void btnInsSortA_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Validate data exists
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Step 2: Start performance timer
            var sw = Stopwatch.StartNew();

            // Step 3: Perform Insertion Sort on Sensor A
            InsertionSort(sensorA);

            // Step 4: Stop performance timer
            sw.Stop();

            // Step 5: Display elapsed time in milliseconds
            txtMsInsA.Text = sw.ElapsedMilliseconds + " ms";

            // Step 6: Refresh ListBox display
            DisplayListboxData(sensorA, lbSensorA);

            // Step 7: Refresh combined sensor view
            ShowAllSensorData();

            // Step 8: Update status
            txtStatus.Text = "Status: Sensor A Insertion Sort completed";
        }


        // ---------------------------------------------------------
        // SENSOR B – Selection Sort
        // ---------------------------------------------------------
        private void btnSelSortB_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Validate data exists
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            // Step 2: Start performance timer
            var sw = Stopwatch.StartNew();

            // Step 3: Perform Selection Sort on Sensor B
            SelectionSort(sensorB);

            // Step 4: Stop performance timer
            sw.Stop();

            // Step 5: Display elapsed time in milliseconds
            txtMsSelB.Text = sw.ElapsedMilliseconds + " ms";

            // Step 6: Refresh ListBox display
            DisplayListboxData(sensorB, lbSensorB);

            // Step 7: Refresh combined sensor view
            ShowAllSensorData();

            // Step 8: Update status
            txtStatus.Text = "Status: Sensor B Selection Sort completed";
        }


        // ---------------------------------------------------------
        // SENSOR B – Insertion Sort
        // ---------------------------------------------------------
        private void btnInsSortB_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Validate data exists
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            // Step 2: Start performance timer
            var sw = Stopwatch.StartNew();

            // Step 3: Perform Insertion Sort on Sensor B
            InsertionSort(sensorB);

            // Step 4: Stop performance timer
            sw.Stop();

            // Step 5: Display elapsed time in milliseconds
            txtMsInsB.Text = sw.ElapsedMilliseconds + " ms";

            // Step 6: Refresh ListBox display
            DisplayListboxData(sensorB, lbSensorB);

            // Step 7: Refresh combined sensor view
            ShowAllSensorData();

            // Step 8: Update status
            txtStatus.Text = "Status: Sensor B Insertion Sort completed";
        }
        // =========================================================
        // Q4.12 – Search Button Handlers 
        // Decimal input allowed: 30.9 -> group 30
        // =========================================================
        // =========================================================
        // Q4.12 – SEARCH BUTTON HANDLERS
        // Requirement:
        // - Allow decimal input (30.9 → grouped as 30)
        // - Validate numeric input
        // - Ensure data exists
        // - Ensure list is sorted before binary search
        // - Start stopwatch BEFORE search
        // - Stop stopwatch AFTER search
        // - Display elapsed ticks
        // - Highlight found value or insertion position
        // =========================================================


        // ---------------------------------------------------------
        // SENSOR A – Iterative Binary Search
        // ---------------------------------------------------------
        private void btnSearchAIter_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Validate numeric input (decimal allowed)
            if (!double.TryParse(txtSearchA.Text, out double input))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor A";
                return;
            }

            // Step 2: Convert decimal to integer group (30.9 → 30)
            int value = (int)input;

            // Step 3: Ensure data exists
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Step 4: Ensure data is sorted before binary search
            if (!Sorted(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // Step 5: Get maximum boundary for search
            int max = NumberOfNodes(sensorA);

            // Step 6: Start performance timer
            var sw = Stopwatch.StartNew();

            // Step 7: Perform iterative binary search
            int result = BinarySearchIterative(sensorA, value, 0, max);

            // Step 8: Stop performance timer
            sw.Stop();

            // Step 9: Display elapsed ticks
            txtTicksAIter.Text = sw.ElapsedTicks + " ticks";

            // Step 10: Refresh ListBox display
            DisplayListboxData(sensorA, lbSensorA);

            // Step 11: Adjust insertion index if needed
            int insertion = (result >= 1 && result <= lbSensorA.Items.Count) ? result - 1 : result;

            // Step 12: Highlight found value and neighbours
            Highlight(sensorA, lbSensorA, value, insertion);
        }


        // ---------------------------------------------------------
        // SENSOR A – Recursive Binary Search
        // ---------------------------------------------------------
        private void btnSearchARec_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Validate numeric input
            if (!double.TryParse(txtSearchA.Text, out double input))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor A";
                return;
            }

            // Step 2: Convert decimal to integer group
            int value = (int)input;

            // Step 3: Ensure data exists
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Step 4: Ensure data is sorted
            if (!Sorted(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // Step 5: Get maximum boundary
            int max = NumberOfNodes(sensorA);

            // Step 6: Start performance timer
            var sw = Stopwatch.StartNew();

            // Step 7: Perform recursive binary search
            int idxOrIns = BinarySearchRecursive(sensorA, value, 0, max);

            // Step 8: Stop performance timer
            sw.Stop();

            // Step 9: Display elapsed ticks
            txtTicksARec.Text = sw.ElapsedTicks + " ticks";

            // Step 10: Refresh ListBox display
            DisplayListboxData(sensorA, lbSensorA);

            // Step 11: Highlight found value or insertion position
            Highlight(sensorA, lbSensorA, value, idxOrIns);
        }


        // ---------------------------------------------------------
        // SENSOR B – Iterative Binary Search
        // ---------------------------------------------------------
        private void btnSearchBIter_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Validate numeric input
            if (!double.TryParse(txtSearchB.Text, out double input))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor B";
                return;
            }

            // Step 2: Convert decimal to integer group
            int value = (int)input;

            // Step 3: Ensure data exists
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            // Step 4: Ensure data is sorted
            if (!Sorted(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // Step 5: Get maximum boundary
            int max = NumberOfNodes(sensorB);

            // Step 6: Start performance timer
            var sw = Stopwatch.StartNew();

            // Step 7: Perform iterative binary search
            int result = BinarySearchIterative(sensorB, value, 0, max);

            // Step 8: Stop performance timer
            sw.Stop();

            // Step 9: Display elapsed ticks
            txtTicksBIter.Text = sw.ElapsedTicks + " ticks";

            // Step 10: Refresh ListBox display
            DisplayListboxData(sensorB, lbSensorB);

            // Step 11: Adjust insertion index if needed
            int insertion = (result >= 1 && result <= lbSensorB.Items.Count) ? result - 1 : result;

            // Step 12: Highlight found value and neighbours
            Highlight(sensorB, lbSensorB, value, insertion);
        }


        // ---------------------------------------------------------
        // SENSOR B – Recursive Binary Search
        // ---------------------------------------------------------
        private void btnSearchBRec_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: Validate numeric input
            if (!double.TryParse(txtSearchB.Text, out double input))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor B";
                return;
            }

            // Step 2: Convert decimal to integer group
            int value = (int)input;

            // Step 3: Ensure data exists
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            // Step 4: Ensure data is sorted
            if (!Sorted(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // Step 5: Get maximum boundary
            int max = NumberOfNodes(sensorB);

            // Step 6: Start performance timer
            var sw = Stopwatch.StartNew();

            // Step 7: Perform recursive binary search
            int idxOrIns = BinarySearchRecursive(sensorB, value, 0, max);

            // Step 8: Stop performance timer
            sw.Stop();

            // Step 9: Display elapsed ticks
            txtTicksBRec.Text = sw.ElapsedTicks + " ticks";

            // Step 10: Refresh ListBox display
            DisplayListboxData(sensorB, lbSensorB);

            // Step 11: Highlight found value or insertion position
            Highlight(sensorB, lbSensorB, value, idxOrIns);
        }
        // =========================================================
        // Reset UI after Load
        // =========================================================
        private void ResetUiAfterLoad()
        {
            lbSensorA.SelectedItems.Clear();
            lbSensorB.SelectedItems.Clear();

            txtSearchA.Clear();
            txtSearchB.Clear();

            txtTicksAIter.Clear();
            txtTicksARec.Clear();
            txtTicksBIter.Clear();
            txtTicksBRec.Clear();

            txtMsSelA.Clear();
            txtMsInsA.Clear();
            txtMsSelB.Clear();
            txtMsInsB.Clear();
        }


        // =========================================================
        // TEST HELPER – Highlight Logic (Unit Test)
        //
        // Rule:
        // - Group = integer part of value (32.1234 → 32)
        // - If found → return ALL matching indices
        // - If not found → return closest index
        // - If equal distance (tie) → return two indices
        //
        // This method does NOT modify UI elements.
        // =========================================================
        public static (List<int> Indices, string Message) GetHighlightIndices(
            LinkedList<double> list,
            int target,
            int insertion)
        {
            // List to store matching or closest indices
            List<int> indices = new List<int>();


            // =====================================================
            // SECTION A – CHECK IF TARGET EXISTS (FOUND CASE)
            // =====================================================

            // Step 1: Iterate through entire LinkedList
            int idx = 0;
            for (var node = list.First; node != null; node = node.Next, idx++)
            {
                // Step 2: Extract integer group of value
                int group = (int)node.Value;

                // Step 3: If integer part matches target, store index
                if (group == target)
                    indices.Add(idx);
            }

            // Step 4: If any matches found, return all of them
            if (indices.Count > 0)
                return (indices, $"FOUND {target}");



            // =====================================================
            // SECTION B – TARGET NOT FOUND → FIND CLOSEST
            // =====================================================

            int count = list.Count;

            // Step 5: Handle empty list edge case
            if (count == 0)
                return (new List<int>(), "NOT FOUND (empty list)");


            // =====================================================
            // STEP 6 – Clamp insertion position to valid range
            // =====================================================

            if (insertion < 0)
                insertion = 0;

            if (insertion > count)
                insertion = count;


            // Determine neighbouring indices
            int right = insertion;
            int left = insertion - 1;


            // =====================================================
            // STEP 7 – Clamp left and right boundaries
            // =====================================================

            if (right < 0) right = 0;
            if (right > count - 1) right = count - 1;

            if (left < 0) left = 0;
            if (left > count - 1) left = count - 1;


            // =====================================================
            // STEP 8 – Compare distances to target
            // =====================================================
            // =====================================================
            // THIS BLOCK CALCULATES WHICH VALUE IS CLOSER
            //
            // I compare the target value with:
            // - the value on the left side
            // - the value on the right side
            //
            // "Diff" means numeric difference (distance).            
            //Math.Abs() is  Returns the absolute value of a number
            // I use Math.Abs so the result is always positive.
            //
            // Example:
            // target = 30
            // leftGroup = 28 is diff = 2
            // rightGroup = 33 is diff = 3
            //
            // Since 2 < 3, left is closer.
            // =====================================================

            int leftGroup = (int)list.ElementAt(left);
            int rightGroup = (int)list.ElementAt(right);

            int leftDiff = Math.Abs(target - leftGroup);
            int rightDiff = Math.Abs(target - rightGroup);


            // =====================================================
            // STEP 9 – Handle tie case (equal distance)
            // =====================================================

            if (leftDiff == rightDiff && left != right)
                return (
                    new List<int> { left, right },
                    $"NOT FOUND {target} (tie {leftGroup}/{rightGroup})"
                );


            // =====================================================
            // STEP 10 – Return closest single index
            // =====================================================

            if (leftDiff < rightDiff)
                return (
                    new List<int> { left },
                    $"NOT FOUND {target} (closest {leftGroup})"
                );

            return (
                new List<int> { right },
                $"NOT FOUND {target} (closest {rightGroup})"
            );
        }
    }

}

