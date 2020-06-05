using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventoryView
{
    public partial class InventoryViewForm : Form
    {
        public InventoryViewForm()
        {
            InitializeComponent();
        }

        List<TreeNode> searchMatches = new List<TreeNode>();
        TreeNode currentMatch = null;

        private void InventoryViewForm_Load(object sender, EventArgs e)
        {
            BindData();
        }

        private void BindData()
        {
            chkCharacters.Items.Clear();
            tv.Nodes.Clear();
            // Get a distinct character list and load them into the checked list box... which is currently not shown/used.
            var characters = Class1.characterData.Select(tbl => tbl.name).Distinct().ToList();
            characters.Sort();

            // Recursively load all the items into the tree
            foreach (var character in characters)
            {
                chkCharacters.Items.Add(character, true);
                TreeNode charNode = tv.Nodes.Add(character);
                foreach (var source in Class1.characterData.Where(tbl => tbl.name == character))
                {
                    TreeNode sourceNode = charNode.Nodes.Add(source.source);
                    PopulateTree(sourceNode, source.items);
                }
            }
        }

        private void PopulateTree(TreeNode treeNode, List<ItemData> itemList)
        {
            foreach (var item in itemList)
            {
                TreeNode newNode = treeNode.Nodes.Add(item.tap);
                if (item.items.Count() > 0)
                    PopulateTree(newNode, item.items);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            // Recursively search the tree.
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                searchMatches.Clear();
                currentMatch = null;
                tv.CollapseAll();
                SearchTree(tv.Nodes);
                //foreach (TreeNode treeNode in tv.Nodes)
                //{
                //    if (chkCharacters.CheckedItems.Contains(treeNode.Text))
                //        if (SearchTree(treeNode.Nodes))
                //            treeNode.Expand();
                //}
            }
            btnFindNext.Visible = btnFindPrev.Visible = btnReset.Visible = (searchMatches.Count() != 0);
            if (searchMatches.Count() != 0)
            {
                btnFindNext.PerformClick();
            }
        }

        private bool SearchTree(TreeNodeCollection nodes)
        {
            bool retValue = false;
            foreach (TreeNode treeNode in nodes)
            {
                treeNode.BackColor = Color.White;
                if (SearchTree(treeNode.Nodes) == true) // Recursively search child items. Expand the node if a child item is expanded.
                {
                    treeNode.Expand();
                    retValue = true;
                }

                if (treeNode.Text.Contains(txtSearch.Text)) // If the current item is a match, change the color & add it to the matches list.
                {
                    treeNode.Expand();
                    treeNode.BackColor = Color.Yellow;
                    retValue = true;
                    searchMatches.Add(treeNode);
                }
            }
            return retValue;
        }

        private void btnExpand_Click(object sender, EventArgs e)
        {
            tv.ExpandAll();
        }

        private void btnCollapse_Click(object sender, EventArgs e)
        {
            tv.CollapseAll();
        }

        private void btnWiki_Click(object sender, EventArgs e)
        {
            // Sends the tap of the item to drservice.info which has been building a table linking taps to wiki pages from the Plaza scans.
            if (tv.SelectedNode == null)
                MessageBox.Show("Select an item to lookup.");
            else
                System.Diagnostics.Process.Start(string.Format("https://drservice.info/wiki.ashx?tap={0}", tv.SelectedNode.Text.Replace(" (closed)", "")));
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "  ";
            btnSearch.PerformClick();
            txtSearch.Text = string.Empty;
        }

        private void btnFindPrev_Click(object sender, EventArgs e)
        {
            if (currentMatch == null)
                currentMatch = searchMatches.Last();
            else
            {
                currentMatch.BackColor = Color.Yellow;
                int index = searchMatches.IndexOf(currentMatch) - 1;
                if (index == -1) index = searchMatches.Count()-1;
                currentMatch = searchMatches[index];
            }
            currentMatch.EnsureVisible();
            currentMatch.BackColor = Color.LightBlue;
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            if (currentMatch == null)
                currentMatch = searchMatches.First();
            else
            {
                currentMatch.BackColor = Color.Yellow;
                int index = searchMatches.IndexOf(currentMatch) + 1;
                if (index == searchMatches.Count()) index = 0;
                currentMatch = searchMatches[index];
            }
            currentMatch.EnsureVisible();
            currentMatch.BackColor = Color.LightBlue;
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            Class1._host.SendText("/InventoryView scan");
            this.Close();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            Class1.LoadSettings();
            Class1._host.EchoText("Inventory reloaded.");
            this.Close();
        }

        private void copyTapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tv.SelectedNode.Text);
        }

        private void exportBranchToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> branchText = new List<string>();
            branchText.Add(tv.SelectedNode.Text);
            copyBranchText(tv.SelectedNode.Nodes, branchText, 1);
            Clipboard.SetText(string.Join("\r\n", branchText.ToArray()));
        }

        private void copyBranchText(TreeNodeCollection nodes, List<string> branchText, int level)
        {
            foreach (TreeNode node in nodes)
            {
                branchText.Add(new string('\t', level) + node.Text);
                copyBranchText(node.Nodes, branchText, level+1);
            }
        }

        private void tv_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point p = new Point(e.X, e.Y);

                TreeNode node = tv.GetNodeAt(p);
                if (node != null)
                {
                    tv.SelectedNode = node;
                    contextMenuStrip1.Show(tv, p);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "CSV file|*.csv";
            saveFileDialog1.Title = "Save the CSV file";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                using (StreamWriter sw = File.CreateText(saveFileDialog1.FileName))
                {
                    List<ExportData> list = new List<ExportData>();
                    exportBranch(tv.Nodes, list, 1);
                    sw.WriteLine("Character,Tap,Path");
                    foreach (ExportData item in list)
                    {
                        if (item.Path.Count < 2) { } // Skip
                        else if (item.Path.Count == 3 && new string[] { "Vault", "Home" }.Contains(item.Path[1])) { } // Skip
                        else
                        {
                            sw.WriteLine(string.Format("{0},{1},{2}", CleanCSV(item.Character), CleanCSV(item.Tap), CleanCSV(string.Join("\\", item.Path))));
                        }
                    }
                }
                MessageBox.Show("Export Complete.");
            }
        }

        private string CleanCSV(string data)
        {
            if (!data.Contains(","))
                return data;
            else if (!data.Contains("\""))
                return string.Format("\"{0}\"", data);
            else
                return string.Format("\"{0}\"", data.Replace("\"","\"\""));
        }

        private void exportBranch(TreeNodeCollection nodes, List<ExportData> list, int level)
        {
            foreach (TreeNode node in nodes)
            {
                ExportData exportData = new ExportData() { Tap = node.Text };
                TreeNode tmpNode = node;
                while (tmpNode.Parent != null)
                {
                    tmpNode = tmpNode.Parent;
                    if (tmpNode.Parent != null)
                        exportData.Path.Insert(0, tmpNode.Text);
                }
                exportData.Character = tmpNode.Text;
                list.Add(exportData);
                exportBranch(node.Nodes, list, level + 1);
            }
        }

        public class ExportData
        {
            public string Character { get; set; }
            public string Tap { get; set; }
            public List<string> Path { get; set; } = new List<string>();
        }
    }
}
