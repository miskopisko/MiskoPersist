using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Data.Viewed
{
	public class ViewedData
	{
		private static ILog Log = LogManager.GetLogger(typeof(ViewedData));

		#region Constructors


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
		
		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked
			{
				foreach (PropertyInfo property in GetViewedProperties())
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
		
		public override Boolean Equals(object obj)
		{
			ViewedData other = obj as ViewedData;
			return other != null && GetHashCode().Equals(obj.GetHashCode());
		}

		public static Boolean operator ==(ViewedData lhs, ViewedData rhs)
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

		public static Boolean operator !=(ViewedData lhs, ViewedData rhs)
		{
			return !(lhs == rhs);
		}
		
		#endregion

		#region Private Methods

		

		#endregion

		#region Public Methods
		
		public ViewedData Clone()
		{
			return (ViewedData)MemberwiseClone();
		}

		public virtual void Fetch(Session session)
		{
			Fetch(session, false);
		}
		
		public virtual void Fetch(Session session, Boolean deep)
		{
		}
		
		public virtual void FetchDeep(Session session)
		{
		}
		
		public void Set(Session session, Persistence persistence)
		{
			if (!persistence.IsEof)
			{
				foreach (PropertyInfo property in GetViewedProperties())
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
					else if (property.PropertyType == typeof(PrimaryKey))
					{
						property.SetValue(this, persistence.GetPrimaryKey(columnName));
					}
					else if (property.PropertyType.IsSubclassOf(typeof(MiskoEnum)))
					{
						MiskoEnum value = null;
						if (persistence.GetLong(columnName).HasValue)
						{
							value = (MiskoEnum)typeof(MiskoEnum).GetMethod("Parse", new [] { typeof(Int64) }).MakeGenericMethod(property.PropertyType).Invoke(null, new Object[] { persistence.GetLong(columnName) });
						}
						else if (persistence.GetString(columnName) != null)
						{
							value = (MiskoEnum)typeof(MiskoEnum).GetMethod("Parse", new [] { typeof(String) }).MakeGenericMethod(property.PropertyType).Invoke(null, new Object[] { persistence.GetString(columnName) });
						}
						else
						{
							value = (MiskoEnum)Activator.CreateInstance(property.PropertyType);
						}
						property.SetValue(this, value);
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
		
		private String GetColumnName(MemberInfo property)
		{
			foreach (Attribute attribute in property.GetCustomAttributes(false))
			{
				if (attribute is ViewedAttribute && !String.IsNullOrEmpty(((ViewedAttribute)attribute).ColumnName))
				{
					return ((ViewedAttribute)attribute).ColumnName;
				}
			}

			return property.Name;
		}
		
		private IEnumerable<PropertyInfo> GetViewedProperties()
		{
			foreach (PropertyInfo property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				if (property.GetCustomAttribute(typeof(ViewedAttribute)) != null)
				{
					yield return property;
				}
			}
		}
		
		#endregion
	}
}
