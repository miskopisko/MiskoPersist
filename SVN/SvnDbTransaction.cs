using System;
using System.Data;
using System.Data.Common;
using MiskoPersist.Core;

namespace MiskoPersist.SVN
{
    public class SvnDbTransaction : DbTransaction
    {
        private static Logger Log = Logger.GetInstance(typeof(SvnDbTransaction));

        #region Fields



        #endregion

        #region Properties

        protected override DbConnection DbConnection { get { throw new NotImplementedException(); }}
        public override IsolationLevel IsolationLevel { get { return IsolationLevel.Unspecified; } }

        #endregion

        #region Constructors



        #endregion

        #region Override Methods

        public override void Rollback()
        {
            // Do nothing
        }

        public override void Commit()
        {
            // Do nothing
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods



        #endregion
    }
}