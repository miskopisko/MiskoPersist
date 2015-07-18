using System;
using System.Collections.Generic;
using System.Reflection;
using MiskoPersist.Attributes;
using MiskoPersist.Core;
using MiskoPersist.Enums;

namespace MiskoPersist.Data
{	
    public abstract class AbstractStoredData : AbstractData
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

        protected AbstractStoredData()
        {
            Id = new PrimaryKey();
        }

        protected AbstractStoredData(Session session, Persistence persistence)
        {
            Set(session, persistence);
        }

        #endregion

        #region Override Methods

        public override string ToString()
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

            for (int i = 0; i < types.Count - 1; i++)
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
            Set(session, p, deep);
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

        #endregion

        #region Abstract Methods

        public abstract AbstractStoredData Create(Session session);
        public abstract AbstractStoredData Store(Session session);
        public abstract AbstractStoredData Remove(Session session);

        #endregion
    }
}
