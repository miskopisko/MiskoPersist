using System;
using System.Collections.Generic;
using System.Reflection;
using MiskoPersist.Attributes;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
	public class AbstractStoredData : AbstractData
    {
        private static Logger Log = Logger.GetInstance(typeof(AbstractStoredData));

        #region Fields

        

        #endregion

        #region Stored Properties

        [Stored(PrimaryKey=true)]
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

        #endregion

        #region Constructors

        public AbstractStoredData()
        {
            Id = new PrimaryKey();
        }

        public AbstractStoredData(Session session, Persistence persistence)
        {
        	Set(session, persistence);
        }

        #endregion

        #region Override Methods
        
		public new AbstractStoredData Set(Session session, Persistence persistence)
		{
			IsSet = persistence.Next();
			
			if(IsSet)
            {
				Id = persistence.GetPrimaryKey("Id");
            	RowVer = persistence.GetLong("RowVer").HasValue ? persistence.GetLong("RowVer").Value : 0;            	
            	base.Set(session, persistence);
			}
			
			return this;
		}

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
            while (!currentType.Equals(typeof(AbstractStoredData)))
            {
                types.Add(currentType);
                currentType = currentType.BaseType;
            }
            types.Reverse();

            String sql = "SELECT *" + Environment.NewLine + "FROM   ";

            foreach (Type type in types)
            {
                sql += type.Name + ", ";
            }

            sql = sql.Trim().Trim(',') + Environment.NewLine;
            sql += "WHERE  ";

            for (Int32 i = 0; i < types.Count - 1; i++)
            {
                sql += types[i].Name + ".Id = " + types[i + 1].Name + ".Id" + Environment.NewLine;

                if (i < types.Count - 1)
                {
                    sql += "AND    ";
                }
            }

            sql += types[0].Name + ".Id = ?";

            return sql;
        }

        #endregion

        #region Public Methods

        public static AbstractStoredData GetInstanceById(Session session, Type type, Int64 id)
        {
            return GetInstanceById(session, type, new PrimaryKey(id), false);
        }

        public static AbstractStoredData GetInstanceById(Session session, Type type, PrimaryKey id)
        {
            return GetInstanceById(session, type, id, false);
        }

        public static AbstractStoredData GetInstanceById(Session session, Type type, Int64 id, Boolean deep)
        {
            return GetInstanceById(session, type, new PrimaryKey(id), deep);
        }
        
        public static AbstractStoredData GetInstanceById(Session session, Type type, PrimaryKey id, Boolean deep)
        {
            AbstractStoredData result = (AbstractStoredData)type.Assembly.CreateInstance(type.FullName);

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
            p.ExecuteQuery(BuildSelectStatement(), new Object[] { Math.Abs(id.Value) });
            Set(session, p);
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

        public AbstractStoredData Save(Session session)
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
        	if (!IsSet && Id != null)
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

        #region Abstract Methods

        public virtual AbstractStoredData Create(Session session)
        {
        	return this;
        }
        
        public virtual AbstractStoredData Store(Session session)
        {
        	return this;
        }
        
        public virtual AbstractStoredData Remove(Session session)
        {
        	return this;
        }

        #endregion
    }
}
