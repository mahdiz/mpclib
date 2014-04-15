using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Unm.UserControls.GraphViewer;
using Unm.DistributedSystem.ButterflyNetwork;
using System.IO;
using ZedGraph;

namespace Unm.DistributedSystem.ButterflyNetworkApp
{
    public partial class FrmMain : Form
    {
		private GraphPane gPane1, gPane2, gPane3;
		private LineItem curveMessages, curveTime, curveMemory;
		private double searchStartTime;
		private int n;

        public FrmMain()
        {
            InitializeComponent();
        }

		private void InitGraphPane(GraphPane gPane, string xText, string yText)
		{
			gPane.Border.IsVisible = false;
			gPane.Title.IsVisible = false;
			gPane.XAxis.Title.Text = xText;
			gPane.YAxis.Title.Text = yText;
		}

		private void FrmMain_Load(object sender, EventArgs args)
		{
			gPane1 = zgGraph1.GraphPane;
			gPane2 = zgGraph2.GraphPane;
			gPane3 = zgGraph3.GraphPane;
			InitGraphPane(gPane1, "Log(n)^2  (n = Network Size)", "Total Bits Sent");
			InitGraphPane(gPane2, "Log(n)  (n = Network Size)", "Total Search Time (ms)");
			InitGraphPane(gPane3, "Log(n)  (n = Network Size)", "Avg. Space Per Node (bits)");

			//InitGraphPane(gPane1, "Network Size", "Total Bits Sent");
			//InitGraphPane(gPane2, "Network Size", "Total Search Time (ms)");
			//InitGraphPane(gPane3, "Network Size", "Avg. Space Per Node (bits)");

			curveMessages = gPane1.AddCurve("Total Bits Sent vs. Size of Network", null, Color.Blue, SymbolType.None);
			curveTime = gPane2.AddCurve("Total Search Time vs. Size of Network", null, Color.Red, SymbolType.None);
			curveMemory = gPane3.AddCurve("Total Memory vs. Size of Network", null, Color.Green, SymbolType.None);

			curveMessages.Line.Width = 1.6F;
			curveMessages.Line.IsAntiAlias = true;
			curveTime.Line.Width = 1.6F;
			curveTime.Line.IsAntiAlias = true;
			curveMemory.Line.Width = 1.6F;
			curveMemory.Line.IsAntiAlias = true;

			for (int i = 6; i < 11; i++)
			{
				var bn = new ButterflyNetwork<Image>();	    // 842667722

				n = (int)Math.Pow(2, i);
				int B = 3, C = 3, D = 3, T = 3;
				double a = 0.1, b = 6.5; 

				//int n = (int)Math.Pow(2, i);
				//double e = 0.05;
				//double d = 0.9;
				//double a = 0.25;
				//double ap = 0.095;
				//double b = 1.001;
				//double g = 0.2;
				//int B = (int)Math.Round(1 / g * Math.Log(Math.E/e, Math.E));
				//int C = (int)Math.Round(10 / 3 * (2 * Math.Log(2 * Math.E, Math.E)) / (d * (1 - g) * Math.Pow(1 - (2 * a), 2)));
				//int D = (int)Math.Round(b / (ap * (a - ap)) * (ap * Math.Log(b * Math.E / ap, Math.E) + (a - ap) * Math.Log(b * Math.E / (a - ap), Math.E) + 2/C));
				//int T = (int)Math.Round(1 / (1 - d) * Math.Log(Math.E / e, Math.E));

				bn.InitializationFinished += new ButterflyNetwork<Image>.InitializationHandler(bn_InitializationFinished);
				bn.SearchStarted += new ButterflyNetwork<Image>.SearchFinishedHandler(bn_SearchStarted);
				bn.SearchFinished += new ButterflyNetwork<Image>.SearchFinishedHandler(bn_SearchFinished);

				bn.InitializeNetwork(n, B, C, D, T, a, b);

				var images = new List<ImageDataItem>();
				var filePaths = Directory.GetFiles(@"c:\images256");
				foreach (var filePath in filePaths.Take(2))
				{
					images.Add(new ImageDataItem(filePath));
				}
				bn.StoreDataItems(images);

				var query = new ImageDataItem()
				{
					Title = Path.GetFileNameWithoutExtension(filePaths[0])
				};

				bn.Search(query, 8);
				bn.Run();

				curveMessages.AddPoint(n, bn.SentSize * 8 / n);
				zgGraph1.RestoreScale(gPane1);
			}
		}

		private void bn_SearchFinished(double simTime, Node<Image> node)
		{
			for (int i = 0; i < curveTime.Points.Count; i++)
				if (curveTime[i].X == n) return;

			curveTime.AddPoint(n, simTime - searchStartTime);
			curveMemory.AddPoint(n, node.GetSize() * 8);

			zgGraph2.RestoreScale(gPane2);
			zgGraph3.RestoreScale(gPane3);
		}

		private void bn_SearchStarted(double simTime, Node<Image> nodeId)
		{
			searchStartTime = simTime;
		}

		private void bn_InitializationFinished(Supernode[,] supernodes)
		{
			// draw the network
			var g = new Graph();

			var numRows = supernodes.GetLength(0);
			var numCols = supernodes.GetLength(1);
			int x, y = 50;
			for (int i = 0; i < numRows; i++)
			{
			    x = 30;
			    for (int j = 0; j < numCols; j++)
			    {
					var node = supernodes[i, j];
			        g.AddNode(new GraphNode(node.Id.ToString(), x, y));
			        x += 40;
			    }
			    y += 95;
			}

			for (int i = 0; i < numRows; i++)
			{
			    for (int j = 0; j < numCols; j++)
			    {
					var superNode = supernodes[i, j];
			        if (superNode.LowerNodes.Length > 0)
			        {
			            foreach (var lowerNode in superNode.LowerNodes)
			            {
			                g.AddEdge(superNode.Id.ToString(), lowerNode.Id.ToString());
			            }
			        }
			    }
			}

			graphViewer.GraphSize = 33;
			graphViewer.Graph = g;
			graphViewer.Refresh();
		}
	}
}
