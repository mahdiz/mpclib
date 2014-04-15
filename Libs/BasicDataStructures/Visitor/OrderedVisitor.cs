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

namespace MpcLib.Common.BasicDataStructures
{
	/// <summary>
	/// A visitor that visits objects in order (PreOrder, PostOrder, or InOrder).
	/// Used primarily as a base class for Visitors specializing in a specific order type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class OrderedVisitor<T>
	{
		#region Globals

		private IVisitor<T> visitorToUse;

		#endregion Globals

		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="OrderedVisitor&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="visitorToUse">The visitor to use when visiting the object.</param>
		public OrderedVisitor(IVisitor<T> visitorToUse)
		{
			if (visitorToUse == null)
			{
				throw new ArgumentNullException("visitorToUse");
			}
			else
			{
				this.visitorToUse = visitorToUse;
			}
		}

		#endregion Construction

		#region IOrderedVisitor<T> Members

		/// <summary>
		/// Determines whether this visitor is done.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// 	<c>true</c> if this visitor is done; otherwise, <c>false</c>.
		/// </returns>
		public bool HasCompleted
		{
			get
			{
				return visitorToUse.HasCompleted;
			}
		}

		/// <summary>
		/// Visits the object in pre order.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void VisitPreOrder(T obj)
		{
			visitorToUse.Visit(obj);
		}

		/// <summary>
		/// Visits the object in post order.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void VisitPostOrder(T obj)
		{
			visitorToUse.Visit(obj);
		}

		/// <summary>
		/// Visits the object in order.
		/// </summary>
		/// <param name="obj">The obj.</param>
		public virtual void VisitInOrder(T obj)
		{
			visitorToUse.Visit(obj);
		}

		#endregion IOrderedVisitor<T> Members

		#region Public Members

		/// <summary>
		/// Gets the visitor to use.
		/// </summary>
		/// <value>The visitor to use.</value>
		public IVisitor<T> VisitorToUse
		{
			get
			{
				return visitorToUse;
			}
		}

		#endregion Public Members
	}
}