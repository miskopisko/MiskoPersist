using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;

namespace MiskoPersist.Data.Stored
{
	public abstract class StoredData : Data
	{
		private static ILog Log = LogManager.GetLogger(typeof(StoredData));

		#region Fields

		

		#endregion

		#region Stored Properties

		[Stored(PrimaryKey = true)]
		public PrimaryKey Id
		{
			get;
			set;
		}
		
		[Stored(RowVer = true)]
		public Int64 RowVer
		{
			get;
			set;
		}

		#endregion
		
		#region Override Properties
		
		public override Boolean IsSet
		{
			get
			{
				return Id.IsSet;
			}
		}
		
		#endregion

		#region Constructors

		protected StoredData()
		{
		}

		protected StoredData(Session session, Persistence persistence) : base(session, persistence)
		{
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

		public static T GetInstanceById<T>(Session session, PrimaryKey id) where T : StoredData, new()
		{
			return GetInstanceById<T>(session, id, false);
		}
		
		public static T GetInstanceById<T>(Session session, PrimaryKey id, Boolean deep) where T : StoredData, new()
		{
			T result = new T();
			result.FetchById(session, id, deep);
			return result;
		}
		
		public void FetchById(Session session, PrimaryKey id)
		{
			FetchById(session, id, false);
		}

		public void FetchById(Session session, PrimaryKey id, Boolean deep)
		{
			if (!id.IsSet)
			{
				return;
			}

			Persistence persistence = Persistence.GetInstance(session);
			persistence.ExecuteQuery(BuildSelectStatement(), Math.Abs(id.Value));
			Set(session, persistence);
			persistence.Close();
			persistence = null;

			if (IsSet && deep)
			{
				FetchDeep(session);
			}
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
		
		#endregion
		
		#region Override Methods
		
		public override void Fetch(Session session)
		{
			Fetch(session, false);
		}
		
		public override void Fetch(Session session, Boolean deep)
		{
			FetchById(session, Id, deep);
		}
		
		public override void FetchDeep(Session session)
		{
			foreach (PropertyInfo property in GetProperties(GetType()))
			{
				if (property.PropertyType.IsSubclassOf(typeof(StoredData)))
				{
					StoredData item = (StoredData)property.GetValue(this, null);
					if (item != null && item.Id > 0 && !item.IsSet)
					{
						item.FetchById(session, item.Id, true);
					}
				}
			}
		}
		
		#endregion
		
		#region Abstract Methods
		
		public abstract StoredData Create(Session session);		
		public abstract StoredData Store(Session session);		
		public abstract StoredData Remove(Session session);
		public abstract void PreSave(Session session, UpdateMode mode);
		public abstract void PostSave(Session session, UpdateMode mode);

		#endregion
	}
}
