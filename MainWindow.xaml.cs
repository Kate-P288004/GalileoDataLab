using Galileo6;
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
    /// Student: Kate Odabas – P288004
    /// Units: ICTPRG535 / ICTPRG547
    ///
    /// Purpose:
    /// - Load 400 readings from Galileo6 DLL into LinkedList<double>
    /// - Sort using Selection Sort and Insertion Sort
    /// - Search using Binary Search:
    ///     A) Group mode (integer)    : 32 -> highlights ALL 32.xxxx
    ///     B) Exact mode (decimal)    : 32.5000 -> highlights only 32.5000
    /// - Measure performance:
    ///     Sort  : milliseconds
    ///     Search: ticks
    ///
    /// Highlight (Lecturer request):
    /// - Whole number input (32 / 32.0000): highlight ALL 32.xxxx values (integer part = 32)
    /// - Decimal input (32.1234): highlight exact 4dp matches, else closest value(s)
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
            const int SIZE = 400;

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

            // Walk node-by-node and count manually
            for (var node = list.First; node != null; node = node.Next)
                count++;

            return count;
        }

        // =========================================================
        // Q4.6 – DisplayListboxData()
        // =========================================================
        private void DisplayListboxData(LinkedList<double> list, ListBox target)
        {
            // Step 1: clear old display
            target.Items.Clear();

            // Step 2: add formatted values (4 decimals)
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

            // Show values side-by-side
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

            // outer loop selects current position
            for (int i = 0; i < max - 1; i++)
            {
                int min = i;

                // search remaining list for smaller value
                for (int j = i + 1; j < max; j++)
                {
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j;
                }

                var nodeI = GetNodeAt(list, i);
                var nodeMin = GetNodeAt(list, min);

                if (nodeI == null || nodeMin == null) return false;

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

            for (int i = 0; i < max - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    if (list.ElementAt(j - 1) > list.ElementAt(j))
                    {
                        var prev = GetNodeAt(list, j - 1);
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

        // Helper: Get node at index
        private LinkedListNode<double>? GetNodeAt(LinkedList<double> list, int index)
        {
            int i = 0;
            var node = list.First;

            while (node != null)
            {
                if (i == index) return node;
                node = node.Next;
                i++;
            }

            return null;
        }

        // =========================================================
        // Q4.9 – BinarySearchIterative()
        // Compare by integer part
        // Returns 1-based index when found, else insertion position
        // =========================================================
        private int BinarySearchIterative(LinkedList<double> list, int searchNumber, int minimum, int maximum)
        {
            var first = list.First;
            if (first == null) return false;

            if (exactDecimal)
            {
                int middle = (minimum + maximum) / 2;
                int middleNumber = (int)Math.Floor(list.ElementAt(middle));

                if (searchNumber == middleNumber)
                    return middle + 1;

                if (searchNumber < middleNumber)
                    maximum = middle - 1;
                else
                    minimum = middle + 1;
            }

            return minimum;
        }

        // =========================================================
        // Q4.10 – BinarySearchRecursive()
        // Compare by integer part
        // Returns 0-based index when found, else insertion position
        // =========================================================
        public int BinarySearchRecursive(LinkedList<double> list, int searchNumber, int minimum, int maximum)
        {
            while (min <= max - 1)
            {
                int middle = (minimum + maximum) / 2;
                int middleNumber = (int)Math.Floor(list.ElementAt(middle));

                if (searchNumber == middleNumber)
                    return middle;

                if (searchNumber < middleNumber)
                    return BinarySearchRecursive(list, searchNumber, minimum, middle - 1);

                return BinarySearchRecursive(list, searchNumber, middle + 1, maximum);
            }

            return minimum;
        }

        // =========================================================
        // Sorted check
        // =========================================================
        private bool Sorted(LinkedList<double> list)
        {
            var node = list.First;
            if (node == null) return false;

            double prev = node.Value;
            node = node.Next;

            while (node != null)
            {
                if (node.Value < prev) return false;
                prev = node.Value;
                node = node.Next;
            }
            return true;
        }

        // =========================================================
        // Parse search value:
        // - rounded4: rounded to 4 decimals
        // - numberPart: integer part (32.1234 -> 32)
        // - whole: true if input is effectively integer
        // =========================================================
        private void HighlightGroup(LinkedList<double> list, ListBox lb, int target, int insertionPosCandidate)
        {
            rounded4 = 0;
            numberPart = 0;
            whole = true;

            if (!double.TryParse(txt.Text, out double raw))
                return false;

            rounded4 = Math.Round(raw, 4);
            numberPart = (int)Math.Floor(rounded4);

            whole = Math.Abs(rounded4 - Math.Round(rounded4, 0)) < 0.00005;
            return true;
        }

        // =========================================================
        // Highlight all values where integer part equals targetNumber
        // =========================================================
        private bool HighlightIntegerPart(LinkedList<double> list, ListBox lb, int targetNumber)
        {
            lb.SelectedItems.Clear();

            int idx = 0;
            int first = -1;

            for (var node = list.First; node != null; node = node.Next, idx++)
            {
                int integerPart = (int)Math.Floor(node.Value);
                if (integerPart == targetNumber)
                {
                    if (first == -1) first = idx;
                    lb.SelectedItems.Add(lb.Items[idx]);
                }
            }

            // if at least one match was found
            if (firstMatchIndex != -1)
            {
                lb.ScrollIntoView(lb.Items[first]);
                txtStatus.Text = $"Status: Found {targetNumber}";
                return true;
            }

            return false;
        }

        // =========================================================
        // Highlight exact 4dp (or closest if not found)
        // =========================================================
        private void HighlightExactOrClosest(LinkedList<double> list, ListBox lb, double targetRounded4)
        {
            lb.SelectedItems.Clear();

            int idx = 0;
            int first = -1;
            bool foundAny = false;

            for (var node = list.First; node != null; node = node.Next, idx++)
            {
                double v = Math.Round(node.Value, 4);
                if (Math.Abs(v - targetRounded4) < 0.00005)
                {
                    foundAny = true;
                    if (first == -1) first = idx;
                    lb.SelectedItems.Add(lb.Items[idx]);
                }
            }

            if (foundAny)
            {
                lb.ScrollIntoView(lb.Items[first]);
                txtStatus.Text = $"Status: Found {targetRounded4:F4}";
                return;
            }

            int best1 = -1, best2 = -1;
            double bestDiff = double.MaxValue;

            idx = 0;
            for (var node = list.First; node != null; node = node.Next, idx++)
            {
                double v = Math.Round(node.Value, 4);
                double diff = Math.Abs(v - targetRounded4);

                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    best1 = idx;
                    best2 = -1;
                }
                else if (Math.Abs(diff - bestDiff) < 0.00005)
                {
                    best2 = idx;
                }
            }

            if (best1 != -1)
            {
                lb.SelectedItems.Add(lb.Items[best1]);
                if (best2 != -1 && best2 != best1)
                    lb.SelectedItems.Add(lb.Items[best2]);

                lb.ScrollIntoView(lb.Items[best1]);
                txtStatus.Text = $"Status: NOT FOUND. Closest value(s) highlighted for {targetRounded4:F4}.";
            }
            else
            {
                txtStatus.Text = "Status: NOT FOUND.";
            }
        }

        // =========================================================
        // Highlight closest integer part around insertion position
        // =========================================================
        private void HighlightClosestInteger(LinkedList<double> list, ListBox lb, int targetNumber, int insertion)
        {
            if (lb.Items.Count == 0) return;

            if (insertion < 0) insertion = 0;
            if (insertion >= lb.Items.Count) insertion = lb.Items.Count - 1;

            int left = insertion - 1;
            if (left < 0) left = 0;

            int leftValue = (int)list.ElementAt(left);
            int rightValue = (int)list.ElementAt(insertion);

            int leftDiff = Math.Abs(targetNumber - leftValue);
            int rightDiff = Math.Abs(targetNumber - rightValue);

            int pickIndex = (leftDiff <= rightDiff) ? left : insertion;

            lb.SelectedItems.Clear();
            lb.SelectedItems.Add(lb.Items[pickIndex]);
            lb.ScrollIntoView(lb.Items[pickIndex]);

            txtStatus.Text = "Status: NOT FOUND. Showing closest value.";
        }

        // =========================================================
        // SEARCH BUTTONS
        // =========================================================

        private void btnSearchAIter_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseSearch(txtSearchA, out double target4, out int numberPart, out bool whole))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor A";
                return;
            }

            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Step 3: stop if data is not sorted (binary search will be wrong)
            if (!IsSortedByGroup(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A first.";
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            int result = BinarySearchIterative(sensorA, numberPart, 0, NumberOfNodes(sensorA));
            sw.Stop();

            txtTicksAIter.Text = sw.ElapsedTicks + " ticks";
            DisplayListboxData(sensorA, lbSensorA);

            if (whole)
            {
                bool found = HighlightIntegerPart(sensorA, lbSensorA, numberPart);
                if (!found)
                {
                    int insertion = (result >= 1 && result <= lbSensorA.Items.Count) ? result - 1 : result;
                    HighlightClosestInteger(sensorA, lbSensorA, numberPart, insertion);
                }
            }
            else
            {
                HighlightExactOrClosest(sensorA, lbSensorA, target4);
            }
        }

        private void btnSearchARec_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseSearch(txtSearchA, out double target4, out int numberPart, out bool whole))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor A";
                return;
            }

            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            if (!Sorted(sensorA))
            {
                txtStatus.Text = "Status: Please sort Sensor A first.";
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            int indexOrInsert = BinarySearchRecursive(sensorA, numberPart, 0, NumberOfNodes(sensorA));
            sw.Stop();

            txtTicksARec.Text = sw.ElapsedTicks + " ticks";
            DisplayListboxData(sensorA, lbSensorA);

            if (whole)
            {
                bool found = HighlightIntegerPart(sensorA, lbSensorA, numberPart);
                if (!found)
                    HighlightClosestInteger(sensorA, lbSensorA, numberPart, indexOrInsert);
            }
            else
            {
                HighlightExactOrClosest(sensorA, lbSensorA, target4);
            }
        }

        private void btnSearchBIter_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseSearch(txtSearchB, out double target4, out int numberPart, out bool whole))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor B";
                return;
            }

            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            if (!Sorted(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B first.";
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            int result = BinarySearchIterative(sensorB, numberPart, 0, NumberOfNodes(sensorB));
            sw.Stop();

            txtTicksBIter.Text = sw.ElapsedTicks + " ticks";
            DisplayListboxData(sensorB, lbSensorB);

            if (whole)
            {
                bool found = HighlightIntegerPart(sensorB, lbSensorB, numberPart);
                if (!found)
                {
                    int insertion = (result >= 1 && result <= lbSensorB.Items.Count) ? result - 1 : result;
                    HighlightClosestInteger(sensorB, lbSensorB, numberPart, insertion);
                }
            }
            else
            {
                HighlightExactOrClosest(sensorB, lbSensorB, target4);
            }
        }

        private void btnSearchBRec_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseSearch(txtSearchB, out double target4, out int numberPart, out bool whole))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor B";
                return;
            }

            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            if (!Sorted(sensorB))
            {
                txtStatus.Text = "Status: Please sort Sensor B first.";
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            int indexOrInsert = BinarySearchRecursive(sensorB, numberPart, 0, NumberOfNodes(sensorB));
            sw.Stop();

            txtTicksBRec.Text = sw.ElapsedTicks + " ticks";
            DisplayListboxData(sensorB, lbSensorB);

            if (whole)
            {
                bool found = HighlightIntegerPart(sensorB, lbSensorB, numberPart);
                if (!found)
                    HighlightClosestInteger(sensorB, lbSensorB, numberPart, indexOrInsert);
            }
            else
            {
                HighlightExactOrClosest(sensorB, lbSensorB, target4);
            }
        }

        // =========================================================
        // SORT BUTTONS
        // =========================================================
        private void btnSelSortA_Click(object sender, RoutedEventArgs e)
        {
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            SelectionSort(sensorA);
            sw.Stop();

            txtMsSelA.Text = sw.ElapsedMilliseconds + " ms";

            DisplayListboxData(sensorA, lbSensorA);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor A Selection Sort completed";
        }

        private void btnInsSortA_Click(object sender, RoutedEventArgs e)
        {
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            InsertionSort(sensorA);
            sw.Stop();

            txtMsInsA.Text = sw.ElapsedMilliseconds + " ms";

            DisplayListboxData(sensorA, lbSensorA);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor A Insertion Sort completed";
        }

        private void btnSelSortB_Click(object sender, RoutedEventArgs e)
        {
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            SelectionSort(sensorB);
            sw.Stop();

            txtMsSelB.Text = sw.ElapsedMilliseconds + " ms";

            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor B Selection Sort completed";
        }

        private void btnInsSortB_Click(object sender, RoutedEventArgs e)
        {
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            Stopwatch sw = Stopwatch.StartNew();
            InsertionSort(sensorB);
            sw.Stop();

            txtMsInsB.Text = sw.ElapsedMilliseconds + " ms";

            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor B Insertion Sort completed";
        }

        // =========================================================
        // Decimal input validation: digits + one dot + max 4 decimals
        // =========================================================
        private void Decimal4_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox tb)
            {
                e.Handled = true;
                return;
            }

            char ch = e.Text.Length > 0 ? e.Text[0] : '\0';

            if (char.IsDigit(ch))
            {
                if (tb.Text.Contains('.'))
                {
                    int dotIndex = tb.Text.IndexOf('.');
                    int decimals = tb.Text.Length - dotIndex - 1;

                    if (tb.SelectionStart > dotIndex && decimals >= 4 && tb.SelectionLength == 0)
                    {
                        e.Handled = true;
                        return;
                    }
                }

                e.Handled = false;
                return;
            }

            if (ch == '.')
            {
                e.Handled = tb.Text.Contains('.');
                return;
            }

            e.Handled = true;
        }

        private void Decimal4_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            TextBox? tb = sender as TextBox;

            if (tb == null)
            {
                e.CancelCommand();
                return;
            }

            string? paste = e.DataObject.GetData(typeof(string)) as string;

            if (paste == null)
            {
                e.CancelCommand();
                return;
            }

            string newText = tb.Text.Insert(tb.SelectionStart, paste);

            int dotCount = 0;

            foreach (char c in newText)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.CancelCommand();
                    return;
                }

                if (c == '.')
                    dotCount++;
            }

            if (dotCount > 1)
            {
                e.CancelCommand();
                return;
            }

            int dotIndex = newText.IndexOf('.');

            if (dotIndex >= 0)
            {
                int decimals = newText.Length - dotIndex - 1;

                if (decimals > 4)
                    e.CancelCommand();
            }
        }

        private void Search_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb) return;

            if (double.TryParse(tb.Text, out double v))
                tb.Text = Math.Round(v, 4).ToString("F4");
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
        // TEST HELPER – Highlight Logic 
        //
        // Rule (matches lecturer requirement):
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
            List<int> indices = new List<int>();

            // ---------- A) FOUND: return all indices where group == target ----------
            int idx = 0;
            for (var node = list.First; node != null; node = node.Next, idx++)
            {
                int group = (int)node.Value;
                if (group == target)
                    indices.Add(idx);
            }

            if (indices.Count > 0)
                return (indices, $"FOUND {target}");

            // ---------- B) NOT FOUND: return closest group index(es) ----------
            int count = list.Count;
            if (count == 0)
                return (new List<int>(), "NOT FOUND (empty list)");

            // Clamp insertion position
            if (insertion < 0) insertion = 0;
            if (insertion > count) insertion = count;

            int right = insertion;
            int left = insertion - 1;

            // Clamp both sides
            if (right < 0) right = 0;
            if (right > count - 1) right = count - 1;

            if (left < 0) left = 0;
            if (left > count - 1) left = count - 1;

            int leftGroup = (int)list.ElementAt(left);
            int rightGroup = (int)list.ElementAt(right);

            int leftDiff = Math.Abs(target - leftGroup);
            int rightDiff = Math.Abs(target - rightGroup);

            // Tie -> return both
            if (leftDiff == rightDiff && left != right)
                return (new List<int> { left, right }, $"NOT FOUND {target} (tie {leftGroup}/{rightGroup})");

            // Closest single
            if (leftDiff < rightDiff)
                return (new List<int> { left }, $"NOT FOUND {target} (closest {leftGroup})");

            return (new List<int> { right }, $"NOT FOUND {target} (closest {rightGroup})");
        }
    }
}