using System;
using System.Collections.Generic;

namespace MpcLib.UserControls.GraphViewer
{
	public class GraphNode
	{
		public string Label { get; set; }

		public int X { get; set; }

		public int Y { get; set; }

		public Dictionary<GraphNode, string> ToNodes { get; private set; }

		public GraphNode(string label)
			: this(label, -1, -1)
		{
		}

		public GraphNode(string label, int x, int y)
		{
			if (label == string.Empty)
				throw new ArgumentNullException();

			Label = label;
			X = x;
			Y = y;
			ToNodes = new Dictionary<GraphNode, string>();
		}

		internal void Connect(GraphNode toNode, string label)
		{
			ToNodes.Add(toNode, label);
		}

		internal void Disconnect(GraphNode fromNode)
		{
			ToNodes.Remove(fromNode);
		}
	}
}