using System;
using log4net;

namespace MiskoPersist.Attributes
{
	public class ViewedAttribute : Attribute
    {
        private static ILog Log = LogManager.GetLogger(typeof(ViewedAttribute));

        #region Fields



        #endregion

        #region Properties

        public String ColumnName
        {
        	get;
        	set;
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
