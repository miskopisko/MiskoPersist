using System;
using System.Xml;
using MiskoPersist.Core;
using Newtonsoft.Json;

namespace MiskoPersist.Data
{
	[JsonConverter(typeof(PrimaryKeySerializer))]
	public class PrimaryKey : AbstractViewedData, IComparable<PrimaryKey>, IComparable<Int64>, IEquatable<PrimaryKey>, IEquatable<Int64>
	{
		private static Logger Log = Logger.GetInstance(typeof(PrimaryKey));

		#region Fields


		#endregion

		#region Properties

		public Int64 Value 
		{ 
			get; 
			set; 
		}
		
		public new Boolean IsSet 
		{ 
			get 
			{ 
				return Value != 0; 
			} 
		}

		#endregion

		#region Constructors

		public PrimaryKey()
		{
			Value = 0;
		}

		public PrimaryKey(Int64 value)
		{
			Value = value;
		}

		public PrimaryKey(String s)
		{
			if (!String.IsNullOrEmpty(s))
			{
				Value = Convert.ToInt64(s);
			}
			else
			{
				Value = 0;
			}
		}

		#endregion

		#region Operators

		public static PrimaryKey operator +(PrimaryKey left, Int64 right)
		{
			if (Object.ReferenceEquals(left, null)) return new PrimaryKey() + right;
			return new PrimaryKey(left.Value + right);
		}

		public static PrimaryKey operator -(PrimaryKey value)
		{
			return new PrimaryKey(-value.Value);
		}

		public static Boolean operator ==(PrimaryKey left, Int64 right)
		{
			if (Object.ReferenceEquals(left, null) && Object.Equals(right, null)) return true;
			else if (Object.ReferenceEquals(left, null) && !Object.Equals(right, null)) return false;
			else if (!Object.ReferenceEquals(left, null) && Object.Equals(right, null)) return false;
			return left.Value.Equals(right);
		}

		public static Boolean operator !=(PrimaryKey left, Int64 right)
		{
			return !(left == right);
		}

		public static Boolean operator ==(PrimaryKey left, PrimaryKey right)
		{
			if (Object.ReferenceEquals(left, null) && Object.ReferenceEquals(right, null)) return true;
			else if (Object.ReferenceEquals(left, null) && !Object.ReferenceEquals(right, null)) return false;
			else if (!Object.ReferenceEquals(left, null) && Object.ReferenceEquals(right, null)) return false;
			return left.Value.Equals(right.Value);
		}

		public static Boolean operator !=(PrimaryKey left, PrimaryKey right)
		{
			return !(left == right);
		}

		public static Boolean operator >(PrimaryKey left, PrimaryKey right)
		{
			if (Object.ReferenceEquals(left, null))
			{
				left = new PrimaryKey(0);
			}

			return left.CompareTo(right.Value) > 0;
		}

		public static Boolean operator >(PrimaryKey left, Int64 right)
		{
			if (Object.ReferenceEquals(left, null))
			{
				left = new PrimaryKey(0);
			}

			return left.CompareTo(right) > 0;
		}

		public static Boolean operator <(PrimaryKey left, PrimaryKey right)
		{
			if (Object.ReferenceEquals(left, null))
			{
				left = new PrimaryKey(0);
			}

			return left.CompareTo(right.Value) < 0;
		}

		public static Boolean operator <(PrimaryKey left, Int64 right)
		{
			if (Object.ReferenceEquals(left, null))
			{
				left = new PrimaryKey(0);
			}

			return left.CompareTo(right) < 0;
		}

		public static Boolean operator >=(PrimaryKey left, PrimaryKey right)
		{
			if (Object.ReferenceEquals(left, null))
			{
				left = new PrimaryKey(0);
			}

			return left.CompareTo(right.Value) >= 0;
		}

		public static Boolean operator >=(PrimaryKey left, Int64 right)
		{
			if (Object.ReferenceEquals(left, null))
			{
				left = new PrimaryKey(0);
			}

			return left.CompareTo(right) >= 0;
		}

		public static Boolean operator <=(PrimaryKey left, PrimaryKey right)
		{
			if (Object.ReferenceEquals(left, null))
			{
				left = new PrimaryKey(0);
			}

			return left.CompareTo(right.Value) <= 0;
		}

		public static Boolean operator <=(PrimaryKey left, Int64 right)
		{
			if (Object.ReferenceEquals(left, null))
			{
				left = new PrimaryKey(0);
			}

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
			if (Object.ReferenceEquals(other, null))
			{
				return false;
			}

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

		#region XmlSerialization

		public override void ReadXml(XmlReader reader)
		{
			Value = Int64.Parse(XML.InnerText);
		}

		public override void WriteXml(XmlWriter writer)
		{
			writer.WriteValue(Value);
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
			return reader.Value != null ? new PrimaryKey((Int64)reader.Value) : null;
		}
		
		public override Boolean CanConvert(Type objectType)
		{
			throw new NotImplementedException();
		}
		
		#endregion    	
	}
}
