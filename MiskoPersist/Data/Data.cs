using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Data.Stored;
using MiskoPersist.Data.Viewed;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Data
{
	public class Data
	{
		private static ILog Log = LogManager.GetLogger(typeof(Data));
		
		#region Fields

		

		#endregion

		#region Other Properties	
		
		public virtual Boolean IsSet
		{
			get;
			private set;
		}

		#endregion

		#region Constructors
		
		public Data()
		{
		}

		public Data(Session session, Persistence persistence)
		{
			Set(session, persistence);
		}
		
		#endregion

		#region Override Methods

		public override String ToString()
		{
			String result = GetType().Name + Environment.NewLine;

			foreach (PropertyInfo property in GetType().GetProperties())
			{
				result += property.Name + ": " + property.GetValue(this, null) + Environment.NewLine;
			}

			return result;
		}
		
		public override Int32 GetHashCode()
		{
			Int32 hashCode = 0;
			unchecked
			{
				foreach (PropertyInfo property in GetType().GetProperties())
				{
					Object value = property.GetValue(this);
					if (value != null)
					{
						hashCode += 515153 * value.GetHashCode();
					}
				}
			}
			return hashCode;
		}
		
		public override Boolean Equals(Object obj)
		{
			Data other = obj as Data;
			return other != null && GetHashCode().Equals(obj.GetHashCode());
		}
		
		#endregion
		
		#region Equality Methods

		public static Boolean operator ==(Data lhs, Data rhs)
		{
			if (ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
			{
				return false;
			}
			return lhs.Equals(rhs);
		}

		public static Boolean operator !=(Data lhs, Data rhs)
		{
			return !(lhs == rhs);
		}
		
		#endregion	

		#region Public Virtual Methods

		public virtual void Fetch(Session session)
		{
		}
		
		public virtual void Fetch(Session session, Boolean deep)
		{
		}
		
		public virtual void FetchDeep(Session session)
		{			
		}
		
		#endregion
		
		#region Public Methods
		
		public void Set(Session session, Persistence persistence)
		{
			if (!persistence.IsEof)
			{
				for (Type current = GetType(); current != typeof(Data); current = current.BaseType)
				{
					foreach (PropertyInfo property in GetProperties(current))
					{
						String columnName = GetColumnName(property);
						
						if (property.PropertyType == typeof(String))
						{
							property.SetValue(this, persistence.GetString(columnName));
						}
						else if (property.PropertyType == typeof(Boolean))
						{
							Boolean? value = persistence.GetBoolean(columnName);
							property.SetValue(this, value.HasValue && value.Value);
						}
						else if (property.PropertyType == typeof(Boolean?))
						{
							property.SetValue(this, persistence.GetBoolean(columnName));
						}
						else if (property.PropertyType == typeof(Int32))
						{
							Int32? value = persistence.GetInt(columnName);
							property.SetValue(this, value.HasValue ? value.Value : 0);
						}
						else if (property.PropertyType == typeof(Int32?))
						{
							property.SetValue(this, persistence.GetInt(columnName));
						}
						else if (property.PropertyType == typeof(Int64))
						{
							Int64? value = persistence.GetLong(columnName);
							property.SetValue(this, value.HasValue ? value.Value : 0L);
						}
						else if (property.PropertyType == typeof(Int64?))
						{
							property.SetValue(this, persistence.GetLong(columnName));
						}
						else if (property.PropertyType == typeof(Double))
						{
							Double? value = persistence.GetDouble(columnName);
							property.SetValue(this, value.HasValue ? value.Value : 0d);
						}
						else if (property.PropertyType == typeof(Double?))
						{
							property.SetValue(this, persistence.GetDouble(columnName));
						}
						else if (property.PropertyType == typeof(DateTime))
						{
							property.SetValue(this, persistence.GetDate(columnName).Value);
						}
						else if (property.PropertyType == typeof(DateTime?))
						{
							property.SetValue(this, persistence.GetDate(columnName));
						}
						else if (property.PropertyType == typeof(Guid))
						{
							property.SetValue(this, new Guid(persistence.GetString(columnName)));
						}
						else if (property.PropertyType == typeof(Guid?))
						{
							String value = persistence.GetString(columnName);
							property.SetValue(this, !String.IsNullOrEmpty(value) ? new Guid(value) : (Guid?)null);
						}
						else if (property.PropertyType == typeof(Money))
						{
							property.SetValue(this, persistence.GetMoney(columnName));
						}
						else if (property.PropertyType == typeof(Money?))
						{
							Decimal? value = persistence.GetDecimal(columnName);
							property.SetValue(this, value.HasValue ? new Money(value.Value) : (Money?)null);
						}
						else if (property.PropertyType == typeof(PrimaryKey))
						{
							property.SetValue(this, persistence.GetPrimaryKey(columnName));
						}
						else if (property.PropertyType == typeof(PrimaryKey?))
						{						
							Int64? value = persistence.GetLong(columnName);
							property.SetValue(this, value.HasValue ? new PrimaryKey(value.Value) : (PrimaryKey?)null);
						}
						else if (property.PropertyType.IsSubclassOf(typeof(MiskoEnum)))
						{
							MiskoEnum value = null;
							if (persistence.GetLong(columnName).HasValue)
							{
								value = (MiskoEnum)typeof(MiskoEnum).GetMethod("Parse", new[] { typeof(Int64) }).MakeGenericMethod(property.PropertyType).Invoke(null, new Object[] { persistence.GetLong(columnName) });
							}
							else if (persistence.GetString(columnName) != null)
							{
								value = (MiskoEnum)typeof(MiskoEnum).GetMethod("Parse", new[] { typeof(String) }).MakeGenericMethod(property.PropertyType).Invoke(null, new Object[] { persistence.GetString(columnName) });
							}
							else
							{
								value = (MiskoEnum)Activator.CreateInstance(property.PropertyType);
							}
							property.SetValue(this, value);
						}
						else if (property.PropertyType.IsSubclassOf(typeof(StoredData)))
						{
							StoredData item = (StoredData)Activator.CreateInstance(property.PropertyType);
							item.Id = persistence.GetPrimaryKey(columnName).Value;
							property.SetValue(this, item);
						}
						else if (property.PropertyType.IsSubclassOf(typeof(ViewedData)))
						{
							ViewedData item = (ViewedData)Activator.CreateInstance(property.PropertyType);
							item.Set(session, persistence);
							property.SetValue(this, item);
						}
					}
				}
			}
		}
		
		#endregion
		
		#region Internal Methods
		
		internal static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				foreach (Attribute attribute in property.GetCustomAttributes(false))
				{
					if (typeof(StoredData).IsAssignableFrom(type) && attribute is StoredAttribute && (((StoredAttribute)attribute).UseInSql || ((StoredAttribute)attribute).PrimaryKey || ((StoredAttribute)attribute).RowVer))
					{
						yield return property;
					}
					
					if (typeof(ViewedData).IsAssignableFrom(type) && attribute is ViewedAttribute)
					{
						yield return property;
					}
				}
			}
		}
		
		internal String GetColumnName(MemberInfo property)
		{
			foreach (Attribute attribute in property.GetCustomAttributes(false))
			{
				if (this is StoredData && attribute is StoredAttribute && ((StoredAttribute)attribute).UseInSql && !String.IsNullOrEmpty(((StoredAttribute)attribute).ColumnName))
				{
					return ((StoredAttribute)attribute).ColumnName;
				}
				
				if (this is ViewedData && attribute is ViewedAttribute && !String.IsNullOrEmpty(((ViewedAttribute)attribute).ColumnName))
				{
					return ((ViewedAttribute)attribute).ColumnName;
				}
			}

			return property.Name;
		}
		
		#endregion
	}
}
