using System;
using System.Collections.Generic;
using MiskoPersist.Core;

namespace MiskoPersist.SVN
{
    public class SvnConnectionPool : List<SvnConnection>
    {
        private static Logger Log = Logger.GetInstance(typeof(SvnConnectionPool));

        #region Fields

        public static SvnConnectionPool mInstance_;

        #endregion

        #region Properties

        private static SvnConnectionPool Instance
        {
            get
            {
                if(mInstance_ == null)
                {
                    mInstance_ = new SvnConnectionPool();
                }
                return mInstance_;
            }
        }

        #endregion

        public static void AddConnection(SvnConnection connection)
        {
            Instance.Add(connection);
        }

        public static SvnConnection GetByConnectionString(String connectionString)
        {
            foreach (SvnConnection item in Instance)
            {
                if(item.ConnectionString.Equals(connectionString, StringComparison.CurrentCultureIgnoreCase))
                {
                    return item;
                }
            }
            return null;
        }
    }
}