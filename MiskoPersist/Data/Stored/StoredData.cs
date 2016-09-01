using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using MiskoPersist.MoneyType;

namespace MiskoPersist.Data.Stored
{
    public class StoredData
	{
		private static ILog Log = LogManager.GetLogger(typeof(StoredData));

		#region Fields

		

		#endregion

		#region Properties

		[Stored(PrimaryKey = true)]
		public PrimaryKey Id
		{
			get;
			set;
		}

		#endregion

		#region Other Properties
		
		public Int64 RowVer
		{
			get;
			set;
		}
		
		public Boolean IsSet
		{
			get;
			set;
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

		public StoredData()
		{
			Id = new PrimaryKey(0);
		}

		public StoredData(Session session, Persistence persistence)
		{
			if (!persistence.IsEof)
			{
				Set(session, persistence);
				Id = persistence.GetPrimaryKey("Id").Value;
				RowVer = persistence.GetLong("RowVer").HasValue ? persistence.GetLong("RowVer").Value : 0;
				IsSet = true;
			}
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

		#endregion

		#region Private Methods

		private String BuildSelectStatement()
		{
			// Get a list of all types associated with this class
			List<Type> types = new List<Type>();
			Type currentType = GetType();
			while (!currentType.Equals(typeof(StoredData)))
			{
				types.Add(currentType);
				currentType = currentType.BaseType;
			}
			types.Reverse();

			String sql = "SELECT * FROM ";

			foreach (Type type in types)
			{
				sql += type.Name + ", ";
			}

			sql = sql.Trim().Trim(',');
			sql += " WHERE ";

			for (Int32 i = 0; i < types.Count - 1; i++)
			{
				sql += types[i].Name + ".Id = " + types[i + 1].Name + ".Id";

				if (i < types.Count - 1)
				{
					sql += " AND ";
				}
			}

			sql += types[0].Name + ".Id = ?";

			return sql;
		}

		#endregion

		#region Public Methods

		public static StoredData GetInstanceById(Session session, Type type, Int64 id)
		{
			return GetInstanceById(session, type, new PrimaryKey(id), false);
		}

		public static StoredData GetInstanceById(Session session, Type type, PrimaryKey id)
		{
			return GetInstanceById(session, type, id, false);
		}

		public static StoredData GetInstanceById(Session session, Type type, Int64 id, Boolean deep)
		{
			return GetInstanceById(session, type, new PrimaryKey(id), deep);
		}
		
		public static StoredData GetInstanceById(Session session, Type type, PrimaryKey id, Boolean deep)
		{
			StoredData result = (StoredData)type.Assembly.CreateInstance(type.FullName);

			result.FetchById(session, id, deep);

			return result;
		}

		public void FetchById(Session session, Int64 id)
		{
			FetchById(session, new PrimaryKey(id), false);
		}
		
		public void FetchById(Session session, PrimaryKey id)
		{
			FetchById(session, id, false);
		}

		public void FetchById(Session session, Int64 id, Boolean deep)
		{
			FetchById(session, new PrimaryKey(id), deep);
		}

		public void FetchById(Session session, PrimaryKey id, Boolean deep)
		{
			if (id == null || id.IsNotSet)
			{
				return;
			}

			Persistence p = Persistence.GetInstance(session);
			p.ExecuteQuery(BuildSelectStatement(), Math.Abs(id.Value));
			if (!p.IsEof)
			{
				Id = p.GetPrimaryKey("Id").Value;
				RowVer = p.GetLong("RowVer").HasValue ? p.GetLong("RowVer").Value : 0;
				Set(session, p);
				IsSet = true;
			}
			p.Close();
			p = null;

			if (IsSet)
			{
				if (deep)
				{
					FetchDeep(session);
				}
			}

			Id = id;
		}

		public StoredData Save(Session session)
		{
			if (Id == 0)    // Insert mode
			{
				Create(session);
			}
			else if (Id > 0)    // Update mode
			{
				Store(session);
			}
			else if (Id < 0) // Delete mode
			{
				Remove(session);
			}

			return this;
		}
		
		public virtual void Fetch(Session session)
		{
			Fetch(session, false);
		}
		
		public virtual void Fetch(Session session, Boolean deep)
		{
			if (!IsSet && Id.IsSet)
			{
				FetchById(session, Id, deep);
			}
			
			if (deep)
			{
				FetchDeep(session);
			}
		}
		
		public virtual void FetchDeep(Session session)
		{
			foreach (PropertyInfo property in GetProperties(GetType()))
			{
				if (property.PropertyType.IsSubclassOf(typeof(StoredData)))
				{
					StoredData item = (StoredData)property.GetValue(this, null);
					if (item != null && item.Id > 0 && item.IsNotSet)
					{
						item.FetchById(session, item.Id, true);
					}
				}
			}
		}

		public void Set(Session session, Persistence persistence)
		{
			if (!persistence.IsEof)
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
					else if (property.PropertyType.IsSubclassOf(typeof(StoredData)))
					{
						StoredData item = (StoredData)property.PropertyType.Assembly.CreateInstance(property.PropertyType.FullName);
						item.Id = persistence.GetPrimaryKey(columnName).Value;
						property.SetValue(this, item);
					}
				}
			}
		}
		
		internal static String GetColumnName(MemberInfo property)
		{
			foreach (Attribute attribute in property.GetCustomAttributes(false))
			{
				if (attribute is StoredAttribute && ((StoredAttribute)attribute).UseInSql && !String.IsNullOrEmpty(((StoredAttribute)attribute).ColumnName))
				{
					return ((StoredAttribute)attribute).ColumnName;
				}
			}

			return property.Name;
		}
		
		internal static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
			{
				foreach (Attribute attribute in property.GetCustomAttributes(false))
				{
					if (attribute is StoredAttribute && ((StoredAttribute)attribute).UseInSql)
					{
						yield return property;
					}
				}
			}
		}

		public virtual StoredData Create(Session session)
		{
			return this;
		}
		
		public virtual StoredData Store(Session session)
		{
			return this;
		}
		
		public virtual StoredData Remove(Session session)
		{
			return this;
		}

		#endregion
	}
}
