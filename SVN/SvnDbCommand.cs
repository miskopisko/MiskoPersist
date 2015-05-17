using System;
using System.Data;
using System.Data.Common;
using MiskoPersist.Core;

namespace MiskoPersist.SVN
{
    public class SvnDbCommand : DbCommand
    {
        private static Logger Log = Logger.GetInstance(typeof(SvnDbCommand));

        #region Fields



        #endregion

        #region Properties

        public override string CommandText { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public override int CommandTimeout { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public override CommandType CommandType { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        protected override DbConnection DbConnection { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        protected override DbParameterCollection DbParameterCollection { get { throw new NotImplementedException(); } }
        protected override DbTransaction DbTransaction { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public override bool DesignTimeVisible { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public override UpdateRowSource UpdatedRowSource { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        #endregion

        #region Constructors



        #endregion

        #region Override Methods

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods



        #endregion

        #region Public Methods



        #endregion        
    }
}