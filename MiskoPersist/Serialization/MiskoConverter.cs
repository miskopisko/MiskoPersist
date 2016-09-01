using System;
using log4net;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Serialization
{
	public static class MiskoConverter
	{
		private static ILog Log = LogManager.GetLogger(typeof(MiskoConverter));
		
		#region Public Methods
		
		public static Object Convert(String value, Type type)
		{
			if (String.IsNullOrEmpty(value))
			{
				return null;
			}
			if (type.Equals(typeof(String)))
			{
				return (String)value;
			}
			if (type.Equals(typeof(Boolean)) || type.Equals(typeof(Boolean?)))
			{
				return Boolean.Parse((String)value);
			}
			if (type.Equals(typeof(DateTime)) || type.Equals(typeof(DateTime?)))
			{
				return DateTime.ParseExact((String)value, Serializer.DateFormat, null);
			}
			if (type.Equals(typeof(Decimal)) || type.Equals(typeof(Decimal?)))
			{
				return Decimal.Parse((String)value);
			}
			if (type.Equals(typeof(Double)) || type.Equals(typeof(Double?)))
			{
				return Double.Parse((String)value);
			}
			if (type.Equals(typeof(Int16)) || type.Equals(typeof(Int16?)))
			{
				return Int16.Parse((String)value);
			}
			if (type.Equals(typeof(Int32)) || type.Equals(typeof(Int32?)))
			{
				return Int32.Parse((String)value);
			}
			if (type.Equals(typeof(Int64)) || type.Equals(typeof(Int64?)))
			{
				return Int64.Parse((String)value);
			}
			if (type.Equals(typeof(Money)))
			{
				return new Money((String)value); 
			}
			if (type.Equals(typeof(PrimaryKey)))
			{
				return new PrimaryKey((String)value);
			}
			if (typeof(MiskoEnum).IsAssignableFrom(type))
			{
				return (MiskoEnum)typeof(MiskoEnum).GetMethod("Parse", new Type[] { typeof(Int64)}).MakeGenericMethod(type).Invoke(null, new Object[] { Int64.Parse((String)value) });
			}
			throw new MiskoException(String.Format("Unknown type to convert : {0}", type.Name));
		}
		
		public static Boolean CanConvert(Type type)
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
		
		#endregion
	}
}
