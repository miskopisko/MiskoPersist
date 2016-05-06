using System;
using System.Reflection;
using System.Runtime.Serialization;
using MiskoPersist.Data;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Serialization
{
	internal class MiskoConverter : IFormatterConverter
	{
		public Object Convert(Object value, TypeCode typeCode)
		{
			throw new NotImplementedException();
		}

		public Object Convert(Object value, Type type)
		{
			if (value != null && value.GetType().Equals(typeof(String)) && !String.IsNullOrEmpty((String)value))
			{
				if (type.Equals(typeof(String)))
				{
					return (String)value;
				}
				if (type.Equals(typeof(Boolean)) || type.Equals(typeof(Boolean?)))
				{
					return ToBoolean(value);
				}
				if (type.Equals(typeof(DateTime)) || type.Equals(typeof(DateTime?)))
				{
					return ToDateTime(value);
				}
				if (type.Equals(typeof(Decimal)) || type.Equals(typeof(Decimal?)))
				{
					return ToDecimal(value);
				}
				if (type.Equals(typeof(Double)) || type.Equals(typeof(Double?)))
				{
					return ToDouble(value);
				}
				if (type.Equals(typeof(Int16)) || type.Equals(typeof(Int16?)))
				{
					return ToInt16(value);
				}
				if (type.Equals(typeof(Int32)) || type.Equals(typeof(Int32?)))
				{
					return ToInt32(value);
				}
				if (type.Equals(typeof(Int64)) || type.Equals(typeof(Int64?)))
				{
					return ToInt64(value);
				}
				if (type.Equals(typeof(Money)))
				{
					return ToMoney(value);
				}
				if (type.Equals(typeof(PrimaryKey)))
				{
					return ToPrimaryKey(value);
				}
				if (typeof(MiskoEnum).IsAssignableFrom(type))
				{
					return ToMiskoEnum(type, value);
				}
			}
			return null;
		}

		public Boolean ToBoolean(Object value)
		{
			return Boolean.Parse((String)value);
		}

		public Byte ToByte(Object value)
		{
			throw new NotImplementedException();
		}

		public Char ToChar(Object value)
		{
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(Object value)
		{
			return DateTime.ParseExact((String)value, Serializer.DATEFORMAT, null);
		}

		public Decimal ToDecimal(Object value)
		{
			return Decimal.Parse((String)value);
		}

		public Double ToDouble(Object value)
		{
			return Double.Parse((String)value);
		}

		public Int16 ToInt16(Object value)
		{
			return Int16.Parse((String)value);
		}

		public Int32 ToInt32(Object value)
		{
			return Int32.Parse((String)value);
		}

		public Int64 ToInt64(Object value)
		{
			return Int64.Parse((String)value);
		}

		public SByte ToSByte(Object value)
		{
			throw new NotImplementedException();
		}

		public Single ToSingle(Object value)
		{
			throw new NotImplementedException();
		}

		public String ToString(Object value)
		{
			return value.ToString();
		}

		public UInt16 ToUInt16(Object value)
		{
			throw new NotImplementedException();
		}

		public UInt32 ToUInt32(Object value)
		{
			throw new NotImplementedException();
		}

		public UInt64 ToUInt64(Object value)
		{
			throw new NotImplementedException();
		}

		public Money ToMoney(Object value)
		{
			return new Money((String)value);
		}

		public MiskoEnum ToMiskoEnum(Type enumType, Object value)
		{
			return (MiskoEnum)enumType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { ToInt64(value) });
		}

		public PrimaryKey ToPrimaryKey(Object value)
		{
			return new PrimaryKey((String)value);
		}

		public Boolean CanConvert(Type type)
		{
			return 	type.IsPrimitive ||
				type.IsEnum ||
				typeof(String).IsAssignableFrom(type) ||
				typeof(DateTime).IsAssignableFrom(type) ||
				typeof(Money).IsAssignableFrom(type) ||
				typeof(PrimaryKey).IsAssignableFrom(type) ||
				typeof(MiskoEnum).IsAssignableFrom(type) ||
				(type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) && CanConvert(Nullable.GetUnderlyingType(type)));
		}
	}
}
