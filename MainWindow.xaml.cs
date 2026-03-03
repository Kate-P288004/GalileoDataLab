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
    /// Lecturer highlight requirement (GROUP MODE):
    /// - If target=32 highlight ALL values 32.xxxx.
    /// - If no 32.xxxx show NOT FOUND and highlight closest group value(s).
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

            // Move boundary of unsorted part
            for (int i = 0; i < max - 1; i++)
            {
                int min = i;

                // Find smallest in remaining list
                for (int j = i + 1; j < max; j++)
                {
                    if (list.ElementAt(j) < list.ElementAt(min))
                        min = j;
                }

                // Move to node at i
                var nodeI = list.First;
                for (int k = 0; k < i && nodeI != null; k++)
                    nodeI = nodeI.Next;

                // Move to node at min
                var nodeMin = list.First;
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
        // Q4.8 – InsertionSort()
        // =========================================================
        private bool InsertionSort(LinkedList<double> list)
        {
            if (list == null) return false;

            int max = NumberOfNodes(list);
            if (max < 2) return false;

            // Take each element and insert into sorted portion
            for (int i = 0; i < max - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    // If out of order do swap neighbours
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
        // Sorted()
        // Why needed:
        // - Binary Search only works on sorted data.
        // - We support TWO search modes:
        //   A) Group mode  : compare (int)value
        //   B) Exact mode  : compare full double
        //
        // exactDecimal:
        // - true  -> validate sorted by full decimals
        // - false -> validate sorted by integer groups
        // =========================================================
        private bool Sorted(LinkedList<double> list, bool exactDecimal)
        {
            var first = list.First;
            if (first == null) return false;

            if (exactDecimal)
            {
                double prev = first.Value;

                for (var n = first.Next; n != null; n = n.Next)
                {
                    double cur = n.Value;
                    if (cur < prev) return false;

                    prev = cur;
                }

                return true;
            }
            else
            {
                int prevGroup = (int)first.Value;

                for (var n = first.Next; n != null; n = n.Next)
                {
                    int curGroup = (int)n.Value;
                    if (curGroup < prevGroup) return false;

                    prevGroup = curGroup;
                }

                return true;
            }
        }

        // =========================================================
        // Q4.9 – BinarySearchIterative() (GROUP MODE)
        //
        // Notes:
        // - Uses integer group compare: (int)value
        // - Returns:
        //   - if found: returns mid+1 (your appendix rule)
        //   - if not found: returns insertion position (min)
        // =========================================================
        private int BinarySearchIterative(LinkedList<double> list, int target, int min, int max)
        {
            while (min <= max - 1)
            {
                int mid = (min + max) / 2;

                // Group compare only
                int group = (int)list.ElementAt(mid);

                if (target == group)
                    return ++mid; // appendix rule (1-based style)

                if (target < group)
                    max = mid - 1;
                else
                    min = mid + 1;
            }

            return min; // insertion position
        }

        // =========================================================
        // Q4.10 – BinarySearchRecursive() (GROUP MODE)
        // - Uses integer group compare: (int)value
        // - Returns:
        //   - if found: returns mid (0-based)
        //   - if not found: returns insertion position (min)
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

                return BinarySearchRecursive(list, target, mid + 1, max);
            }

            return min; // insertion position
        }

        // =========================================================
        // BinarySearchIterativeExact() (EXACT DECIMAL MODE)
        // - Searches by FULL decimal value (double)
        // - Returns:
        //   - index if found
        //   - insertion position if NOT found
        //
        // EPS is used because doubles can have tiny precision differences.
        // =========================================================
        private int BinarySearchIterativeExact(LinkedList<double> list, double target, int min, int max)
        {
            const double EPS = 0.0000001;

            while (min <= max - 1)
            {
                int mid = (min + max) / 2;
                double val = list.ElementAt(mid);

                if (Math.Abs(val - target) < EPS)
                    return mid;

                if (target < val)
                    max = mid - 1;
                else
                    min = mid + 1;
            }

            return min; // insertion position
        }

        // =========================================================
        // BinarySearchRecursiveExact() (EXACT DECIMAL MODE)
        // - Searches by FULL decimal value (double)
        // - Returns:
        //   - index if found
        //   - insertion position if NOT found
        // =========================================================
        private int BinarySearchRecursiveExact(LinkedList<double> list, double target, int min, int max)
        {
            const double EPS = 0.0000001;

            if (min <= max - 1)
            {
                int mid = (min + max) / 2;
                double val = list.ElementAt(mid);

                if (Math.Abs(val - target) < EPS)
                    return mid;

                if (target < val)
                    return BinarySearchRecursiveExact(list, target, min, mid - 1);

                return BinarySearchRecursiveExact(list, target, mid + 1, max);
            }

            return min; // insertion position
        }

        // =========================================================
        // Highlight() (DUAL MODE)
        //
        // GROUP MODE (lecturer requirement):
        // 1) If target group exists (target=32), highlight ALL values 32.xxxx
        // 2) If group does NOT exist, highlight closest group(s)
        //    If tie, highlight both.
        //
        // EXACT MODE (added feature):
        // 1) If input has decimals (32.5000), highlight only exact value
        // 2) If exact value does NOT exist, highlight closest decimal(s)
        //
        // Parameters:
        // - input        : raw user input number (double)
        // - exactDecimal : true = exact decimal mode, false = group mode
        // - insertionPos : insertion position returned by binary search
        // =========================================================
        private void Highlight(LinkedList<double> list, ListBox lb, double input, bool exactDecimal, int insertionPos)
        {
            // Safety: if ListBox is empty, nothing to highlight
            if (lb.Items.Count == 0) return;

            // Clear any old highlighted items first
            lb.SelectedItems.Clear();

            const double EPS = 0.0000001;

            // =========================================================
            // PART A: FOUND
            // =========================================================
            int idx = 0;
            int firstMatch = -1;

            for (var n = list.First; n != null; n = n.Next, idx++)
            {
                if (exactDecimal)
                {
                    // Exact decimal match (tolerance for doubles)
                    if (Math.Abs(n.Value - input) < EPS)
                    {
                        if (firstMatch == -1)
                            firstMatch = idx;

                        lb.SelectedItems.Add(lb.Items[idx]);
                    }
                }
                else
                {
                    // Group match: integer part only (32.999 -> 32)
                    int group = (int)n.Value;
                    int targetGroup = (int)input;

                    if (group == targetGroup)
                    {
                        if (firstMatch == -1)
                            firstMatch = idx;

                        lb.SelectedItems.Add(lb.Items[idx]);
                    }
                }
            }

            // If at least one match was found, scroll + status and stop
            if (firstMatch != -1)
            {
                lb.ScrollIntoView(lb.Items[firstMatch]);

                if (exactDecimal)
                    txtStatus.Text = $"Status: Found {input:F4}. Highlighted exact value.";
                else
                    txtStatus.Text = $"Status: Found {(int)input}. Highlighted all {(int)input}.";

                return;
            }

            // =========================================================
            // PART B: NOT FOUND -> highlight closest value(s)
            // =========================================================
            int count = lb.Items.Count;

            // Clamp insertion position so it never goes outside the list
            if (insertionPos < 0) insertionPos = 0;
            if (insertionPos > count) insertionPos = count;

            // Neighbours around the insertion position
            int right = insertionPos;
            int left = insertionPos - 1;

            // Clamp right
            if (right < 0) right = 0;
            if (right > count - 1) right = count - 1;

            // Clamp left
            if (left < 0) left = 0;
            if (left > count - 1) left = count - 1;

            // ---------------------------------------------------------
            // NOT FOUND: Exact decimal mode
            // ---------------------------------------------------------
            if (exactDecimal)
            {
                double leftVal = list.ElementAt(left);
                double rightVal = list.ElementAt(right);

                double leftDiff = Math.Abs(input - leftVal);
                double rightDiff = Math.Abs(input - rightVal);

                // Tie rule: if equal distance, highlight both
                bool pickLeft = leftDiff <= rightDiff;
                bool pickRight = rightDiff <= leftDiff;

                if (pickLeft)
                    lb.SelectedItems.Add(lb.Items[left]);

                if (pickRight && right != left)
                    lb.SelectedItems.Add(lb.Items[right]);

                lb.ScrollIntoView(lb.Items[pickLeft ? left : right]);

                if (pickLeft && pickRight && left != right && Math.Abs(leftDiff - rightDiff) < EPS)
                    txtStatus.Text = $"Status: NOT FOUND {input:F4}. Closest: {leftVal:F4} and {rightVal:F4}.";
                else
                    txtStatus.Text = $"Status: NOT FOUND {input:F4}. Closest: {(pickLeft ? leftVal : rightVal):F4}.";

                return;
            }

            // ---------------------------------------------------------
            // NOT FOUND: Group mode (lecturer requirement)
            // ---------------------------------------------------------
            int targetGroup2 = (int)input;

            int leftGroup = (int)list.ElementAt(left);
            int rightGroup = (int)list.ElementAt(right);

            int leftDiff2 = Math.Abs(targetGroup2 - leftGroup);
            int rightDiff2 = Math.Abs(targetGroup2 - rightGroup);

            bool pickLeft2 = leftDiff2 <= rightDiff2;
            bool pickRight2 = rightDiff2 <= leftDiff2;

            if (pickLeft2)
                lb.SelectedItems.Add(lb.Items[left]);

            if (pickRight2 && right != left)
                lb.SelectedItems.Add(lb.Items[right]);

            lb.ScrollIntoView(lb.Items[pickLeft2 ? left : right]);

            if (pickLeft2 && pickRight2 && left != right && leftDiff2 == rightDiff2)
                txtStatus.Text = $"Status: NOT FOUND {targetGroup2}. Closest: {leftGroup} and {rightGroup}.";
            else
                txtStatus.Text = $"Status: NOT FOUND {targetGroup2}. Closest: {(pickLeft2 ? leftGroup : rightGroup)}.";
        }

        // =========================================================
        // Q4.11 – SORT BUTTONS (Selection + Insertion)
        // =========================================================

        private void btnSelSortA_Click(object sender, RoutedEventArgs e)
        {
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

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
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

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
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

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
            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            var sw = Stopwatch.StartNew();
            InsertionSort(sensorB);
            sw.Stop();

            txtMsInsB.Text = sw.ElapsedMilliseconds + " ms";

            DisplayListboxData(sensorB, lbSensorB);
            ShowAllSensorData();

            txtStatus.Text = "Status: Sensor B Insertion Sort completed";
        }

        // =========================================================
        // Q4.12 – SEARCH BUTTON HANDLERS (DUAL MODE)
        //
        // Auto mode rule:
        // - If user types an integer (32)     -> GROUP MODE
        // - If user types a decimal (32.5)   -> EXACT MODE
        // =========================================================

        private void btnSearchAIter_Click(object sender, RoutedEventArgs e)
        {
            // Step 1: validate input
            if (!double.TryParse(txtSearchA.Text, out double input))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor A";
                return;
            }

            // Step 2: decide search mode
            bool exactDecimal = input % 1 != 0;

            // Step 3: ensure data exists
            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            // Step 4: ensure list sorted for this mode
            if (!Sorted(sensorA, exactDecimal))
            {
                txtStatus.Text = exactDecimal
                    ? "Status: Please sort Sensor A by decimal values first."
                    : "Status: Please sort Sensor A by group values first.";
                return;
            }

            int max = NumberOfNodes(sensorA);

            // Step 5: time the search
            var sw = Stopwatch.StartNew();

            int result;
            if (exactDecimal)
            {
                // Exact decimal search
                result = BinarySearchIterativeExact(sensorA, input, 0, max);
            }
            else
            {
                // Group search (integer target)
                int groupTarget = (int)input;
                result = BinarySearchIterative(sensorA, groupTarget, 0, max);
            }

            sw.Stop();
            txtTicksAIter.Text = sw.ElapsedTicks + " ticks";

            // Step 6: refresh listbox
            DisplayListboxData(sensorA, lbSensorA);

            // Step 7: adjust appendix rule only for GROUP iterative search
            int insertionPos = result;

            if (!exactDecimal)
                insertionPos = (result >= 1 && result <= lbSensorA.Items.Count) ? result - 1 : result;

            // Step 8: highlight
            Highlight(sensorA, lbSensorA, input, exactDecimal, insertionPos);
        }

        private void btnSearchARec_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtSearchA.Text, out double input))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor A";
                return;
            }

            bool exactDecimal = input % 1 != 0;

            if (NumberOfNodes(sensorA) < 2)
            {
                txtStatus.Text = "Status: Sensor A has no data loaded.";
                return;
            }

            if (!Sorted(sensorA, exactDecimal))
            {
                txtStatus.Text = exactDecimal
                    ? "Status: Please sort Sensor A by decimal values first."
                    : "Status: Please sort Sensor A by group values first.";
                return;
            }

            int max = NumberOfNodes(sensorA);

            var sw = Stopwatch.StartNew();

            int result;
            if (exactDecimal)
            {
                result = BinarySearchRecursiveExact(sensorA, input, 0, max);
            }
            else
            {
                int groupTarget = (int)input;
                result = BinarySearchRecursive(sensorA, groupTarget, 0, max);
            }

            sw.Stop();
            txtTicksARec.Text = sw.ElapsedTicks + " ticks";

            DisplayListboxData(sensorA, lbSensorA);

            Highlight(sensorA, lbSensorA, input, exactDecimal, result);
        }

        private void btnSearchBIter_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtSearchB.Text, out double input))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor B";
                return;
            }

            bool exactDecimal = input % 1 != 0;

            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            if (!Sorted(sensorB, exactDecimal))
            {
                txtStatus.Text = exactDecimal
                    ? "Status: Please sort Sensor B by decimal values first."
                    : "Status: Please sort Sensor B by group values first.";
                return;
            }

            int max = NumberOfNodes(sensorB);

            var sw = Stopwatch.StartNew();

            int result;
            if (exactDecimal)
            {
                result = BinarySearchIterativeExact(sensorB, input, 0, max);
            }
            else
            {
                int groupTarget = (int)input;
                result = BinarySearchIterative(sensorB, groupTarget, 0, max);
            }

            sw.Stop();
            txtTicksBIter.Text = sw.ElapsedTicks + " ticks";

            DisplayListboxData(sensorB, lbSensorB);

            int insertionPos = result;

            if (!exactDecimal)
                insertionPos = (result >= 1 && result <= lbSensorB.Items.Count) ? result - 1 : result;

            Highlight(sensorB, lbSensorB, input, exactDecimal, insertionPos);
        }

        private void btnSearchBRec_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtSearchB.Text, out double input))
            {
                txtStatus.Text = "Status: Enter a valid number for Sensor B";
                return;
            }

            bool exactDecimal = input % 1 != 0;

            if (NumberOfNodes(sensorB) < 2)
            {
                txtStatus.Text = "Status: Sensor B has no data loaded.";
                return;
            }

            if (!Sorted(sensorB, exactDecimal))
            {
                txtStatus.Text = exactDecimal
                    ? "Status: Please sort Sensor B by decimal values first."
                    : "Status: Please sort Sensor B by group values first.";
                return;
            }

            int max = NumberOfNodes(sensorB);

            var sw = Stopwatch.StartNew();

            int result;
            if (exactDecimal)
            {
                result = BinarySearchRecursiveExact(sensorB, input, 0, max);
            }
            else
            {
                int groupTarget = (int)input;
                result = BinarySearchRecursive(sensorB, groupTarget, 0, max);
            }

            sw.Stop();
            txtTicksBRec.Text = sw.ElapsedTicks + " ticks";

            DisplayListboxData(sensorB, lbSensorB);

            Highlight(sensorB, lbSensorB, input, exactDecimal, result);
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