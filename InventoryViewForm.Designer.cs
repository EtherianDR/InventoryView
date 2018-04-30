namespace InventoryView
{
    partial class InventoryViewForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tv = new System.Windows.Forms.TreeView();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.chkCharacters = new System.Windows.Forms.CheckedListBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnExpand = new System.Windows.Forms.Button();
            this.btnCollapse = new System.Windows.Forms.Button();
            this.btnWiki = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.btnFindPrev = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyTapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportBranchToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tv
            // 
            this.tv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tv.Location = new System.Drawing.Point(12, 55);
            this.tv.Name = "tv";
            this.tv.Size = new System.Drawing.Size(1033, 404);
            this.tv.TabIndex = 10;
            this.tv.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tv_MouseUp);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(62, 18);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(365, 20);
            this.txtSearch.TabIndex = 1;
            // 
            // chkCharacters
            // 
            this.chkCharacters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCharacters.FormattingEnabled = true;
            this.chkCharacters.Location = new System.Drawing.Point(909, 41);
            this.chkCharacters.Name = "chkCharacters";
            this.chkCharacters.Size = new System.Drawing.Size(136, 19);
            this.chkCharacters.TabIndex = 9;
            this.chkCharacters.Visible = false;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(12, 21);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(44, 13);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search:";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(433, 16);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnExpand
            // 
            this.btnExpand.Location = new System.Drawing.Point(676, 5);
            this.btnExpand.Name = "btnExpand";
            this.btnExpand.Size = new System.Drawing.Size(75, 23);
            this.btnExpand.TabIndex = 6;
            this.btnExpand.Text = "Expand All";
            this.btnExpand.UseVisualStyleBackColor = true;
            this.btnExpand.Click += new System.EventHandler(this.btnExpand_Click);
            // 
            // btnCollapse
            // 
            this.btnCollapse.Location = new System.Drawing.Point(676, 27);
            this.btnCollapse.Name = "btnCollapse";
            this.btnCollapse.Size = new System.Drawing.Size(75, 23);
            this.btnCollapse.TabIndex = 7;
            this.btnCollapse.Text = "Collapse All";
            this.btnCollapse.UseVisualStyleBackColor = true;
            this.btnCollapse.Click += new System.EventHandler(this.btnCollapse_Click);
            // 
            // btnWiki
            // 
            this.btnWiki.Location = new System.Drawing.Point(757, 16);
            this.btnWiki.Name = "btnWiki";
            this.btnWiki.Size = new System.Drawing.Size(75, 23);
            this.btnWiki.TabIndex = 8;
            this.btnWiki.Text = "Wiki Lookup";
            this.btnWiki.UseVisualStyleBackColor = true;
            this.btnWiki.Click += new System.EventHandler(this.btnWiki_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(595, 16);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 5;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Visible = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnFindNext
            // 
            this.btnFindNext.Location = new System.Drawing.Point(514, 27);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(75, 23);
            this.btnFindNext.TabIndex = 4;
            this.btnFindNext.Text = "Find Next";
            this.btnFindNext.UseVisualStyleBackColor = true;
            this.btnFindNext.Visible = false;
            this.btnFindNext.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // btnFindPrev
            // 
            this.btnFindPrev.Location = new System.Drawing.Point(514, 5);
            this.btnFindPrev.Name = "btnFindPrev";
            this.btnFindPrev.Size = new System.Drawing.Size(75, 23);
            this.btnFindPrev.TabIndex = 3;
            this.btnFindPrev.Text = "Find Prev";
            this.btnFindPrev.UseVisualStyleBackColor = true;
            this.btnFindPrev.Visible = false;
            this.btnFindPrev.Click += new System.EventHandler(this.btnFindPrev_Click);
            // 
            // btnScan
            // 
            this.btnScan.Location = new System.Drawing.Point(838, 16);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(97, 23);
            this.btnScan.TabIndex = 11;
            this.btnScan.Text = "Scan Inventory";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(941, 16);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(97, 23);
            this.btnReload.TabIndex = 12;
            this.btnReload.Text = "Reload File";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyTapToolStripMenuItem,
            this.exportBranchToFileToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(143, 48);
            // 
            // copyTapToolStripMenuItem
            // 
            this.copyTapToolStripMenuItem.Name = "copyTapToolStripMenuItem";
            this.copyTapToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.copyTapToolStripMenuItem.Text = "Copy Text";
            this.copyTapToolStripMenuItem.Click += new System.EventHandler(this.copyTapToolStripMenuItem_Click);
            // 
            // exportBranchToFileToolStripMenuItem
            // 
            this.exportBranchToFileToolStripMenuItem.Name = "exportBranchToFileToolStripMenuItem";
            this.exportBranchToFileToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.exportBranchToFileToolStripMenuItem.Text = "Copy Branch";
            this.exportBranchToFileToolStripMenuItem.Click += new System.EventHandler(this.exportBranchToFileToolStripMenuItem_Click);
            // 
            // InventoryViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1057, 471);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.btnFindNext);
            this.Controls.Add(this.btnFindPrev);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnWiki);
            this.Controls.Add(this.btnCollapse);
            this.Controls.Add(this.btnExpand);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.chkCharacters);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.tv);
            this.Name = "InventoryViewForm";
            this.Text = "Inventory View";
            this.Load += new System.EventHandler(this.InventoryViewForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tv;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.CheckedListBox chkCharacters;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnExpand;
        private System.Windows.Forms.Button btnCollapse;
        private System.Windows.Forms.Button btnWiki;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnFindNext;
        private System.Windows.Forms.Button btnFindPrev;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyTapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportBranchToFileToolStripMenuItem;
    }
}