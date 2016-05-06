using System;
using log4net;

namespace MiskoPersist.Data
{
	public class PrimaryKey : IComparable<PrimaryKey>, IEquatable<PrimaryKey>
	{
		private static ILog Log = LogManager.GetLogger(typeof(PrimaryKey));

		#region Fields
		
		

		#endregion

		#region Properties

		public Int64 Value
		{
			get;
			set;
		}
		
		public Boolean IsSet
		{
			get
			{
				return Value != 0;
			}
		}

		public Boolean IsNotSet
		{
			get
			{
				return !IsSet;
			}
		}

		#endregion

		#region Constructors

		public PrimaryKey(Int64 value)
		{
			Value = value;
		}

		public PrimaryKey(String s)
		{
			Value = !String.IsNullOrEmpty(s) ? Convert.ToInt64(s) : 0;
		}

		#endregion

		#region Operators

		public static PrimaryKey operator -(PrimaryKey value)
		{
			value = value ?? 0;
			return new PrimaryKey(-value.Value);
		}

		public static Boolean operator ==(PrimaryKey left, PrimaryKey right)
		{
			left = left ?? 0;
			right = right ?? 0;
			return left.Equals(right);
		}

		public static Boolean operator !=(PrimaryKey left, PrimaryKey right)
		{
			return !(left == right);
		}

		public static Boolean operator >(PrimaryKey left, PrimaryKey right)
		{
			left = left ?? 0;
			right = right ?? 0;
			return left.CompareTo(right) > 0;
		}

		public static Boolean operator <(PrimaryKey left, PrimaryKey right)
		{
			left = left ?? 0;
			right = right ?? 0;
			return left.CompareTo(right) < 0;
		}

		public static Boolean operator >=(PrimaryKey left, PrimaryKey right)
		{
			left = left ?? 0;
			right = right ?? 0;
			return left.CompareTo(right) >= 0;
		}

		public static Boolean operator <=(PrimaryKey left, PrimaryKey right)
		{
			left = left ?? 0;
			right = right ?? 0;
			return left.CompareTo(right) <= 0;
		}

		public static implicit operator PrimaryKey(Int64 value)
		{
			return new PrimaryKey(value);
		}

		#endregion

		#region Override Methods

		public override Boolean Equals(Object obj)
		{
			PrimaryKey value = obj as PrimaryKey;
			return value != null && Equals(value);
		}

		public override Int32 GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override String ToString()
		{
			return Value.ToString();
		}

		#endregion

		#region IComparable<PrimaryKey> Members

		public Int32 CompareTo(PrimaryKey other)
		{
			return Value.CompareTo(other.Value);
		}

		#endregion

		#region IEquatable<PrimaryKey> Members

		public Boolean Equals(PrimaryKey other)
		{
			return Value.Equals(other.Value);
		}

		#endregion
	}
}
