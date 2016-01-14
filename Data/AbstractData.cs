using System;
using System.Collections.Generic;
using System.Reflection;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Data
{
    public class AbstractData
    {
        private static Logger Log = Logger.GetInstance(typeof(AbstractData));

        #region Fields



        #endregion

        #region Properties

        

        #endregion

        #region Constructors


        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        protected void Set(Session session, Persistence persistence)
        {
            foreach (PropertyInfo property in GetProperties(GetType()))
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
                else if (property.PropertyType == typeof(Money))
                {
                    property.SetValue(this, persistence.GetMoney(columnName));
                }
                else if (property.PropertyType == typeof(PrimaryKey))
                {
                    property.SetValue(this, persistence.GetPrimaryKey(columnName));
                }
                else if (property.PropertyType.IsSubclassOf(typeof(AbstractEnum)))
                {
                    if (persistence.GetLong(columnName).HasValue)
                    {
                    	property.SetValue(this, (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { persistence.GetLong(columnName) }));
                    }
                    else if (persistence.GetString(columnName) != null)
                    {
                    	property.SetValue(this, (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { persistence.GetString(columnName) }));
                    }
                    else
                    {
                    	property.SetValue(this, (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { -1 }));
                    }
                }
                else if (property.PropertyType.IsSubclassOf(typeof(AbstractStoredData)))
                {
                	AbstractStoredData item = (AbstractStoredData)property.PropertyType.Assembly.CreateInstance(property.PropertyType.FullName);
                	item.Id = persistence.GetPrimaryKey(columnName);
                    property.SetValue(this, item);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(AbstractViewedData)))
                {
                	AbstractViewedData item = (AbstractViewedData)Activator.CreateInstance(property.PropertyType);
                	item.Set(session, persistence);
                    property.SetValue(this, item);
                }
            }
        }

        #endregion

        #region Protected Internal Methods
        
        internal static String GetColumnName(PropertyInfo property)
        {
            foreach (Attribute attribute in property.GetCustomAttributes(false))
            {
                if (attribute is StoredAttribute && ((StoredAttribute)attribute).UseInSql && !String.IsNullOrEmpty(((StoredAttribute)attribute).ColumnName))
                {
                    return ((StoredAttribute)attribute).ColumnName;
                }

                if (attribute is ViewedAttribute && !String.IsNullOrEmpty(((ViewedAttribute)attribute).ColumnName))
                {
                    return ((ViewedAttribute)attribute).ColumnName;
                }
            }

            return property.Name;
        }
        
        internal static List<PropertyInfo> GetProperties(Type type)
        {
        	List<PropertyInfo> result = new List<PropertyInfo>();
        	
        	foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
            	foreach (Attribute attribute in property.GetCustomAttributes(false))
                {
            		if(attribute is ViewedAttribute || (attribute is StoredAttribute && ((StoredAttribute)attribute).UseInSql))
                    {
                        result.Add(property);
                    }
            	}
            }
        	
        	return result;
        }
		
        #endregion		
    }
}
