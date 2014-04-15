namespace Unm.DistributedSystem.ButterflyNetworkApp
{
    partial class FrmMain
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
			this.graphViewer = new Unm.UserControls.GraphViewer.GraphViewer();
			this.zgGraph1 = new ZedGraph.ZedGraphControl();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.zgGraph3 = new ZedGraph.ZedGraphControl();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.zgGraph2 = new ZedGraph.ZedGraphControl();
			this.SuspendLayout();
			// 
			// graphViewer
			// 
			this.graphViewer.BackColor = System.Drawing.Color.White;
			this.graphViewer.Dock = System.Windows.Forms.DockStyle.Left;
			this.graphViewer.EdgeConnectionStyle = Unm.UserControls.GraphViewer.EdgeConnectionStyle.TopBottom;
			this.graphViewer.Graph = null;
			this.graphViewer.Location = new System.Drawing.Point(0, 0);
			this.graphViewer.Name = "graphViewer";
			this.graphViewer.Size = new System.Drawing.Size(394, 430);
			this.graphViewer.TabIndex = 0;
			// 
			// zgGraph1
			// 
			this.zgGraph1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.zgGraph1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zgGraph1.Location = new System.Drawing.Point(394, 0);
			this.zgGraph1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.zgGraph1.Name = "zgGraph1";
			this.zgGraph1.ScrollGrace = 0D;
			this.zgGraph1.ScrollMaxX = 0D;
			this.zgGraph1.ScrollMaxY = 0D;
			this.zgGraph1.ScrollMaxY2 = 0D;
			this.zgGraph1.ScrollMinX = 0D;
			this.zgGraph1.ScrollMinY = 0D;
			this.zgGraph1.ScrollMinY2 = 0D;
			this.zgGraph1.Size = new System.Drawing.Size(517, 217);
			this.zgGraph1.TabIndex = 1;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(394, 217);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 213);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter2.Location = new System.Drawing.Point(397, 217);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(514, 3);
			this.splitter2.TabIndex = 3;
			this.splitter2.TabStop = false;
			// 
			// zgGraph3
			// 
			this.zgGraph3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.zgGraph3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zgGraph3.Location = new System.Drawing.Point(397, 245);
			this.zgGraph3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.zgGraph3.Name = "zgGraph3";
			this.zgGraph3.ScrollGrace = 0D;
			this.zgGraph3.ScrollMaxX = 0D;
			this.zgGraph3.ScrollMaxY = 0D;
			this.zgGraph3.ScrollMaxY2 = 0D;
			this.zgGraph3.ScrollMinX = 0D;
			this.zgGraph3.ScrollMinY = 0D;
			this.zgGraph3.ScrollMinY2 = 0D;
			this.zgGraph3.Size = new System.Drawing.Size(514, 185);
			this.zgGraph3.TabIndex = 4;
			// 
			// splitter3
			// 
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter3.Location = new System.Drawing.Point(397, 242);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(514, 3);
			this.splitter3.TabIndex = 5;
			this.splitter3.TabStop = false;
			// 
			// zgGraph2
			// 
			this.zgGraph2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.zgGraph2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zgGraph2.Location = new System.Drawing.Point(397, 220);
			this.zgGraph2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.zgGraph2.Name = "zgGraph2";
			this.zgGraph2.ScrollGrace = 0D;
			this.zgGraph2.ScrollMaxX = 0D;
			this.zgGraph2.ScrollMaxY = 0D;
			this.zgGraph2.ScrollMaxY2 = 0D;
			this.zgGraph2.ScrollMinX = 0D;
			this.zgGraph2.ScrollMinY = 0D;
			this.zgGraph2.ScrollMinY2 = 0D;
			this.zgGraph2.Size = new System.Drawing.Size(514, 22);
			this.zgGraph2.TabIndex = 6;
			// 
			// FrmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(911, 430);
			this.Controls.Add(this.zgGraph2);
			this.Controls.Add(this.splitter3);
			this.Controls.Add(this.zgGraph3);
			this.Controls.Add(this.splitter2);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.zgGraph1);
			this.Controls.Add(this.graphViewer);
			this.Name = "FrmMain";
			this.Text = "Content-Addressable Butterfly Network";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.FrmMain_Load);
			this.ResumeLayout(false);

        }

        #endregion

		private UserControls.GraphViewer.GraphViewer graphViewer;
		private ZedGraph.ZedGraphControl zgGraph1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Splitter splitter2;
		private ZedGraph.ZedGraphControl zgGraph3;
		private System.Windows.Forms.Splitter splitter3;
		private ZedGraph.ZedGraphControl zgGraph2;



	}
}

