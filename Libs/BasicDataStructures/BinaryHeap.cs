using System;
using System.Collections;
using System.Collections.Generic;

namespace MpcLib.Common.BasicDataStructures
{
	/// <summary>
	/// A binary heap, useful for sorting data and priority queues.
	/// </summary>
	/// <typeparam name="T"><![CDATA[IComparable<T> type of item in the heap]]>.</typeparam>
	public class BinaryHeap<T> : ICollection<T> where T : IComparable<T>
	{
		#region Fields

		private const int DEFAULT_SIZE = 4;
		private T[] data = new T[DEFAULT_SIZE];
		private int count = 0;
		private int capacity = DEFAULT_SIZE;
		private bool sorted;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets the number of values in the heap.
		/// </summary>
		public int Count
		{
			get { return count; }
		}

		/// <summary>
		/// Gets or sets the capacity of the heap.
		/// </summary>
		public int Capacity
		{
			get { return capacity; }
			set
			{
				int previousCapacity = capacity;
				capacity = Math.Max(value, count);
				if (capacity != previousCapacity)
				{
					var temp = new T[capacity];
					Array.Copy(data, temp, count);
					data = temp;
				}
			}
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Creates a new binary heap.
		/// </summary>
		public BinaryHeap()
		{
		}

		private BinaryHeap(T[] data, int count)
		{
			Capacity = count;
			this.count = count;
			Array.Copy(data, data, count);
		}

		/// <summary>
		/// Gets the first value in the heap without removing it.
		/// </summary>
		/// <returns>The lowest value of type TValue.</returns>
		public T Peek()
		{
			return data[0];
		}

		/// <summary>
		/// Removes all items from the heap.
		/// </summary>
		public void Clear()
		{
			this.count = 0;
			data = new T[capacity];
		}

		/// <summary>
		/// Adds a key and value to the heap.
		/// </summary>
		/// <param name="item">The item to add to the heap.</param>
		public void Add(T item)
		{
			if (count == capacity)
			{
				Capacity *= 2;
			}
			data[count] = item;
			UpHeap();
			count++;
		}

		/// <summary>
		/// Removes and returns the first item in the heap.
		/// </summary>
		/// <returns>The next value in the heap.</returns>
		public T Remove()
		{
			if (this.count == 0)
			{
				throw new InvalidOperationException("Cannot remove item, heap is empty.");
			}
			T v = data[0];
			count--;
			data[0] = data[count];
			data[count] = default(T); //Clears the Last Node
			DownHeap();
			return v;
		}

		/// <summary>
		/// helper function that performs up-heap bubbling
		/// </summary>
		private void UpHeap()
		{
			sorted = false;
			int p = count;
			T item = data[p];
			int par = Parent(p);
			while (par > -1 && item.CompareTo(data[par]) < 0)
			{
				data[p] = data[par]; //Swap nodes
				p = par;
				par = Parent(p);
			}
			data[p] = item;
		}

		/// <summary>
		/// helper function that performs down-heap bubbling
		/// </summary>
		private void DownHeap()
		{
			sorted = false;
			int n;
			int p = 0;
			T item = data[p];
			while (true)
			{
				int ch1 = Child1(p);
				if (ch1 >= count)
					break;
				int ch2 = Child2(p);
				if (ch2 >= count)
				{
					n = ch1;
				}
				else
				{
					n = data[ch1].CompareTo(data[ch2]) < 0 ? ch1 : ch2;
				}
				if (item.CompareTo(data[n]) > 0)
				{
					data[p] = data[n]; //Swap nodes
					p = n;
				}
				else
				{
					break;
				}
			}
			data[p] = item;
		}

		private void EnsureSort()
		{
			if (sorted)
				return;
			Array.Sort(data, 0, count);
			sorted = true;
		}

		/// <summary>
		/// helper function that calculates the parent of a node
		/// </summary>
		private static int Parent(int index)
		{
			return (index - 1) >> 1;
		}

		/// <summary>
		/// helper function that calculates the first child of a node
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private static int Child1(int index)
		{
			return (index << 1) + 1;
		}

		/// <summary>
		/// helper function that calculates the second child of a node
		/// </summary>
		private static int Child2(int index)
		{
			return (index << 1) + 2;
		}

		/// <summary>
		/// Creates a new instance of an identical binary heap.
		/// </summary>
		/// <returns>A BinaryHeap.</returns>
		public BinaryHeap<T> Copy()
		{
			return new BinaryHeap<T>(data, count);
		}

		/// <summary>
		/// Gets an enumerator for the binary heap.
		/// </summary>
		/// <returns>An IEnumerator of type T.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			EnsureSort();
			for (int i = 0; i < count; i++)
			{
				yield return data[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Checks to see if the binary heap contains the specified item.
		/// </summary>
		/// <param name="item">The item to search the binary heap for.</param>
		/// <returns>A boolean, true if binary heap contains item.</returns>
		public bool Contains(T item)
		{
			EnsureSort();
			return Array.BinarySearch<T>(data, 0, count, item) >= 0;
		}

		/// <summary>
		/// Copies the binary heap to an array at the specified index.
		/// </summary>
		/// <param name="array">One dimensional array that is the destination of the copied elements.</param>
		/// <param name="arrayIndex">The zero-based index at which copying begins.</param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			EnsureSort();
			Array.Copy(data, array, count);
		}

		/// <summary>
		/// Gets whether or not the binary heap is readonly.
		/// </summary>
		public bool IsReadOnly
		{
			get { return false; }
		}

		/// <summary>
		/// Removes an item from the binary heap. This utilizes the type T's Comparer and will not remove duplicates.
		/// </summary>
		/// <param name="item">The item to be removed.</param>
		/// <returns>Boolean true if the item was removed.</returns>
		public bool Remove(T item)
		{
			EnsureSort();
			int i = Array.BinarySearch<T>(data, 0, count, item);
			if (i < 0)
				return false;
			Array.Copy(data, i + 1, data, i, count - i);
			data[count] = default(T);
			count--;
			return true;
		}

		#endregion Methods
	}

    public class StableBinaryHeap<T> : BinaryHeap<T> where T : IComparable<T>
    {
        private class TimestampedT : IComparable<TimestampedT>
        {
            public T Value;
            private bool AnyTimestamp;
            private ulong Timestamp;

            public TimestampedT(T value)
            {
                Value = value;
                AnyTimestamp = true;
            }

            public TimestampedT(T value, ulong timestamp)
            {
                Value = value;
                Timestamp = timestamp;
                AnyTimestamp = false;
            }

            public int CompareTo(TimestampedT other)
            {
                int cmpT = Value.CompareTo(other.Value);

                if (cmpT != 0)
                    return cmpT;

                if (AnyTimestamp || other.AnyTimestamp)
                    return 0;
                
                return Timestamp.CompareTo(other.Timestamp);
            }
        }

        private ulong NextTimestamp = 0;

        private BinaryHeap<TimestampedT> Impl = new BinaryHeap<TimestampedT>();

        public new int Count
        {
            get
            {
                return Impl.Count;
            }
        }

        public new bool IsReadOnly
        {
            get
            {
                return Impl.IsReadOnly;
            }
        }

        public new void Add(T item)
        {
            Impl.Add(new TimestampedT(item, NextTimestamp++));
        }

        public new T Peek()
        {
            return Impl.Peek().Value;
        }

        public new T Remove()
        {
            return Impl.Remove().Value;
        }

        public new void Clear()
        {
            Impl.Clear();
        }

        public new bool Contains(T item)
        {
            return Impl.Contains(new TimestampedT(item));
        }

        public new void CopyTo(T[] array, int arrayIndex)
        {
            TimestampedT[] data = new TimestampedT[Count];
            Impl.CopyTo(data, 0);
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = data[i].Value;
            }
        }

        public new IEnumerator<T> GetEnumerator()
        {
            foreach (var element in Impl)
            {
                yield return element.Value;
            }
        }

        public new bool Remove(T item)
        {
            return Impl.Remove(new TimestampedT(item));
        }
    }
}