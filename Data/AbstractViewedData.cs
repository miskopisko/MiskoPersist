using System;
using System.Reflection;
using MiskoPersist.Core;

namespace MiskoPersist.Data
{
    public abstract class AbstractViewedData : AbstractData
    {
        private static Logger Log = Logger.GetInstance(typeof(AbstractViewedData));

        #region Fields



        #endregion

        #region Properties

        

        #endregion

        #region Constructors

        protected AbstractViewedData()
        {
        }

        protected AbstractViewedData(Session session, Persistence persistence)
        {
            Set(session, persistence, true);
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

        

        #endregion

        #region Public Methods

        

        #endregion
    }
}
