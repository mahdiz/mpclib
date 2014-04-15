using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MpcLib.UserControls.GraphViewer
{
	public partial class GraphViewer : UserControl
	{
		[Browsable(false)]
		public Graph Graph { get; set; }

		[DefaultValue(50)]
		public int GraphSize { get; set; }

		[DefaultValue(SmoothingMode.AntiAlias)]
		public SmoothingMode GraphSmoothingMode { get; set; }

		[DefaultValue(EdgeConnectionStyle.FreeStyle)]
		public EdgeConnectionStyle EdgeConnectionStyle { get; set; }

		[DefaultValue(true)]
		public bool DisplayNodeLabels { get; set; }

		[DefaultValue(true)]
		public bool DisplayEdgeLabels { get; set; }

		public GraphViewer()
		{
			GraphSize = 50;
			GraphSmoothingMode = SmoothingMode.AntiAlias;
			DisplayNodeLabels = true;
			DisplayEdgeLabels = true;
			InitializeComponent();
		}

		private void DrawGraph(Graphics g, Graph graph, int size)
		{
			g.SmoothingMode = GraphSmoothingMode;
			var visited = new List<Pair<GraphNode, GraphNode>>();

			foreach (var node in graph.Nodes)
			{
				DrawNode(g, node.Label, node.X, node.Y, size);
				foreach (var edge in node.ToNodes)
				{
					if (visited.Any(p => p.First == node && p.Second == edge.Key))
						continue;

					if (edge.Key.ToNodes.ContainsKey(node))
					{
						var reverseEdge = edge.Key.ToNodes.FirstOrDefault(n => n.Key == node);
						DrawDoubleEdge(g, node, edge.Key, edge.Value, reverseEdge.Value, size);
						visited.Add(new Pair<GraphNode, GraphNode>(edge.Key, node));
					}
					else
					{
						DrawSingleEdge(g, node, edge.Key, edge.Value, size);
					}
					visited.Add(new Pair<GraphNode, GraphNode>(node, edge.Key));
				}
			}
		}

		private void DrawDoubleEdge(Graphics g, GraphNode node1, GraphNode node2, string label1, string label2, int size)
		{
			// todo: incomplete implementation!
			var edge = CalculateEdgeCoordintes(node1, node2, size);
			DrawEdge(g, label1, edge.X1, edge.Y1, edge.X2, edge.Y2, size, true);

			//var edge1 = CalculateEdgeCoordintes(node1, node2, size);
			//var edge2 = CalculateEdgeCoordintes(node2, node1, size);

			//var dx = 0.015 * size - 0.32;
			//var dy = (edge1.X2 - edge1.X1) / (edge1.Y2 - edge1.Y1) * dx;
			//edge1.X1 += (int) Math.Round(dx);
			//edge1.Y1 += (int) Math.Round(dy);
			//edge1.X2 += (int) Math.Round(dx);
			//edge1.Y2 += (int) Math.Round(dy);
			//edge2.X1 -= (int) Math.Round(dx);
			//edge2.Y1 -= (int) Math.Round(dy);
			//edge2.X2 -= (int) Math.Round(dx);
			//edge2.Y2 -= (int) Math.Round(dy);

			//DrawEdge(g, label1, edge1.X1, edge1.Y1, edge1.X2, edge1.Y2, size);
			//DrawEdge(g, label2, edge2.X1, edge2.Y1, edge2.X2, edge2.Y2, size);
		}

		private void DrawSingleEdge(Graphics g, GraphNode node1, GraphNode node2, string label, int size)
		{
			var edge = CalculateEdgeCoordintes(node1, node2, size);
			DrawEdge(g, label, edge.X1, edge.Y1, edge.X2, edge.Y2, size, false);
		}

		private LineCoordinate CalculateEdgeCoordintes(GraphNode node1, GraphNode node2, int size)
		{
			double r = size / 2.0;
			int x1 = node1.X, x2 = node2.X, y1 = node1.Y, y2 = node2.Y;

			switch (EdgeConnectionStyle)
			{
				case EdgeConnectionStyle.FreeStyle:
					int dx1 = (int)Math.Round(Math.Abs(x2 - x1) / Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)) * r);
					int dy1 = (int)Math.Round(Math.Abs(y2 - y1) / Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)) * r);

					if (node2.X > node1.X)
					{
						x1 += dx1;
						x2 -= dx1;
					}
					else
					{
						x1 -= dx1;
						x2 += dx1;
					}
					if (node2.Y > node1.Y)
					{
						y1 += dy1;
						y2 -= dy1;
					}
					else
					{
						y1 -= dy1;
						y2 += dy1;
					}
					break;

				case EdgeConnectionStyle.TopBottom:
					if (node2.Y > node1.Y)
					{
						y1 = (int)Math.Ceiling(y1 + r);
						y2 = (int)Math.Ceiling(y2 - r);
					}
					else
					{
						y1 = (int)Math.Ceiling(y1 - r);
						y2 = (int)Math.Ceiling(y2 + r);
					}
					break;

				case EdgeConnectionStyle.LeftRight:
					if (node2.X > node1.X)
					{
						x1 = (int)Math.Ceiling(x1 + r);
						x2 = (int)Math.Ceiling(x2 - r);
					}
					else
					{
						x1 = (int)Math.Ceiling(x1 - r);
						x2 = (int)Math.Ceiling(x2 + r) + 1;
					}
					break;

				default:
					throw new NotSupportedException();
			}

			return new LineCoordinate(x1, y1, x2, y2);
		}

		private void DrawEdge(Graphics g, string label, int x1, int y1, int x2, int y2, int size, bool doubleArrow)
		{
			int penWidth = size / 15;
			if (penWidth < 2)
				penWidth = 2;

			var p = new Pen(Color.Black, penWidth);

			if (doubleArrow)
				p.CustomStartCap = new AdjustableArrowCap(4, 6, true);
			else
				p.StartCap = LineCap.Round;

			p.CustomEndCap = new AdjustableArrowCap(4, 6, true);
			g.DrawLine(p, x1, y1, x2, y2);

			//if (label != string.Empty)
			//{
			//    var angle = (float)(Math.Atan((y2 - y1) / (x2 - x1)) * 57.295779) + 15;
			//    DrawText(g, label, x2, y2, angle, size);
			//}
		}

		private void DrawText(Graphics g, string text, int x, int y, float angle, int size)
		{
			var brush = new SolidBrush(Color.Black);
			var font = new Font(Font.FontFamily, size / 3, FontStyle.Regular);
			g.RotateTransform(angle, MatrixOrder.Append);
			g.DrawString(text, font, brush, x, y);
			g.ResetTransform();
		}

		private void DrawNode(Graphics g, string label, int x, int y, int d)
		{
			int penWidth = d / 15;
			if (penWidth < 2)
				penWidth = 2;

			var p = new Pen(Color.Black, penWidth);
			var b = new SolidBrush(Color.Black);

			var labelFont = new Font(Font.FontFamily, (float)(d / 3.5), FontStyle.Regular);
			var labelSize = g.MeasureString(label, labelFont);

			g.DrawEllipse(p, x - d / 2, y - d / 2, d, d);

			if (DisplayNodeLabels)
			{
				g.DrawString(label, labelFont, b,
					x - labelSize.Width / 2,
					y - labelSize.Height / 2);
			}
		}

		private void GraphViewer_Paint(object sender, PaintEventArgs e)
		{
			if (Graph != null)
			{
				var g = e.Graphics;
				DrawGraph(g, Graph, GraphSize);
			}
		}
	}
}