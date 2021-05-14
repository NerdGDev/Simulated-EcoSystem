using System.Collections.Generic;
using System.Collections.ObjectModel;
/// <summary>Custom Enum</summary>
/// <remarks><see cref="https://www.codeproject.com/Articles/20805/Enhancing-C-Enums"/> </remarks>
/// <typeparam name="T"></typeparam>
public abstract class EnumBaseType<T> : EnumBaseStructure where T : EnumBaseType<T>
{
	protected static List<T> enumValues = new List<T>();
	public readonly int Key;
	public readonly string Value;

	/// <summary>Constructor, you need to re-define this in sub-class</summary>
	/// <example>
	/// public YOUR_CLASS(int key, string value) : base(key, value) { }
	/// </example>
	/// <param name="key"></param>
	/// <param name="value"></param>
	public EnumBaseType(int key, string value)
	{
		Key = key;
		Value = value;
		enumValues.Add((T)this);
	}

	/// <summary>Use to create your own GetValues() methods</summary>
	/// <remarks>add method in sub-class</remarks>
	/// <example>
	/// public static ReadOnlyCollection<YourEnumClass> GetValues()
	/// { return GetBaseValues(); }
	/// </example>
	protected static ReadOnlyCollection<T> GetBaseValues()
	{
		return enumValues.AsReadOnly();
	}

	/// <summary>Use to create your own GetByKey() methods</summary>
	/// <remarks>add method in sub-class</remarks>
	/// <example>
	/// public static YourEnumClass GetByKey(int key)
	/// { return GetBaseByKey(key); }
	/// </example>
	protected static T GetBaseByKey(int key)
	{
		foreach (T t in enumValues)
		{
			if (t.Key == key) return t;
		}
		return null;
	}
	
	public override string ToString()
	{
		return Value;
	}
}

public abstract class EnumBaseStructure { }