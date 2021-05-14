// For templating using integers--for example, to define a class
// that has a predefined length.

namespace Kit
{
	public abstract class Integer
	{
		abstract public int Get { get; }
	}

	public class One : Integer
	{
		public override int Get { get { return 1; } }
	}

	public class Two : Integer
	{
		public override int Get { get { return 2; } }
	}

	public class Three : Integer
	{
		public override int Get { get { return 3; } }
	}

	public class Four : Integer
	{
		public override int Get { get { return 4; } }
	}

	public class Five : Integer
	{
		public override int Get { get { return 5; } }
	}

	public class Six : Integer
	{
		public override int Get { get { return 6; } }
	}

	public abstract class Boolean
	{
		abstract public bool Value { get; }
	}

	public class True : Boolean
	{
		public override bool Value { get { return true; } }
	}

	public class False : Boolean
	{
		public override bool Value { get { return false; } }
	}

	public delegate void Callback();

	public delegate void Callback<T0>(T0 t0);

	public delegate void Callback<T0, T1>(T0 t0, T1 t1);

	public delegate void Callback<T0, T1, T2>(T0 t0, T1 t1, T2 t2);
}