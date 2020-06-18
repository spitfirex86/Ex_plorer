namespace ex_plorer
{
    partial class ExplorerForm
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
            this.folderView = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // folderView
            // 
            this.folderView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.folderView.HideSelection = false;
            this.folderView.Location = new System.Drawing.Point(0, 0);
            this.folderView.Name = "folderView";
            this.folderView.Size = new System.Drawing.Size(484, 361);
            this.folderView.TabIndex = 2;
            this.folderView.UseCompatibleStateImageBehavior = false;
            this.folderView.ItemActivate += new System.EventHandler(this.folderView_ItemActivate);
            // 
            // ExplorerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.folderView);
            this.DoubleBuffered = true;
            this.Name = "ExplorerForm";
            this.Text = "Explorer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExplorerForm_FormClosed);
            this.Load += new System.EventHandler(this.ExplorerForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListView folderView;
    }
}

