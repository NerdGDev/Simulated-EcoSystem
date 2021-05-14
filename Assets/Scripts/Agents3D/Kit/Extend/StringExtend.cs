using UnityEngine;
using System.Collections.Generic;

namespace Kit
{
	public static class StringExtend
	{
		/// <summary>Splits the string[] of the string by length.</summary>
		/// <returns>Array of string</returns>
		/// <param name="string">string</param>
		/// <param name="length">length.</param>
		public static string[] SplitByLength(this string _string, int _length)
		{
			int _strLength = _string.Length;
			int _strCount = (_strLength + _length - 1) / _length;
			string[] _rst = new string[_strCount];
			for (int i = 0; i < _strCount; ++i)
			{
				_rst[i] = _string.Substring(i * _length, Mathf.Min(_length, _strLength));
				_strLength -= _length;
			}
			return _rst;
		}

		/// <summary>Computes the Levenshtein Edit Distance between two enumerables.</summary>
		/// <param name="lhs">The first enumerable.</param>
		/// <param name="rhs">The second enumerable.</param>
		/// <returns>The edit distance.</returns>
		/// <see cref="https://en.wikipedia.org/wiki/Levenshtein_distance"/>
		public static int LevenshteinDistance(string lhs, string rhs, bool caseSensitive = true)
		{
			if (!caseSensitive)
			{
				lhs = lhs.ToLower();
				rhs = rhs.ToLower();
			}
			char[] first = lhs.ToCharArray();
			char[] second = rhs.ToCharArray();
			return LevenshteinDistance<char>(first, second);
		}
		
		/// <summary>Computes the Levenshtein Edit Distance between two enumerables.</summary>
		/// <typeparam name="T">The type of the items in the enumerables.</typeparam>
		/// <param name="lhs">The first enumerable.</param>
		/// <param name="rhs">The second enumerable.</param>
		/// <returns>The edit distance.</returns>
		/// <see cref="https://blogs.msdn.microsoft.com/toub/2006/05/05/generic-levenshtein-edit-distance-with-c/"/>
		public static int LevenshteinDistance<T>(IEnumerable<T> lhs, IEnumerable<T> rhs) where T : System.IEquatable<T>
		{
			// Validate parameters
			if (lhs == null) throw new System.ArgumentNullException("lhs");
			if (rhs == null) throw new System.ArgumentNullException("rhs");

			// Convert the parameters into IList instances
			// in order to obtain indexing capabilities
			IList<T> first = lhs as IList<T> ?? new List<T>(lhs);
			IList<T> second = rhs as IList<T> ?? new List<T>(rhs);

			// Get the length of both.  If either is 0, return
			// the length of the other, since that number of insertions
			// would be required.
			int n = first.Count, m = second.Count;
			if (n == 0) return m;
			if (m == 0) return n;

			// Rather than maintain an entire matrix (which would require O(n*m) space),
			// just store the current row and the next row, each of which has a length m+1,
			// so just O(m) space. Initialize the current row.
			int curRow = 0, nextRow = 1;

			int[][] rows = new int[][] { new int[m + 1], new int[m + 1] };
			for (int j = 0; j <= m; ++j)
				rows[curRow][j] = j;

			// For each virtual row (since we only have physical storage for two)
			for (int i = 1; i <= n; ++i)
			{
				// Fill in the values in the row
				rows[nextRow][0] = i;

				for (int j = 1; j <= m; ++j)
				{
					int dist1 = rows[curRow][j] + 1;
					int dist2 = rows[nextRow][j - 1] + 1;
					int dist3 = rows[curRow][j - 1] +
						(first[i - 1].Equals(second[j - 1]) ? 0 : 1);

					rows[nextRow][j] = System.Math.Min(dist1, System.Math.Min(dist2, dist3));
				}

				// Swap the current and next rows
				if (curRow == 0)
				{
					curRow = 1;
					nextRow = 0;
				}
				else
				{
					curRow = 0;
					nextRow = 1;
				}
			}

			// Return the computed edit distance
			return rows[curRow][m];
		}
	}
}