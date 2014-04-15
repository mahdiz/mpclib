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
using System.Collections.Generic;

namespace MpcLib.Common.BasicDataStructures
{
	/// <summary>
	/// A custom queue datastructure extending the default .NET Queue.
	/// </summary>
	public sealed class VisitableQueue<T> : Queue<T>, IQueue<T>, IVisitableCollection<T>
	{
		#region Construction

		/// <summary>
		/// Initializes a new instance of the <see cref="Queue&lt;T&gt;"/> class.
		/// </summary>
		public VisitableQueue()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Queue&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="capacity">The capacity.</param>
		public VisitableQueue(int capacity)
			: base(capacity)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Queue&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="collection">The collection whose elements are copied to the new <see cref="T:System.Collections.Generic.Queue`1"></see>.</param>
		/// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
		public VisitableQueue(IEnumerable<T> collection)
			: base(collection)
		{
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

			VisitableQueue<T>.Enumerator enumerator = this.GetEnumerator();

			while (enumerator.MoveNext())
			{
				visitor.Visit(enumerator.Current);

				if (visitor.HasCompleted)
				{
					break;
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
				return this.Count == 0;
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

		#endregion IVisitableCollection<T> Members

		#region IComparable<T> Members

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
				VisitableQueue<T> q = obj as VisitableQueue<T>;

				return this.Count.CompareTo(q.Count);
			}
			else
			{
				return this.GetType().FullName.CompareTo(obj.GetType().FullName);
			}
		}

		#endregion IComparable<T> Members

		#region ICollection<T> Members

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
		/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
		public void Add(T item)
		{
			this.Enqueue(item);
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
		public bool IsReadOnly
		{
			get
			{
				return false;
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
			throw new NotSupportedException();
		}

		#endregion ICollection<T> Members
	}
}