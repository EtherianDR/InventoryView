using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            var characters = Class1.characterData.Select(tbl => tbl.name).Distinct().ToList();
            characters.Sort();
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
                if (SearchTree(treeNode.Nodes) == true)
                {
                    treeNode.Expand();
                    retValue = true;
                }

                if (treeNode.Text.Contains(txtSearch.Text))
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
                if (index == -1) index = searchMatches.Count();
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
    }
}
