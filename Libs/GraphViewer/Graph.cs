using System;
using System.Collections.Generic;
using System.Linq;

namespace MpcLib.UserControls.GraphViewer
{
	public class Graph
	{
		public List<GraphNode> Nodes { get; private set; }

		public Graph()
		{
			Nodes = new List<GraphNode>();
		}

		public void AddNode(GraphNode node)
		{
			if (Nodes.Contains(node) || Nodes.Any(n => n.Label == node.Label))
				throw new Exception();

			Nodes.Add(node);
		}

		public void AddEdge(GraphNode src, GraphNode dest)
		{
			AddEdge(src, dest, "");
		}

		public void AddEdge(string srcLabel, string destLabel)
		{
			AddEdge(srcLabel, destLabel, "");
		}

		public void AddEdge(string srcLabel, string destLabel, string label)
		{
			AddEdge(Nodes.First(n => n.Label == srcLabel), Nodes.First(n => n.Label == destLabel));
		}

		private void AddEdge(GraphNode src, GraphNode dest, string label)
		{
			if (!Nodes.Contains(src))
				AddNode(src);

			if (!Nodes.Contains(dest))
				AddNode(dest);

			src.Connect(dest, label);
		}
	}
}