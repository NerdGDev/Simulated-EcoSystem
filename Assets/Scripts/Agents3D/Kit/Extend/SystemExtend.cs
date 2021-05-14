// using UnityEngine;
using System;
using System.Reflection;
namespace Kit
{
    public static class SystemExtend
    {
        #region DebugFunctions
        /// <summary>Gets the methods of an object.</summary>
        /// <returns>A list of methods accessible from this object.</returns>
        /// <param name='obj'>The object to get the methods of.</param>
        /// <param name='includeInfo'>Whether or not to include each method's method info in the list.</param>
        public static string MethodsOfObject(this Object obj, bool includeInfo = false)
        {
            string methods = string.Empty;
            MethodInfo[] methodInfos = obj.GetType().GetMethods();
            for (int i = 0; i < methodInfos.Length; i++)
            {
                if (includeInfo)
                {
                    methods += methodInfos[i] + "\n";
                }
                else
                {
                    methods += methodInfos[i].Name + "\n";
                }
            }
            return (methods);
        }

        /// <summary>Gets the methods of a type.</summary>
        /// <returns>A list of methods accessible from this type.</returns>
        /// <param name='type'>The type to get the methods of.</param>
        /// <param name='includeInfo'>Whether or not to include each method's method info in the list.</param>
        public static string MethodsOfType(this Type type, bool includeInfo = false)
        {
            string methods = string.Empty;
            MethodInfo[] methodInfos = type.GetMethods();
            for (var i = 0; i < methodInfos.Length; i++)
            {
                if (includeInfo)
                {
                    methods += methodInfos[i] + "\n";
                }
                else
                {
                    methods += methodInfos[i].Name + "\n";
                }
            }
           return (methods);
        }

		/// <summary>Use reflection to invoke function by Name</summary>
		/// <param name="obj">This object</param>
		/// <param name="functionName">function name in string</param>
		/// <param name="bindingFlags"><see cref="BindingFlags"/></param>
		/// <param name="args">The values you wanted to pass, will trim out if destination params less than provider.</param>
		/// <returns></returns>
		public static bool InvokeMethod(this object obj, string functionName, BindingFlags bindingFlags, params object[] args)
		{
			Type type = obj.GetType();
			MethodInfo method = type.GetMethod(functionName, bindingFlags);
			if (method != null)
			{
				int length = method.GetParameters().Length;
				if (length > args.Length)
				{
					throw new ArgumentOutOfRangeException("Destination parameter(s) are required " + length + ", but system provided " + args.Length);
				}
				else
				{
					object[] trimArgs = new object[length];
					Array.Copy(args, trimArgs, length);
					method.Invoke(obj, trimArgs);
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}