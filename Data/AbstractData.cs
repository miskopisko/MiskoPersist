using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public bool IsSet 
        { 
        	get; 
        	set; 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsNotSet 
        { 
        	get 
        	{ 
        		return !IsSet; 
        	} 
        }

        #endregion

        #region Constructors

        public AbstractData()
        {
        }

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
                    property.SetValue(this, persistence.GetString(columnName), null);
                }
                else if (property.PropertyType == typeof(Boolean))
                {
                    Boolean? value = persistence.GetBoolean(columnName);
					property.SetValue(this, value.HasValue && value.Value, null);
                }
                else if (property.PropertyType == typeof(Boolean?))
                {
                    property.SetValue(this, persistence.GetBoolean(columnName), null);
                }
                else if (property.PropertyType == typeof(Int32))
                {
                    Int32? value = persistence.GetInt(columnName);
                    property.SetValue(this, value.HasValue ? value.Value : 0, null);
                }
                else if (property.PropertyType == typeof(Int32?))
                {
                    property.SetValue(this, persistence.GetInt(columnName), null);
                }
                else if (property.PropertyType == typeof(Int64))
                {
                    Int64? value = persistence.GetLong(columnName);
                    property.SetValue(this, value.HasValue ? value.Value : 0, null);
                }
                else if (property.PropertyType == typeof(Int64?))
                {
                    property.SetValue(this, persistence.GetLong(columnName), null);
                }
                else if (property.PropertyType == typeof(Double))
                {
                    Double? value = persistence.GetDouble(columnName);
                    property.SetValue(this, value.HasValue ? value.Value : 0, null);
                }
                else if (property.PropertyType == typeof(Double?))
                {
                    property.SetValue(this, persistence.GetDouble(columnName), null);
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    DateTime? value = persistence.GetDate(columnName);
                    property.SetValue(this, value.HasValue ? value.Value : DateTime.MinValue, null);
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    property.SetValue(this, persistence.GetDate(columnName), null);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(AbstractEnum)))
                {
                    AbstractEnum item = null;

                    if (persistence.GetLong(columnName) != null && persistence.GetLong(columnName).HasValue)
                    {
                        item = (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { persistence.GetLong(columnName) });
                    }
                    else if (persistence.GetString(columnName) != null)
                    {
                        item = (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { persistence.GetString(columnName) });
                    }
                    else
                    {
                        item = (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { -1 });
                    }

                    property.SetValue(this, item, null);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(AbstractStoredData)))
                {
                    AbstractStoredData item = null;

                    if (persistence.GetInt(columnName) > 0)
                    {
                        item = (AbstractStoredData)property.PropertyType.Assembly.CreateInstance(property.PropertyType.FullName);
                        item.Id = persistence.GetPrimaryKey(columnName);
                    }

                    property.SetValue(this, item, null);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(AbstractViewedData)))
                {
                	ConstructorInfo ctor = property.PropertyType.GetConstructor(new[] { typeof(Session), typeof(Persistence) });                   	
                	AbstractViewedData item = (AbstractViewedData)ctor.Invoke(new Object[] { session, persistence });
                    property.SetValue(this, item, null);
                }
                else if (property.PropertyType == typeof(Guid))
                {
                    property.SetValue(this, new Guid(persistence.GetString(columnName)), null);
                }
                else if (property.PropertyType == typeof(Money))
                {
                    property.SetValue(this, persistence.GetMoney(columnName), null);
                }
                else if (property.PropertyType == typeof(PrimaryKey))
                {
                    property.SetValue(this, persistence.GetPrimaryKey(columnName), null);
                }
            }
        }

        #endregion

        #region Protected Internal Methods
        
        protected internal static String GetColumnName(PropertyInfo property)
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
        
        protected internal static List<PropertyInfo> GetProperties(Type type)
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
