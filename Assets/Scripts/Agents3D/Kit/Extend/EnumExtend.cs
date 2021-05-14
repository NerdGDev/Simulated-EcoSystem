using System;
using System.Linq;

namespace Kit
{
	/// <summary>
	/// Quick Notes:
	/// ~X flips/inverts all bits in X
	/// X |= Y sets bit(s) Y
	/// X &= ~Y clears bit(s) Y
	/// X & Y == Y, X contain Y
	/// (X & (Y | Z)) == (Y|Z)
	/// (X & (Y | Z)) != 0, X contain Y or Z
	/// </summary>
	public static class EnumExtend
    {
        #region Flags
        public static TEnum GetEnumFromString<TEnum>(string value)
            where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("<T> must be an enumerated type.");
            if (string.IsNullOrEmpty(value))
                return default(TEnum);
            string checker = value.Trim().ToLower();
            return GetValues<TEnum>().FirstOrDefault<TEnum>(o => o.ToString().ToLower().Equals(checker));
        }
        #endregion

        #region lists
        /// <summary>
		/// Field the specified _label and _type.
		/// </summary>
		/// <param name="_label">_label.</param>
		/// <param name="_type">_type.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Field<T>(string _label, T _type) where T : new()
		{
			return Field<T>(_label,_type);
		}
		
		/// <summary>
		/// Gets the enum values.
		/// </summary>
		/// <returns>The enum values by List</returns>
		/// <typeparam name='T'>The 1st type parameter.</typeparam>
		public static T[] GetValues<T>() where T : new() {
		    T valueType = new T();
		    return typeof(T).GetFields().Where(o => o.FieldType == typeof(T)).Select(fieldInfo => (T)fieldInfo.GetValue(valueType)).Distinct().ToArray();
		}
		/// <summary>
		/// Gets the enum names.
		/// </summary>
		/// <returns>The names.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static string[] GetNames<T>() {
		    return typeof (T).GetFields().Where(o => o.FieldType == typeof(T)).Select(info => info.Name).Distinct().ToArray();
        }
        #endregion
    }
}