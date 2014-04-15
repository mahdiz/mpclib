/*
 Author: Riaan Hanekom
 Modified by: Mahdi Zamani

 Copyright 2007 Riaan Hanekom

 Permission is hereby granted, free of charge, to any person obtaining
 a copy of this software and associated documentation files (the
 "Software"), to deal in the Software without restriction, including
 without limitation the rights to use, copy, modify, merge, publish,
 distribute, sublicense, and/or sell copies of the Software, and to
 permit persons to whom the Software is furnished to do so, subject to
 the following conditions:

 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MpcLib.Common.BasicDataStructures.Graph
{
	/// <summary>
	/// An implementation of a Graph data structure.  The graph can be either
	/// directed or undirected.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Graph<T> : IVisitableCollection<T>
	{
		#region Globals

		protected List<Vertex<T>> graphVertices;
		protected List<Edge<T>> graphEdges;
		protected bool graphIsDirected;

		#endregion Globals

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="Graph&lt;T&gt;"/> class.
		/// </summary>
		public Graph()
			: this(false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Graph&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="isDirected">if set to <c>true</c> [is directed].</param>
		public Graph(bool isDirected)
		{
			graphIsDirected = isDirected;
			graphVertices = new List<Vertex<T>>();
			graphEdges = new List<Edge<T>>();
		}

		#endregion Construction

		#region IVisitableCollection<T> Members

		/// <summary>
		/// Accepts the specified visitor.
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		public void Accept(IVisitor<T> visitor)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException("visitor");
			}

			using (IEnumerator<T> enumerator = this.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					visitor.Visit(enumerator.Current);

					if (visitor.HasCompleted)
					{
						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is of a fixed size.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is fixed size; otherwise, <c>false</c>.
		/// </value>
		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this collection is empty.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this collection is empty; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty
		{
			get
			{
				return this.VertexCount == 0;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this collection is full.
		/// </summary>
		/// <value><c>true</c> if this collection is full; otherwise, <c>false</c>.</value>
		public bool IsFull
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.
		/// </summary>
		/// <value></value>
		/// <returns>The number of elements contained in the <see cref="T:System.Collections.ICollection"></see>.</returns>
		int ICollection<T>.Count
		{
			get
			{
				return VertexCount;
			}
		}

		/// <summary>
		/// Adds a vertex to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <param name="item">The vertex data to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
		void ICollection<T>.Add(T item)
		{
			AddVertex(new Vertex<T>(item));
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
		/// <returns>
		/// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
		/// </returns>
		bool ICollection<T>.Contains(T item)
		{
			return ContainsVertex(item);
		}

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
		/// <exception cref="T:System.ArgumentNullException">array is null.</exception>
		/// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if ((array.Length - arrayIndex) < this.VertexCount)
			{
				throw new ArgumentException();
			}

			int counter = arrayIndex;

			for (int i = 0; i < graphVertices.Count; i++)
			{
				array.SetValue(graphVertices[i].Data, counter);
				counter++;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
		/// <returns>
		/// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
		bool ICollection<T>.Remove(T item)
		{
			return RemoveVertex(item);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < graphVertices.Count; i++)
			{
				yield return graphVertices[i].Data;
			}
		}

		/// <summary>
		/// Clears all the objects in this instance.
		/// </summary>
		public void Clear()
		{
			graphVertices.Clear();
			graphEdges.Clear();
		}

		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than obj. Zero This instance is equal to obj. Greater than zero This instance is greater than obj.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">obj is not the same type as this instance. </exception>
		public int CompareTo(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}

			if (obj.GetType() == this.GetType())
			{
				Graph<T> graph = obj as Graph<T>;
				return this.VertexCount.CompareTo(graph.VertexCount);
			}
			else
			{
				return this.GetType().FullName.CompareTo(obj.GetType().FullName);
			}
		}

		#endregion IVisitableCollection<T> Members

		#region Public Members

		/// <summary>
		/// Performs a depth-first traversal, starting at the specified vertex.
		/// </summary>
		/// <param name="visitor">The visitor to use.  Note that in-order is not applicable in a graph.</param>
		/// <param name="startVertex">The vertex to start from.</param>
		public void DepthFirstTraversal(OrderedVisitor<Vertex<T>> visitor, Vertex<T> startVertex)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException("visitor");
			}

			if (startVertex == null)
			{
				throw new ArgumentNullException("startVertex");
			}

			var visitedVertices = new List<Vertex<T>>(graphVertices.Count);
			DepthFirstTraversal(visitor, startVertex, ref visitedVertices);
		}

		/// <summary>
		/// Performs a breadth-first traversal from the specified vertex.
		/// </summary>
		/// <param name="visitor">The visitor to use.</param>
		/// <param name="startVertex">The vertex to start from.</param>
		public void BreadthFirstTraversal(IVisitor<Vertex<T>> visitor, Vertex<T> startVertex)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException("visitor");
			}

			if (startVertex == null)
			{
				throw new ArgumentNullException("startVertex");
			}

			var visitedVertices = new List<Vertex<T>>(graphVertices.Count);

			var q = new VisitableQueue<Vertex<T>>();

			q.Enqueue(startVertex);
			visitedVertices.Add(startVertex);

			while (!((q.IsEmpty) || (visitor.HasCompleted)))
			{
				Vertex<T> vertex = q.Dequeue();

				visitor.Visit(vertex);

				List<Edge<T>> edges = vertex.EmanatingEdgeList;

				for (int i = 0; i < edges.Count; i++)
				{
					Vertex<T> vertexToVisit = edges[i].GetPartnerVertex(vertex);

					if (!visitedVertices.Contains(vertexToVisit))
					{
						q.Enqueue(vertexToVisit);
						visitedVertices.Add(vertexToVisit);
					}
				}
			}
		}

		/// <summary>
		/// Removes the specified vertex from the graph.
		/// </summary>
		/// <param name="vertex">The vertex to be removed.</param>
		/// <returns>A value indicating whether the vertex was found (and removed) in the graph.</returns>
		public bool RemoveVertex(Vertex<T> vertex)
		{
			if (vertex == null)
			{
				throw new ArgumentNullException("vertex");
			}

			if (!graphVertices.Remove(vertex))
			{
				return false;
			}
			else
			{
				// Delete all the edges in which this vertex forms part of
				List<Edge<T>> list = vertex.IncidentEdgeList;

				while (list.Count > 0)
				{
					RemoveEdge(list[0]);
				}

				return true;
			}
		}

		/// <summary>
		/// Removes the vertex with the specified value from the graph.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>A value indicating whether a vertex with the value specified was found (and removed) in the graph.</returns>
		public bool RemoveVertex(T item)
		{
			for (int i = 0; i < graphVertices.Count; i++)
			{
				if (graphVertices[i].Data.Equals(item))
				{
					RemoveVertex(graphVertices[i]);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the edge count.
		/// </summary>
		/// <value>The edge count.</value>
		public int EdgeCount
		{
			get
			{
				return graphEdges.Count;
			}
		}

		/// <summary>
		/// Determines whether this graph contains the specified vertex.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		/// <returns>
		/// 	<c>true</c> if this instance contains the specified vertex; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsVertex(Vertex<T> vertex)
		{
			return graphVertices.Contains(vertex);
		}

		/// <summary>
		/// Determines whether the specified item is contained in the graph.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns>
		/// 	<c>true</c> if the specified item contains vertex; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsVertex(T item)
		{
			for (int i = 0; i < graphVertices.Count; i++)
			{
				if (graphVertices[i].Data.Equals(item))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the vertice count.
		/// </summary>
		/// <value>The vertice count.</value>
		public int VertexCount
		{
			get
			{
				return graphVertices.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is directed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is directed; otherwise, <c>false</c>.
		/// </value>
		public bool IsDirected
		{
			get
			{
				return graphIsDirected;
			}
		}

		/// <summary>
		/// Removes the edge specified from the graph.
		/// </summary>
		/// <param name="edge">The edge to be removed.</param>
		/// <returns>A value indicating whether the edge specified was found (and removed) from the graph.</returns>
		public bool RemoveEdge(Edge<T> edge)
		{
			CheckEdgeNotNull(edge);

			if (!graphEdges.Remove(edge))
			{
				return false;
			}

			edge.FromVertex.RemoveEdge(edge);
			edge.ToVertex.RemoveEdge(edge);

			return true;
		}

		/// <summary>
		/// Removes the edge specified from the graph.
		/// </summary>
		/// <param name="from">The from vertex.</param>
		/// <param name="to">The to vertex.</param>
		/// <returns>A value indicating whether the edge between the two vertices supplied was found (and removed) from the graph.</returns>
		public bool RemoveEdge(Vertex<T> from, Vertex<T> to)
		{
			if (from == null)
			{
				throw new ArgumentNullException("from");
			}

			if (to == null)
			{
				throw new ArgumentNullException("to");
			}

			if (graphIsDirected)
			{
				for (int i = 0; i < graphEdges.Count; i++)
				{
					if ((graphEdges[i].FromVertex == from) && (graphEdges[i].ToVertex == to))
					{
						RemoveEdge(graphEdges[i]);
						return true;
					}
				}
			}
			else
			{
				for (int i = 0; i < graphEdges.Count; i++)
				{
					if (((graphEdges[i].FromVertex == from) && (graphEdges[i].ToVertex == to)) ||
						((graphEdges[i].FromVertex == to) && (graphEdges[i].ToVertex == from)))
					{
						RemoveEdge(graphEdges[i]);
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Adds the specified edge to the graph.
		/// </summary>
		/// <param name="edge">The edge to add.</param>
		public void AddEdge(Edge<T> edge)
		{
			CheckEdgeNotNull(edge);

			if (edge.IsDirected != graphIsDirected)
			{
				throw new ArgumentException();
			}

			if ((!graphVertices.Contains(edge.FromVertex)) || (!graphVertices.Contains(edge.ToVertex)))
			{
				throw new ArgumentException();
			}

			if (edge.FromVertex.HasEmanatingEdgeTo(edge.ToVertex))
			{
				throw new ArgumentException();
			}

			graphEdges.Add(edge);
			AddEdgeToVertices(edge);
		}

		/// <summary>
		/// Adds the vertex specified to the graph.
		/// </summary>
		/// <param name="vertex">The vertex to add.</param>
		public void AddVertex(Vertex<T> vertex)
		{
			if (graphVertices.Contains(vertex))
			{
				throw new ArgumentException();
			}

			graphVertices.Add(vertex);
		}

		/// <summary>
		/// Adds a vertex to the graph with the specified data item.
		/// </summary>
		/// <param name="item">The item to store in the vertex.</param>
		/// <returns>The vertex created and added to the graph.</returns>
		public Vertex<T> AddVertex(T item)
		{
			var vertex = new Vertex<T>(item);
			graphVertices.Add(vertex);
			return vertex;
		}

		/// <summary>
		/// Adds the edge to the graph.
		/// </summary>
		/// <param name="from">The from vertex.</param>
		/// <param name="to">The to vertex.</param>
		/// <returns>The newly created edge.</returns>
		public Edge<T> AddEdge(Vertex<T> from, Vertex<T> to)
		{
			var edge = new Edge<T>(from, to, graphIsDirected);
			AddEdge(edge);
			return edge;
		}

		/// <summary>
		/// Adds the edge to the graph.
		/// </summary>
		/// <param name="from">The from vertex.</param>
		/// <param name="to">The to vertex.</param>
		/// <param name="weight">The weight of this edge.</param>
		public void AddEdge(Vertex<T> from, Vertex<T> to, double weight)
		{
			var edge = new Edge<T>(from, to, weight, graphIsDirected);
			AddEdge(edge);
		}

		/// <summary>
		/// Gets the vertices contained in this graph.
		/// </summary>
		/// <value>The vertices contained in this graph.</value>
		public List<Vertex<T>> Vertices
		{
			get
			{
				return graphVertices;
			}
		}

		/// <summary>
		/// Gets the edges contained in this graph.
		/// </summary>
		/// <value>The edges contained in this graph.</value>
		public IEnumerator<Edge<T>> Edges
		{
			get
			{
				return graphEdges.GetEnumerator();
			}
		}

		/// <summary>
		/// Determines whether the vertex with the specified from value has an edge to a vertex with the specified to value.
		/// </summary>
		/// <param name="fromValue">The from vertex value.</param>
		/// <param name="toValue">The to vertex value.</param>
		/// <returns>
		/// 	<c>true</c> if the vertex with the specified from value has an edge to a vertex with the specified to value; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsEdge(T fromValue, T toValue)
		{
			if (graphIsDirected)
			{
				for (int i = 0; i < graphEdges.Count; i++)
				{
					if ((graphEdges[i].FromVertex.Data.Equals(fromValue) &&
						(graphEdges[i].ToVertex.Data.Equals(toValue))))
					{
						return true;
					}
				}
			}
			else
			{
				for (int i = 0; i < graphEdges.Count; i++)
				{
					if (((graphEdges[i].FromVertex.Data.Equals(fromValue) &&
						(graphEdges[i].ToVertex.Data.Equals(toValue)))) ||
						((graphEdges[i].FromVertex.Data.Equals(toValue) &&
						(graphEdges[i].ToVertex.Data.Equals(fromValue)))))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified vertex has a edge to the to vertex.
		/// </summary>
		/// <param name="from">The from vertex.</param>
		/// <param name="to">The to vertex.</param>
		/// <returns>
		/// 	<c>true</c> if the specified from vertex has an edge to the to vertex; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsEdge(Vertex<T> from, Vertex<T> to)
		{
			if (graphIsDirected)
			{
				return from.HasEmanatingEdgeTo(to);
			}
			else
			{
				return from.HasIncidentEdgeWith(to);
			}
		}

		/// <summary>
		/// Determines whether the specified edge is contained in this graph.
		/// </summary>
		/// <param name="edge">The edge to look for.</param>
		/// <returns>
		/// 	<c>true</c> if the specified edge is contained in the graph; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsEdge(Edge<T> edge)
		{
			return graphEdges.Contains(edge);
		}

		/// <summary>
		/// Gets the edge specified by the two vertices.
		/// </summary>
		/// <param name="from">The from vertex.</param>
		/// <param name="to">The two vertex.</param>
		/// <returns>The edge between the two specified vertices if found.</returns>
		public Edge<T> GetEdge(Vertex<T> from, Vertex<T> to)
		{
			return from.GetEmanatingEdgeTo(to);
		}

		#endregion Public Members

		#region Private Members

		/// <summary>
		/// Performs a depth-first traversal.
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		/// <param name="startVertex">The start vertex.</param>
		/// <param name="visitedVertices">The visited vertices.</param>
		private void DepthFirstTraversal(OrderedVisitor<Vertex<T>> visitor, Vertex<T> startVertex, ref List<Vertex<T>> visitedVertices)
		{
			if (visitor.HasCompleted)
			{
				return;
			}

			// Add the vertex to the "visited" list
			visitedVertices.Add(startVertex);

			// Visit the vertex in pre-order
			visitor.VisitPreOrder(startVertex);

			// Get the list of emanating edges from the vertex
			List<Edge<T>> edges = startVertex.EmanatingEdgeList;

			for (int i = 0; i < edges.Count; i++)
			{
				// Get the partner vertex of the start vertex
				Vertex<T> vertexToVisit = edges[i].GetPartnerVertex(startVertex);

				// If the vertex hasn't been visited before, do a depth-first
				// traversal starting at that vertex
				if (!visitedVertices.Contains(vertexToVisit))
				{
					DepthFirstTraversal(visitor, vertexToVisit, ref visitedVertices);
				}
			}

			// Visit the vertex in post order
			visitor.VisitPostOrder(startVertex);
		}

		/// <summary>
		/// Adds the edge to the vertices in the edge.
		/// </summary>
		/// <param name="edge">The edge to add.</param>
		private void AddEdgeToVertices(Edge<T> edge)
		{
			#region Asserts

			Debug.Assert(edge != null);
			Debug.Assert(edge.FromVertex != null);
			Debug.Assert(edge.ToVertex != null);

			#endregion Asserts

			edge.FromVertex.AddEdge(edge);
			edge.ToVertex.AddEdge(edge);
		}

		/// <summary>
		/// Checks that the edge is not null.
		/// </summary>
		/// <param name="edge">The edge to check.</param>
		private void CheckEdgeNotNull(Edge<T> edge)
		{
			if (edge == null)
			{
				throw new ArgumentNullException("edge");
			}

			#region Asserts

			// Since the edge constructor doesn't allow null vertices,
			// is shouldn't be necessary to check for those here.
			// Rather, substitute the exceptions with Asserts.

			Debug.Assert(edge.FromVertex != null);
			Debug.Assert(edge.ToVertex != null);

			#endregion Asserts
		}

		#endregion Private Members

		#region ICollection<T> Members

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		#endregion ICollection<T> Members

		#region IEnumerable Members

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>A enumerator to enumerate though the collection.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion IEnumerable Members
	}
}