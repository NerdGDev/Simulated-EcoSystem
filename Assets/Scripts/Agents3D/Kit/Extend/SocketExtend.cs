using UnityEngine;
using System.Net;

namespace Kit
{
	public static class SocketExtend
	{
		public static IPEndPoint TryParseToEndPoint(this string str)
		{
			if (str == null || str.Length == 0)
				return null;
			string[] tmp = str.Split (':');
			if (tmp.Length != 2)
				return null;
			IPAddress address = IPAddress.Parse (tmp [0]);
			int port = int.Parse (tmp [1]);
			return new IPEndPoint (address, port);
		}
	}
}