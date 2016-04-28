using System;
using log4net;

namespace MiskoPersist.Attributes
{
	public class StoredAttribute : Attribute
    {
        private static ILog Log = LogManager.GetLogger(typeof(StoredAttribute));

        #region Fields



        #endregion

        #region Properties

        public String ColumnName 
        { 
        	get;
        	set; 
        }
        
        public Int32 Length
        { 
        	get; 
        	set; 
        }
        
        public Int32 Precision
        { 
        	get;
        	set;
        }
        
        public Boolean NotNull
        {
        	get;
        	set;
        }
        
        public Object DefaultValue
        {
        	get;
        	set;
        }

        public Boolean PrimaryKey
        {
        	get;
        	set;
        }
        
        public Boolean DtCreated
        {
        	get;
        	set;
        }
        
        public Boolean DTModified
        {
        	get;
        	set;
        }
        
        public Boolean RowVer
        {
        	get;
        	set;
        }

        public Boolean UseInSql 
        {
        	get 
        	{
        		return !PrimaryKey && !DtCreated && !DTModified && !RowVer;
        	}
        }

        #endregion

        #region Constructors



        #endregion

        #region Private Methods



        #endregion

        #region Public Methods



        #endregion
    }
}
