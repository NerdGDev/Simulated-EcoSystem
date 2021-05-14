// #define _PATHDEBUGEVENT_
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using FlyAgent.Utilities;

namespace FlyAgent.Navigation
{
	public interface IPFVertex<IPF> where IPF : IPFVertex<IPF>, IEquatable<IPF>
	{
		bool LineOfSight(IPF other);
		float Cost(IPF other, float sizeRef);
		IEnumerable<IPF> Neighbours();
	}

	public static class LazyThetaStar<IPF> where IPF : IPFVertex<IPF>, IEquatable<IPF>
	{
		public class PathFinder : IDisposable
		{
			public int maxIterations = 200;
			public float heuristicWeight = 1.5f;

			struct PathPackage : IComparable<PathPackage>
			{
				public PathPackage(IPF val, float gph)
				{
					value = val;
                    this.gph = gph;
				}
				public IPF value;
				public float gph;

				public int CompareTo(PathPackage other)
				{
					if (gph < other.gph)
					{
						return -1;
					}
					if (gph > other.gph)
						return 1;
					return 0;
				}
			}
			
			PriorityQueue<PathPackage> open = new PriorityQueue<PathPackage>();
			HashSet<IPF> close = new HashSet<IPF>();
			Dictionary<IPF,IPF> parent = new Dictionary<IPF, IPF>();
			Dictionary<IPF,float> g = new Dictionary<IPF, float>();
			public IPF start { get; private set; }
			public IPF end { get; private set; }
			int iteration = 0;
			private Task m_Task = null;
			private float m_SizeRef;
			public PathFinder(IPF start, IPF end, float sizeRef, float heuristicWeight = 1.0f, int maxIterations = 200)
			{
				this.start = start;
				this.end = end;
				this.heuristicWeight = heuristicWeight;
				this.maxIterations = maxIterations;
				m_SizeRef = sizeRef;
				parent[start] = start;
				g[start] = 0;
				open.Enqueue(new PathPackage(start, g[start] + this.heuristicWeight * start.Cost(end, sizeRef)));
			}
			public event Action<IPF> DebugOnExpanded;
			//the code here is basicly a translation of psuedo-code from http://aigamedev.com/wp-content/blogs.dir/5/files/2013/07/fig53-full.png
			private bool InternalIterate()
			{
				while (open.Count() != 0)
				{
					var s = open.Dequeue().value;
#if _PATHDEBUGEVENT_
					if (DebugOnExpanded != null)
						DebugOnExpanded(s);
#endif
					if (close.Contains(s))
						continue;
					SetVertex(s);
					close.Add(s);
					if (iteration++ > maxIterations || s.Equals(end))
					{
						return true;
					}
					foreach (var sp in s.Neighbours())
					{
						if (!close.Contains(sp))
						{
							if (!g.ContainsKey(sp))
							{
								g[sp] = Mathf.Infinity;
								parent.Remove(sp);
							}
							UpdateVertex(s, sp);
						}
					}
				}
				return false;
			}

			public IEnumerable QuickFind()
			{
				while (!InternalIterate())
					;

				List<IPF> res = new List<IPF>(100);
				IPF temp = close
					.OrderBy(v => v.Cost(end, m_SizeRef))
					.FirstOrDefault();
				if (temp == null)
					return null;
				while (!parent[temp].Equals(temp))
				{
					res.Add(temp);
					temp = parent[temp];
				}
				res.Reverse();
				return res;
			}

			public void AsyncFind(Action<List<IPF>> callback)
			{
				m_Task = _AsyncFind(callback);
			}

			private async Task _AsyncFind(Action<List<IPF>> callback)
			{
				// 99% same as QuickFind() + InternalIterate;
				while (open.Count() != 0)
				{
					var s = open.Dequeue().value;
					if (close.Contains(s))
						continue;
					SetVertex(s);
					close.Add(s);
					if (iteration++ > maxIterations || s.Equals(end))
					{
						break;
					}
					foreach (var sp in s.Neighbours())
					{
						if (iteration % 10 == 0)
							await Task.Delay(1); // since Delay(0)/(null) will result in dead loop.

						if (!close.Contains(sp))
						{
							if (!g.ContainsKey(sp))
							{
								g[sp] = Mathf.Infinity;
								parent.Remove(sp);
							}
							UpdateVertex(s, sp);
						}
					}
				}

				List<IPF> res = new List<IPF>(100);
				IPF temp = close
					.OrderBy(v => v.Cost(end, m_SizeRef))
					.FirstOrDefault();
				if (temp == null)
				{
					callback(res);
					return;
				}
				while (!parent[temp].Equals(temp))
				{
					res.Add(temp);
					temp = parent[temp];
				}
				res.Reverse();
				callback(res);
			}

			void UpdateVertex(IPF s, IPF sp)
			{
				var gold = g[sp];
				ComputeCost(s, sp);
				if (g[sp] < gold)
				{
					open.Enqueue(new PathPackage(sp, g[sp] + heuristicWeight * sp.Cost(end, m_SizeRef)));
				}
			}
			void ComputeCost(IPF s, IPF sp)
			{
				if (g[parent[s]] + parent[s].Cost(sp, m_SizeRef) < g[sp])
				{
					parent[sp] = parent[s];
					g[sp] = g[parent[s]] + parent[s].Cost(sp, m_SizeRef);
				}
			}
			void SetVertex(IPF s)
			{
				if (!parent[s].LineOfSight(s))
				{
					var temp = s
						.Neighbours()
						.Intersect(close)
						.Select(sp => new { sp = sp, gpc = g[sp] + sp.Cost(s, m_SizeRef) })
						.OrderBy(sppair => sppair.gpc)
						.FirstOrDefault();
					if (temp == null)
						return;
					parent[s] = temp.sp;
					g[s] = temp.gpc;
					;
				}
			}

			#region IDisposable Support
			private bool IsDisposed = false; // To detect redundant calls

			protected virtual void Dispose(bool disposing)
			{
				if (!IsDisposed)
				{
					if (disposing)
					{
						g.Clear();
						parent.Clear();
						close.Clear();
						// TODO: dispose managed state (managed objects).
					}

					// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
					// TODO: set large fields to null.
					if (m_Task != null)
						m_Task.Dispose();
					// m_Task = null;
					
					IsDisposed = true;
				}
			}

			// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
			~PathFinder()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose(false);
			}

			// This code added to correctly implement the disposable pattern.
			public void Dispose()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose(true);
				// TODO: uncomment the following line if the finalizer is overridden above.
				// GC.SuppressFinalize(this);
			}
			#endregion
		}

		public static PathFinder FindPath(IPF start, IPF end, float sizeRef)
		{
			return new PathFinder(start, end, sizeRef);
		}
	}
}