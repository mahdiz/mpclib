/*
 Author: Riaan Hanekom

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

using System.Collections.Generic;
using System.Diagnostics;

namespace MpcLib.Common.BasicDataStructures.Graph
{
	/// <summary>
	/// A class representing a vertex in a graph.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Vertex<T>
	{
		#region Globals

		private T vertexData;
		private List<Edge<T>> incidentEdges = null;
		private List<Edge<T>> emanatingEdges = null;
		private double vertexWeight;

		#endregion Globals

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="Vertex&lt;T&gt;"/> class.
		/// The weight is 0 by default.
		/// </summary>
		/// <param name="data">The data contained in the vertex.</param>
		public Vertex(T data)
		{
			this.vertexData = data;
			var incidentEdges = new List<Edge<T>>();
			var emanatingEdges = new List<Edge<T>>();
			vertexWeight = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vertex&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="data">The data contained in the vertex</param>
		/// <param name="weight">The weight of the vertex.</param>
		public Vertex(T data, double weight)
		{
			this.vertexData = data;
			var incidentEdges = new List<Edge<T>>();
			var emanatingEdges = new List<Edge<T>>();
			vertexWeight = weight;
		}

		#endregion Construction

		#region Public Members

		/// <summary>
		/// Gets or sets the weight.
		/// </summary>
		/// <value>The weight.</value>
		public double Weight
		{
			get
			{
				return vertexWeight;
			}
			set
			{
				vertexWeight = value;
			}
		}

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>The data contained in the vertex.</value>
		public T Data
		{
			get
			{
				return vertexData;
			}
			set
			{
				vertexData = value;
			}
		}

		/// <summary>
		/// Gets the count of the incident edges on this vertex.
		/// </summary>
		/// <value>The count of the incident edges on this vertex.</value>
		public int IncidentEdgesCount
		{
			get
			{
				return incidentEdges.Count;
			}
		}

		/// <summary>
		/// Gets the degree of this vertex (the number of emanating edges).
		/// </summary>
		/// <value>The degree.</value>
		public int Degree
		{
			get
			{
				return emanatingEdges.Count;
			}
		}

		/// <summary>
		/// Gets the edges incident on this vertex.
		/// </summary>
		/// <value>The edges incident on this vertex.</value>
		public List<Edge<T>> IncidentEdges
		{
			get
			{
				return incidentEdges;
			}
		}

		/// <summary>
		/// Gets the emanating edges on this vertex.
		/// </summary>
		/// <value>The emanating edges on this vertex.</value>
		public List<Edge<T>> EmanatingEdges
		{
			get
			{
				return emanatingEdges;
			}
		}

		/// <summary>
		/// Determines whether this vertex has an emanating edge to the specified vertex.
		/// </summary>
		/// <param name="toVertex">The vertex to test connectivity to.</param>
		/// <returns>
		/// 	<c>true</c> if this vertex has an emanating edge to the specified vertex; otherwise, <c>false</c>.
		/// </returns>
		public bool HasEmanatingEdgeTo(Vertex<T> toVertex)
		{
			for (int i = 0; i < emanatingEdges.Count; i++)
			{
				if (emanatingEdges[i].IsDirected)
				{
					if (emanatingEdges[i].ToVertex == toVertex)
					{
						return true;
					}
				}
				else
				{
					if ((emanatingEdges[i].ToVertex == toVertex) || ((emanatingEdges[i].FromVertex == toVertex)))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether [has incident edge with] [the specified from vertex].
		/// </summary>
		/// <param name="fromVertex">From vertex.</param>
		/// <returns>
		/// 	<c>true</c> if [has incident edge with] [the specified from vertex]; otherwise, <c>false</c>.
		/// </returns>
		public bool HasIncidentEdgeWith(Vertex<T> fromVertex)
		{
			for (int i = 0; i < incidentEdges.Count; i++)
			{
				if ((incidentEdges[i].FromVertex == fromVertex) || (incidentEdges[i].ToVertex == fromVertex))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the emanating edge to the specified vertex.
		/// </summary>
		/// <param name="toVertex">To to vertex.</param>
		/// <returns>The emanating edge to the vertex specified if found, otherwise null.</returns>
		public Edge<T> GetEmanatingEdgeTo(Vertex<T> toVertex)
		{
			for (int i = 0; i < emanatingEdges.Count; i++)
			{
				if (emanatingEdges[i].IsDirected)
				{
					if (emanatingEdges[i].ToVertex == toVertex)
					{
						return emanatingEdges[i];
					}
				}
				else
				{
					if ((emanatingEdges[i].FromVertex == toVertex) || (emanatingEdges[i].ToVertex == toVertex))
					{
						return emanatingEdges[i];
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the incident edge to the specified vertex.
		/// </summary>
		/// <param name="toVertex">The to vertex.</param>
		/// <returns>The incident edge to the vertex specified if found, otherwise null.</returns>
		public Edge<T> GetIncidentEdgeWith(Vertex<T> toVertex)
		{
			for (int i = 0; i < incidentEdges.Count; i++)
			{
				if ((incidentEdges[i].ToVertex == toVertex) || (incidentEdges[i].FromVertex == toVertex))
				{
					return incidentEdges[i];
				}
			}

			return null;
		}

		#endregion Public Members

		#region Internal Members

		internal List<Edge<T>> IncidentEdgeList
		{
			get
			{
				return incidentEdges;
			}
		}

		internal List<Edge<T>> EmanatingEdgeList
		{
			get
			{
				return emanatingEdges;
			}
		}

		/// <summary>
		/// Removes the edge specified from the vertex.
		/// </summary>
		/// <param name="edge">The edge to be removed.</param>
		internal void RemoveEdge(Edge<T> edge)
		{
			#region Asserts

			Debug.Assert(edge != null);

			#endregion Asserts

			RemoveEdgeFromVertex(edge);
		}

		internal void AddEdge(Edge<T> edge)
		{
			#region Asserts

			Debug.Assert(edge != null);

			#endregion Asserts

			if (edge.IsDirected)
			{
				if (edge.FromVertex == this)
				{
					emanatingEdges.Add(edge);
				}
			}
			else
			{
				emanatingEdges.Add(edge);
			}

			incidentEdges.Add(edge);
		}

		#endregion Internal Members

		#region Private Members

		private void RemoveEdgeFromVertex(Edge<T> edge)
		{
			#region Asserts

			Debug.Assert(this.incidentEdges.Remove(edge), "Edge not found on vertex in RemoveEdgeFromVertex.");

			#endregion Asserts

			this.incidentEdges.Remove(edge);

			if (edge.IsDirected)
			{
				if (edge.FromVertex == this)
				{
					emanatingEdges.Remove(edge);
				}
			}
			else
			{
				emanatingEdges.Remove(edge);
			}
		}

		#endregion Private Members
	}
}