using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MpcLib.Common.BasicDataStructures.Dag
{
	/// <summary>
	/// Represents a node (vertex) to be used in a fast Directed-Acyclic Graph (DAG).
	/// </summary>
	public abstract class Node<V> where V : Node<V>
	{
		public Node()
		{
		}

		private List<V> inNodes = new List<V>();

		private List<V> outNodes = new List<V>();

		/// <summary>
		/// All nodes connected to this node through incidenting edges.
		/// This property is an O(1) operation.
		/// </summary>
		public ReadOnlyCollection<V> InNodes
		{
			get
			{
				return inNodes.AsReadOnly();
			}
		}

		/// <summary>
		/// All nodes connected to this node through emanating edges.
		/// This property is an O(1) operation.
		/// </summary>
		public ReadOnlyCollection<V> OutNodes
		{
			get
			{
				return outNodes.AsReadOnly();
			}
		}

		/// <summary>
		/// All nodes connected to this node through incident or emanating edges.
		/// </summary>
		public IEnumerable<V> ConnectedNodes
		{
			get
			{
				return inNodes.Concat(outNodes);
			}
		}

		public void AddEdgeTo(V to)
		{
			Debug.Assert(to != null);

			if (outNodes.Contains(to) || to.inNodes.Contains(this))
				throw new Exception("There is already an edge from " + this + " to " + to + " in the DAG.");

			outNodes.Add(to);
			to.inNodes.Add(this as V);
		}

		public void RemoveEdgeTo(V to)
		{
			Debug.Assert(to != null);

			outNodes.Remove(to);
			to.inNodes.Remove(this as V);
		}
	}
}