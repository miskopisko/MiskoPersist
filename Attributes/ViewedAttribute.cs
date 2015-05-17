using System;
using MiskoPersist.Core;

namespace MiskoPersist.Attributes
{
    public class ViewedAttribute : Attribute
    {
        private static Logger Log = Logger.GetInstance(typeof(ViewedAttribute));

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
