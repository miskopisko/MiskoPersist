using System;
using Newtonsoft.Json;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
	[JsonConverter(typeof(PrimaryKeySerializer))]
	public struct PrimaryKey : IComparable<PrimaryKey>, IComparable<Int64>, IEquatable<PrimaryKey>, IEquatable<Int64>
	{
		private static Logger Log = Logger.GetInstance(typeof(PrimaryKey));

		#region Fields

		private readonly Int64 mValue_;
		
		public static readonly PrimaryKey ZERO = new PrimaryKey(0);

		#endregion

		#region Properties

		public Int64 Value 
		{ 
			get
			{
				return mValue_;
			} 
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
			mValue_ = value;
		}

		public PrimaryKey(String s)
		{
			mValue_ = !String.IsNullOrEmpty(s) ? Convert.ToInt64(s) : 0;
		}

		#endregion

		#region Operators

		public static PrimaryKey operator +(PrimaryKey left, Int64 right)
		{
			return new PrimaryKey(left.Value + right);
		}

		public static PrimaryKey operator -(PrimaryKey value)
		{
			return new PrimaryKey(-value.Value);
		}

		public static Boolean operator ==(PrimaryKey left, Int64 right)
		{
			return left.Value.Equals(right);
		}

		public static Boolean operator !=(PrimaryKey left, Int64 right)
		{
			return !(left == right);
		}

		public static Boolean operator ==(PrimaryKey left, PrimaryKey right)
		{
			return left.Value.Equals(right.Value);
		}

		public static Boolean operator !=(PrimaryKey left, PrimaryKey right)
		{
			return !(left == right);
		}

		public static Boolean operator >(PrimaryKey left, PrimaryKey right)
		{
			return left.CompareTo(right.Value) > 0;
		}

		public static Boolean operator >(PrimaryKey left, Int64 right)
		{
			return left.CompareTo(right) > 0;
		}

		public static Boolean operator <(PrimaryKey left, PrimaryKey right)
		{
			return left.CompareTo(right.Value) < 0;
		}

		public static Boolean operator <(PrimaryKey left, Int64 right)
		{
			return left.CompareTo(right) < 0;
		}

		public static Boolean operator >=(PrimaryKey left, PrimaryKey right)
		{
			return left.CompareTo(right.Value) >= 0;
		}

		public static Boolean operator >=(PrimaryKey left, Int64 right)
		{
			return left.CompareTo(right) >= 0;
		}

		public static Boolean operator <=(PrimaryKey left, PrimaryKey right)
		{
			return left.CompareTo(right.Value) <= 0;
		}

		public static Boolean operator <=(PrimaryKey left, Int64 right)
		{
			return left.CompareTo(right) <= 0;
		}

		#endregion

		#region Override Methods

		public override Boolean Equals(Object obj)
		{
			if (obj is PrimaryKey)
			{
				return Equals((PrimaryKey)obj);
			}
			else if (obj is Int64)
			{
				return Equals((Int64)obj);
			}

			return false;
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

		#region IComparable<long> Members

		public Int32 CompareTo(Int64 other)
		{
			return Value.CompareTo(other);
		}

		#endregion

		#region IEquatable<PrimaryKey> Members

		public Boolean Equals(PrimaryKey other)
		{
			return Value == other.Value;
		}

		#endregion

		#region IEquatable<long> Members

		public Boolean Equals(Int64 other)
		{
			if (Object.Equals(other, null))
			{
				return false;
			}

			return Value == other;
		}

		#endregion
	}
	
	internal sealed class PrimaryKeySerializer : JsonConverter
	{
		#region implemented abstract members of JsonConverter
		
		public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
		{
			writer.WriteValue(((PrimaryKey)value).Value);
		}
		
		public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
		{
			return reader.Value != null ? new PrimaryKey((Int64)reader.Value) : new PrimaryKey(0);
		}
		
		public override Boolean CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}
		
		#endregion    	
	}
}
