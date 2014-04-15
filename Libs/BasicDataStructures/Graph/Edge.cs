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

using System;

namespace MpcLib.Common.BasicDataStructures.Graph
{
	/// <summary>
	/// A class representing an edge in a graph.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Edge<T>
	{
		#region Globals

		private Vertex<T> from;
		private Vertex<T> to;
		private double edgeWeight;
		private bool edgeIsDirected;

		#endregion Globals

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="Edge&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="fromVertex">From vertex.</param>
		/// <param name="toVertex">To vertex.</param>
		/// <param name="isDirected">if set to <c>true</c> [is directed].</param>
		public Edge(Vertex<T> fromVertex, Vertex<T> toVertex, bool isDirected)
			: this(fromVertex, toVertex, 0, isDirected)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Edge&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="fromVertex">From vertex.</param>
		/// <param name="toVertex">To vertex.</param>
		/// <param name="weight">The weight associated with the edge.</param>
		/// <param name="isDirected">if set to <c>true</c> [is directed].</param>
		public Edge(Vertex<T> fromVertex, Vertex<T> toVertex, double weight, bool isDirected)
		{
			if (fromVertex == null)
			{
				throw new ArgumentNullException("fromVertex");
			}

			if (toVertex == null)
			{
				throw new ArgumentNullException("toVertex");
			}

			from = fromVertex;
			to = toVertex;
			edgeWeight = weight;
			edgeIsDirected = isDirected;
		}

		#endregion Construction

		#region Public Members

		/// <summary>
		/// Gets the from vertex.
		/// </summary>
		/// <value>The from vertex.</value>
		public Vertex<T> FromVertex
		{
			get
			{
				return from;
			}
		}

		/// <summary>
		/// Gets the to vertex.
		/// </summary>
		/// <value>The to vertex.</value>
		public Vertex<T> ToVertex
		{
			get
			{
				return to;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this edge is directed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this edge is directed; otherwise, <c>false</c>.
		/// </value>
		public bool IsDirected
		{
			get
			{
				return edgeIsDirected;
			}
		}

		/// <summary>
		/// Gets the weight associated with this edge.
		/// </summary>
		/// <value>The weight associated with this edge.</value>
		public double Weight
		{
			get
			{
				return edgeWeight;
			}
		}

		/// <summary>
		/// Gets the partner vertex in this edge relationship.
		/// </summary>
		/// <param name="vertex">The vertex.</param>
		/// <returns>The partner of the vertex specified in this edge relationship.</returns>
		public Vertex<T> GetPartnerVertex(Vertex<T> vertex)
		{
			if (from == vertex)
			{
				return to;
			}
			else if (to == vertex)
			{
				return from;
			}
			else
			{
				throw new ArgumentException();
			}
		}

		#endregion Public Members
	}
}