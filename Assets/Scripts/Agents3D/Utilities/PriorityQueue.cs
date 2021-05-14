using System.Collections.Generic;

namespace FlyAgent.Utilities
{
	/// <summary>A priority queue is a data structure that holds information that has some sort of priority value.
	/// When an item is removed from a priority queue, it's always the item with the highest priority.
	/// Priority queues are used in many important computer algorithms,
	/// in particular graph-based shortest-path algorithms.</summary>
	/// <typeparam name="T"></typeparam>
	/// <see cref="https://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx"/>
	public class PriorityQueue<T> where T : System.IComparable<T>
	{
		private List<T> data;

		public PriorityQueue()
		{
			this.data = new List<T>();
		}

		public void Enqueue(T item)
		{
			data.Add(item);
			int ci = data.Count - 1; // child index; start at end
			while (ci > 0)
			{
				int pi = (ci - 1) / 2; // parent index
				if (data[ci].CompareTo(data[pi]) >= 0)
					break; // child item is larger than (or equal) parent so we're done
				T tmp = data[ci];
				data[ci] = data[pi];
				data[pi] = tmp;
				ci = pi;
			}
		}

		public T Dequeue()
		{
			// assumes pq is not empty; up to calling code
			int li = data.Count - 1; // last index (before removal)
			T frontItem = data[0];   // fetch the front
			data[0] = data[li];
			data.RemoveAt(li);

			--li; // last index (after removal)
			int pi = 0; // parent index. start at front of pq
			while (true)
			{
				int ci = pi * 2 + 1; // left child index of parent
				if (ci > li)
					break;  // no children so done
				int rc = ci + 1;     // right child
				if (rc <= li && data[rc].CompareTo(data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
					ci = rc;
				if (data[pi].CompareTo(data[ci]) <= 0)
					break; // parent is smaller than (or equal to) smallest child so done
				T tmp = data[pi];
				data[pi] = data[ci];
				data[ci] = tmp; // swap parent and child
				pi = ci;
			}
			return frontItem;
		}

		public T Peek()
		{
			T frontItem = data[0];
			return frontItem;
		}

		public int Count()
		{
			return data.Count;
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendFormat("{0}<{1}> count = {2}", GetType().Name, typeof(T).Name, data.Count);
			for (int i = 0; i < data.Count; ++i)
			{
				sb.AppendFormat("\n\r{0}", data[i].ToString());
			}
			return sb.ToString();
		}

		public bool IsConsistent()
		{
			// is the heap property true for all data?
			if (data.Count == 0)
				return true;
			int li = data.Count - 1; // last index
			for (int pi = 0; pi < data.Count; ++pi) // each parent index
			{
				int lci = 2 * pi + 1; // left child index
				int rci = 2 * pi + 2; // right child index

				if (lci <= li && data[pi].CompareTo(data[lci]) > 0)
					return false; // if lc exists and it's greater than parent then bad.
				if (rci <= li && data[pi].CompareTo(data[rci]) > 0)
					return false; // check the right child too.
			}
			return true; // passed all checks
		} // IsConsistent
	} // PriorityQueue

	/// <summary>
	/// Based on http://blogs.msdn.com/b/ericlippert/archive/2007/10/08/path-finding-using-a-in-c-3-0-part-three.aspx
	/// Backported to C# 2.0
	/// </summary>
	public class PriorityQueue<P, V>
	{
		private SortedDictionary<P, LinkedList<V>> list = new SortedDictionary<P, LinkedList<V>>();

		public void Enqueue(V value, P priority)
		{
			LinkedList<V> q;
			if (!list.TryGetValue(priority, out q))
			{
				q = new LinkedList<V>();
				list.Add(priority, q);
			}
			q.AddLast(value);
		}

		public V Dequeue()
		{
			// will throw exception if there isn’t any first element!
			SortedDictionary<P, LinkedList<V>>.KeyCollection.Enumerator enume = list.Keys.GetEnumerator();
			enume.MoveNext();
			P key = enume.Current;
			LinkedList<V> v = list[key];
			V res = v.First.Value;
			v.RemoveFirst();
			if (v.Count == 0)
			{ // nothing left of the top priority.
				list.Remove(key);
			}
			return res;
		}
		
		public void Replace(V value, P oldPriority, P newPriority)
		{
			LinkedList<V> v = list[oldPriority];
			v.Remove(value);

			if (v.Count == 0)
			{ // nothing left of the top priority.
				list.Remove(oldPriority);
			}

			Enqueue(value, newPriority);
		}

		public bool IsEmpty
		{
			get { return list.Count == 0; }
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (P key in list.Keys)
			{
				foreach (V val in list[key])
				{
					sb.AppendFormat("{0}, ", val);
				}
			}
			sb.Insert(0, string.Format("{0}<{1}> count = {2}", GetType().Name, typeof(P).Name, sb.Length));
			return sb.ToString();
		}
	}
}