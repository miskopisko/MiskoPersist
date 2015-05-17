using System;
using MiskoPersist.Core;

namespace MiskoPersist.SVN
{
    public class SvnRevision : IComparable
    {
        private static Logger Log = Logger.GetInstance(typeof(SvnRevision));

        #region Global Constants

        public static readonly SvnRevision HEAD = new SvnRevision(-1000);
        public static readonly SvnRevision BASE = new SvnRevision(-1001);
        public static readonly SvnRevision COMMITTED = new SvnRevision(-1002);
        public static readonly SvnRevision PREV = new SvnRevision(-1003);

        #endregion

        #region Fields



        #endregion

        #region Properties

        public Int32 Revision { get; set; }
        public Int32 Previous { get { return Revision - 1; } }

        #endregion

        #region Constructor

        public SvnRevision()
        {
        }

        public SvnRevision(Int32 revision)
        {
            Revision = revision;
        }

        public SvnRevision(String revision)
        {
            Revision = Convert.ToInt32(revision);
        }

        #endregion

        #region Public Methods



        #endregion

        #region Override Methods

        public override string ToString()
        {
            if (this.Equals(SvnRevision.HEAD))
            {
                return "HEAD";
            }
            else if (this.Equals(SvnRevision.BASE))
            {
                return "BASE";
            }
            else if (this.Equals(SvnRevision.COMMITTED))
            {
                return "COMMITTED";
            }
            else if (this.Equals(SvnRevision.PREV))
            {
                return "PREV";
            }
            else
            {
                return Revision.ToString();
            }
        }

        #endregion

        #region IComparable Implementation

        public int CompareTo(object obj)
        {
            if (obj is SvnRevision)
            {
                return ((SvnRevision)obj).Revision.CompareTo(this.Revision);
            }
            return -1;
        }

        #endregion
    }
}