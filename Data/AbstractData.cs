using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;
using System.ComponentModel;

namespace MiskoPersist.Data
{
    public class AbstractData
    {
        private static Logger Log = Logger.GetInstance(typeof(AbstractData));

        #region Constants

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        			{ 	
                        TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                        TypeNameHandling = TypeNameHandling.Objects,
                        NullValueHandling = NullValueHandling.Ignore,
                        DateFormatString = "yyyy-MM-dd",
						Formatting = Formatting.Indented
        			};

        #endregion

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

        public AbstractData(Session session, Persistence persistence)
        {
            Set(session, persistence);
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods

        public AbstractData Set(Session session, Persistence persistence)
        {
            return Set(session, persistence, false);
        }

        public AbstractData Set(Session session, Persistence persistence, bool deep)
        {
            if (persistence.Next())
            {
                PropertyInfo[] properties = null;
                if(this is AbstractStoredData)
                {
                    properties = GetStoredProperties();

                    ((AbstractStoredData)this).Id = persistence.GetPrimaryKey("Id");
                    ((AbstractStoredData)this).RowVer = persistence.GetLong("RowVer").HasValue ? persistence.GetLong("RowVer").Value : 0;
                }
                else if(this is AbstractViewedData)
                {
                    properties = GetViewedProperties();
                }
                
                IsSet = true;

                foreach (PropertyInfo property in properties)
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

                    if (deep)
                    {
                        FetchDeep(session);
                    }
                }
            }

            return this;
        }

        public void FetchDeep(Session session)
        {
            foreach (PropertyInfo property in GetStoredProperties(GetType()))
            {
                if (property.PropertyType.IsSubclassOf(typeof(AbstractStoredData)))
                {
                    AbstractStoredData item = (AbstractStoredData)property.GetValue(this, null);
                    if (item != null && item.Id > 0 && item.IsNotSet)
                    {
                        item.FetchById(session, item.Id, true);
                    }
                }
            }
        }

        #endregion

        #region Protected Internal Methods

        protected internal String GetColumnName(PropertyInfo property)
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

        protected internal PropertyInfo[] GetStoredProperties()
        {
            PropertyInfo[] properties = GetType().GetProperties();

            List<PropertyInfo> storedProperties = new List<PropertyInfo>();
            foreach (PropertyInfo property in properties)
            {
                foreach (Attribute attribute in property.GetCustomAttributes(false))
                {
                    if (attribute is StoredAttribute && ((StoredAttribute)attribute).UseInSql)
                    {
                        storedProperties.Add(property);
                    }
                }
            }

            return storedProperties.ToArray();
        }

        protected internal PropertyInfo[] GetStoredProperties(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            List<PropertyInfo> storedProperties = new List<PropertyInfo>();
            foreach (PropertyInfo property in properties)
            {
                foreach (Attribute attribute in property.GetCustomAttributes(false))
                {
                    if (attribute is StoredAttribute && ((StoredAttribute)attribute).UseInSql)
                    {
                        storedProperties.Add(property);
                    }
                }
            }

            return storedProperties.ToArray();
        }

        protected internal PropertyInfo[] GetViewedProperties()
        {
            PropertyInfo[] properties = GetType().GetProperties();

            List<PropertyInfo> viewedProperties = new List<PropertyInfo>();
            foreach (PropertyInfo property in properties)
            {
                foreach (Attribute attribute in property.GetCustomAttributes(false))
                {
                    if (attribute is ViewedAttribute)
                    {
                        viewedProperties.Add(property);
                    }
                }
            }

            return viewedProperties.ToArray();
        }

        #endregion     
        
        #region JSON Serialization
        
        public static String SerializeJson(Object obj)
        {
        	return JsonConvert.SerializeObject(obj, settings);
        }
        
        public static Object DeserializeJson(String json)
        {
        	return JsonConvert.DeserializeObject(json, settings);
        }
        
        #endregion
    }
}
