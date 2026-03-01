using Galileo6;
using System;
using System.Collections.Generic;
using System.Data;
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
    ///
    /// Lecturer requirement (Search highlight):
    /// - If target is 32 -> highlight ALL values 32.xxxx (integer-part group)
    /// - If no 32.xxxx exists -> show Not found and highlight closest group value(s)
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
        // =========================================================
        private void LoadData()
        {
            // required fixed dataset size from assessment
            const int SIZE = 400;

            if (!double.TryParse(txtSigma.Text, out double sigma) ||
                !double.TryParse(txtMu.Text, out double mu))
            {
                MessageBox.Show("Invalid numeric input for Sigma or Mu.");
                return;
            }

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

            sensorA.Clear();
            sensorB.Clear();

            ReadData galileo = new ReadData();

            for (int i = 0; i < SIZE; i++)
            {
                sensorA.AddLast(galileo.SensorA(mu, sigma));
                sensorB.AddLast(galileo.SensorB(mu, sigma));
            }
        }

        // =========================================================
        // Q4.5 – NumberOfNodes()
        // =========================================================
        private int NumberOfNodes(LinkedList<double> list)
        {
            int count = 0;
            for (var node = list.First; node != null; node = node.Next)
                count++;
            return count;
        }

        // =========================================================
        // Q4.6 – DisplayListboxData()
        // =========================================================
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            target.Items.Clear();
            foreach (double value in list)
                target.Items.Add(value.ToString("F4"));
        }

        // =========================================================
        // Q4.3 – ShowAllSensorData()
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
            // stop if list is null
            if (list == null) return false;

            // get total number of nodes
            int max = NumberOfNodes(list);

            // nothing to sort if list has less than 2 values
            if (max < 2) return false;

            // outer loop selects current position
            for (int i = 0; i < max - 1; i++)
            {
                // assume current index is the minimum
                int min = i;

                // search remaining list for smaller value
                for (int j = i + 1; j < max; j++)
                {
                    // update min index if smaller value found
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j;
                }

                // move to node at index i
                LinkedListNode<double>? nodeI = list.First;
                for (int k = 0; k < i && nodeI != null; k++)
                    nodeI = nodeI.Next;

                // move to node at index min
                LinkedListNode<double>? nodeMin = list.First;
                for (int k = 0; k < min && nodeMin != null; k++)
                    nodeMin = nodeMin.Next;

                // safety check
                if (nodeI == null || nodeMin == null) return false;

                // swap values between current node and minimum node
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
            // stop if list is null
            if (list == null) return false;

            // get total number of nodes
            int max = NumberOfNodes(list);

            // no sorting needed if list has less than 2 values
            if (max < 2) return false;

            // outer loop moves through list
            for (int i = 0; i < max - 1; i++)
            {
                // inner loop shifts current value left until correct position
                for (int j = i + 1; j > 0; j--)
                {
                    // compare neighbouring values
                    if (list.ElementAt(j - 1) > list.ElementAt(j))
                    {
                        // move to node at index (j - 1)
                        LinkedListNode<double>? prevNode = list.First;
                        for (int k = 0; k < j - 1 && prevNode != null; k++)
                            prevNode = prevNode.Next;

                        // safety check
                        if (prevNode == null || prevNode.Next == null) return false;

                        // next node is current position (j)
                        LinkedListNode<double> curNode = prevNode.Next;

                        // swap adjacent values
                        double temp = prevNode.Value;
                        prevNode.Value = curNode.Value;
                        curNode.Value = temp;
                    }
                }
            }

            return true;
        }

        // =========================================================
        // Q4.9 – BinarySearchIterative()
        // Lecturer requirement: compare by integer group
        // =========================================================
        private int BinarySearchIterative(LinkedList<double> list, int searchValue, int minimum, int maximum)
        {
            while (minimum <= maximum - 1)
            {
                int middle = (minimum + maximum) / 2;

                // compare by integer part (32.1234 -> 32)
                int middleGroup = (int)list.ElementAt(middle);

                if (searchValue == middleGroup)
                    return ++middle; // appendix behaviour

                else if (searchValue < middleGroup)
                    maximum = middle - 1;
                else
                    minimum = middle + 1;
            }

            // not found, return insertion position
            return minimum;
        }

        // =========================================================
        // Q4.10 – BinarySearchRecursive()
        // Lecturer requirement: compare by integer group
        // =========================================================
        public int BinarySearchRecursive(LinkedList<double> list, int searchValue, int minimum, int maximum)
        {
            if (minimum <= maximum - 1)
            {
                int middle = (minimum + maximum) / 2;

                int middleGroup = (int)list.ElementAt(middle);

                if (searchValue == middleGroup)
                    return middle;

                else if (searchValue < middleGroup)
                    return BinarySearchRecursive(list, searchValue, minimum, middle - 1);
                else
                    return BinarySearchRecursive(list, searchValue, middle + 1, maximum);
            }

            // not found, return insertion position
            return minimum;
        }

        // =========================================================
        // IsSortedByGroup()
        // Checks list is sorted by integer groups before binary search
        // =========================================================
        private bool IsSortedByGroup(LinkedList<double> list)
        {
            var firstNode = list.First;
            if (firstNode == null) return false;

            int prev = (int)firstNode.Value;
            var node = firstNode.Next;

            while (node != null)
            {
                int cur = (int)node.Value;
                if (cur < prev) return false;

                prev = cur;
                node = node.Next;
            }

            return true;
        }

        // =========================================================
        // HighlightGroup()
        // - Highlights ALL target group values (example: 32.xxxx)
        // - If not found, highlights the closest group value(s)
        // =========================================================
        private void HighlightGroup(LinkedList<double> list, ListBox lb, int target, int insertionPosCandidate)
        {
            // stop if ListBox has no items to highlight
            if (lb.Items.Count == 0) return;

            // clear old highlights 
            lb.SelectedItems.Clear();

            // =========================================================
            // PART A: FOUND
            // Highlight all values that belong to the target group.
            // Example: target = 32 -> highlight every 32.xxxx value.
            // =========================================================
            int idx = 0;                 // current item index in ListBox
            int firstMatchIndex = -1;    // used to scroll to first match

            // walk through LinkedList and ListBox at the same time
            for (var node = list.First; node != null; node = node.Next, idx++)
            {
                // group is the integer part (32.1234 -> 32)
                int group = (int)node.Value;

                // if this value is in the target group, select it
                if (group == target)
                {
                    // remember first matched index so we can scroll to it
                    if (firstMatchIndex == -1)
                        firstMatchIndex = idx;

                    // highlight this ListBox item
                    lb.SelectedItems.Add(lb.Items[idx]);
                }
            }

            // if at least one match was found
            if (firstMatchIndex != -1)
            {
                // scroll to the first highlighted value
                lb.ScrollIntoView(lb.Items[firstMatchIndex]);

                // show found message
                txtStatus.Text = $"Status: Found {target}";
                return;
            }

            // =========================================================
            // PART B: NOT FOUND
            // If no target group exists, highlight the closest group.
            // This shows the nearest available values to the user.
            // =========================================================
            int count = lb.Items.Count;

            // keep insertion position inside valid bounds
            if (insertionPosCandidate < 0) insertionPosCandidate = 0;
            if (insertionPosCandidate > count) insertionPosCandidate = count;

            // choose the positions around the insertion point
            int right = insertionPosCandidate;
            int left = insertionPosCandidate - 1;

            // clamp right index
            if (right < 0) right = 0;
            if (right > count - 1) right = count - 1;

            // clamp left index
            if (left < 0) left = 0;
            if (left > count - 1) left = count - 1;

            // get group values at left and right positions
            int leftGroup = (int)list.ElementAt(left);
            int rightGroup = (int)list.ElementAt(right);

            // calculate which side is closer to target
            int leftDiff = Math.Abs(target - leftGroup);
            int rightDiff = Math.Abs(target - rightGroup);

            // pick closest side
            bool pickLeft = leftDiff <= rightDiff;
            bool pickRight = rightDiff <= leftDiff;

            // highlight left item if it is closest 
            if (pickLeft)
                lb.SelectedItems.Add(lb.Items[left]);

            // highlight right item if it is closest  and different index
            if (pickRight && right != left)
                lb.SelectedItems.Add(lb.Items[right]);

            // scroll to the closest highlighted item
            lb.ScrollIntoView(lb.Items[pickLeft ? left : right]);

            // show NOT FOUND message with closest group
            if (pickLeft && pickRight && left != right && leftDiff == rightDiff)
                txtStatus.Text = $"Status: NOT FOUND. Closest groups: {leftGroup} and {rightGroup}.";
            else
                txtStatus.Text = $"Status: NOT FOUND. Closest group: {(pickLeft ? leftGroup : rightGroup)}.";
        }

        // =========================================================
        // Q4.11 – Sort Button Methods
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
        // Q4.12 – Search Button Methods (ticks)
        // =========================================================
        private void btnSearchAIter_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: validate user input (must be an integer)
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            // Step 2: check data exists (binary search needs data)
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Step 3: stop if data is not sorted (binary search will be wrong)
            if (!IsSortedByGroup(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // Step 4: set search range using number of nodes
            int max = NumberOfNodes(sensorA);

            // Step 5: start timing before search
            var sw = Stopwatch.StartNew();

            // Step 6: run iterative binary search
            int result = BinarySearchIterative(sensorA, value, 0, max);

            // Step 7: stop timing after search
            sw.Stop();

            // Step 8: show elapsed ticks in textbox
            txtTicksAIter.Text = sw.ElapsedTicks + " ticks";

            // Step 9: refresh ListBox values
            DisplayListboxData(sensorA, lbSensorA);

            // Step 10: convert search return to a safe index to use for "closest"
            // iterative returns:
            // - ++middle (1-based) when found
            // - insertion position when not found
            int insertionPosCandidate;
            if (result >= 1 && result <= lbSensorA.Items.Count)
                insertionPosCandidate = result - 1; // convert to 0-based index
            else
                insertionPosCandidate = result;

            // Step 11: highlight all matching group values or closest group
            HighlightGroup(sensorA, lbSensorA, value, insertionPosCandidate);
        }

        private void btnSearchARec_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: validate user input (must be an integer)
            if (!int.TryParse(txtSearchA.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor A";
                return;
            }

            // Step 2: check data exists
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Step 3: stop if data is not sorted
            if (!IsSortedByGroup(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A data first.";
                return;
            }

            // Step 4: set search range
            int max = NumberOfNodes(sensorA);

            // Step 5: start timing before recursive search
            var sw = Stopwatch.StartNew();

            // Step 6: run recursive binary search
            int indexOrInsert = BinarySearchRecursive(sensorA, value, 0, max);

            // Step 7: stop timing after search
            sw.Stop();

            // Step 8: show ticks in UI
            txtTicksARec.Text = sw.ElapsedTicks + " ticks";

            // Step 9: refresh ListBox display
            DisplayListboxData(sensorA, lbSensorA);

            // Step 10: highlight all matching group values or closest group
            HighlightGroup(sensorA, lbSensorA, value, indexOrInsert);
        }

        // =========================================================
        // SENSOR B SEARCH METHODS (these were missing)
        // =========================================================
        private void btnSearchBIter_Click(object sender, RoutedEventArgs e)
        {
            // validate integer input
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            // stop if no data loaded
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            // binary search requires sorted data
            if (!IsSortedByGroup(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // total nodes used as search range
            int max = NumberOfNodes(sensorB);

            // start timing
            var sw = Stopwatch.StartNew();

            // run iterative search
            int result = BinarySearchIterative(sensorB, value, 0, max);

            // stop timing
            sw.Stop();

            // display ticks
            txtTicksBIter.Text = sw.ElapsedTicks + " ticks";

            // refresh list
            DisplayListboxData(sensorB, lbSensorB);

            // convert return value to insertion candidate
            int insertionPosCandidate;
            if (result >= 1 && result <= lbSensorB.Items.Count)
                insertionPosCandidate = result - 1;
            else
                insertionPosCandidate = result;

            // highlight results
            HighlightGroup(sensorB, lbSensorB, value, insertionPosCandidate);
        }

        private void btnSearchBRec_Click(object sender, RoutedEventArgs e)
        {
            // validate integer input
            if (!int.TryParse(txtSearchB.Text, out int value))
            {
                txtStatus.Text = "Status: Enter a valid integer for Sensor B";
                return;
            }

            // stop if list has no data
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            // binary search works only on sorted data
            if (!IsSortedByGroup(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B data first.";
                return;
            }

            // get total nodes for search
            int max = NumberOfNodes(sensorB);

            // start timing
            var sw = Stopwatch.StartNew();

            // run recursive search
            int indexOrInsert = BinarySearchRecursive(sensorB, value, 0, max);

            // stop timing
            sw.Stop();

            // display ticks
            txtTicksBRec.Text = sw.ElapsedTicks + " ticks";

            // refresh list
            DisplayListboxData(sensorB, lbSensorB);

            // highlight results
            HighlightGroup(sensorB, lbSensorB, value, indexOrInsert);
        }

        // =========================================================
        // Q4.13 – Input Validation
        // =========================================================
        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void IntegerOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            object? data = e.DataObject.GetData(typeof(string));
            string? text = data as string;

            if (string.IsNullOrEmpty(text) || !text.All(char.IsDigit))
                e.CancelCommand();
        }

        // ---------------------------------------------------------
        // Clears ListBox selection
        // ---------------------------------------------------------
        private void ClearAll(ListBox lb)
        {
            if (lb.SelectionMode == SelectionMode.Single)
            {
                lb.SelectedItem = null;
                lb.SelectedIndex = -1;
            }
            else
            {
                lb.SelectedItems.Clear();
            }
        }

        // ---------------------------------------------------------
        // Resets UI after loading new sensor data
        // ---------------------------------------------------------
        private void ResetUiAfterLoad()
        {
            ClearAll(lbSensorA);
            ClearAll(lbSensorB);

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
    }
}