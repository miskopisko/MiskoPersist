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

        public virtual Boolean IsSet 
        { 
        	get; 
        	set; 
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public Boolean IsNotSet 
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
                    property.SetValue(this, persistence.GetString(columnName));
                }
                else if (property.PropertyType == typeof(Boolean))
                {
					property.SetValue(this, persistence.GetBoolean(columnName).Value);
                }
                else if (property.PropertyType == typeof(Boolean?))
                {
                    property.SetValue(this, persistence.GetBoolean(columnName));
                }
                else if (property.PropertyType == typeof(Int32))
                {
                    property.SetValue(this, persistence.GetInt(columnName).Value);
                }
                else if (property.PropertyType == typeof(Int32?))
                {
                    property.SetValue(this, persistence.GetInt(columnName));
                }
                else if (property.PropertyType == typeof(Int64))
                {
                    property.SetValue(this, persistence.GetLong(columnName).Value);
                }
                else if (property.PropertyType == typeof(Int64?))
                {
                    property.SetValue(this, persistence.GetLong(columnName));
                }
                else if (property.PropertyType == typeof(Double))
                {
                    property.SetValue(this, persistence.GetDouble(columnName).Value);
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
                    AbstractEnum item = null;

                    if (persistence.GetLong(columnName) != null && persistence.GetLong(columnName).HasValue)
                    {
                        item = (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { persistence.GetLong(columnName) });
                    }
                    else if (persistence.GetString(columnName) != null)
                    {
                        item = (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { persistence.GetString(columnName) });
                    }
                    else
                    {
                        item = (AbstractEnum)property.PropertyType.InvokeMember("GetElement", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new Object[] { -1 });
                    }

                    property.SetValue(this, item);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(AbstractStoredData)))
                {
                    AbstractStoredData item = null;

                    if (persistence.GetInt(columnName) > 0)
                    {
                        item = (AbstractStoredData)property.PropertyType.Assembly.CreateInstance(property.PropertyType.FullName);
                        item.Id = persistence.GetPrimaryKey(columnName);
                    }

                    property.SetValue(this, item);
                }
                else if (property.PropertyType.IsSubclassOf(typeof(AbstractViewedData)))
                {
                	ConstructorInfo ctor = property.PropertyType.GetConstructor(new[] { typeof(Session), typeof(Persistence) });                   	
                	AbstractViewedData item = (AbstractViewedData)ctor.Invoke(new Object[] { session, persistence });
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
