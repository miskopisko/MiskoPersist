using System;
using System.Xml.Serialization;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using MiskoPersist.Tools;

namespace MiskoPersist.SVN
{
    [XmlRoot("log")]
    public class SvnLog
    {
        private static Logger Log = Logger.GetInstance(typeof(SvnLog));

        #region Fields

        private SvnLogEntry[] mLogEntries_;

        #endregion

        #region Properties

        [XmlElement("logentry")]
        public SvnLogEntry[] LogEntries { get { return mLogEntries_; } set { mLogEntries_ = value; } }

        [XmlIgnore]
        public Int32 Count { get { return mLogEntries_ != null ? mLogEntries_.Length : 0; } }

        #endregion

        #region Constructors

        public SvnLog()
        {
        }

        public SvnLog(String file, Int16 limit)
            : this(new SvnTarget(file), null, null, true, limit)
        {
        }
        
        public SvnLog(String file)
            : this(new SvnTarget(file), null, null, true, null)
        {
        }

        public SvnLog(String file, Boolean stopOnCopyRename, Int16 limit)
            : this(new SvnTarget(file), null, null, stopOnCopyRename, limit)
        {
        }

        public SvnLog(String file, Boolean stopOnCopyRename)
            : this(new SvnTarget(file), null, null, stopOnCopyRename, null)
        {
        }

        public SvnLog(String file, SvnRevision revisionTo, Int16 limit)
            : this(new SvnTarget(file), null, revisionTo, true, limit)
        {
        }

        public SvnLog(String file, SvnRevision revisionTo)
            : this(new SvnTarget(file), null, revisionTo, true, null)
        {
        }

        public SvnLog(String file, SvnRevision revisionTo, Boolean stopOnCopyRename, Int16 limit)
            : this(new SvnTarget(file), null, revisionTo, stopOnCopyRename, limit)
        {
        }

        public SvnLog(String file, SvnRevision revisionTo, Boolean stopOnCopyRename)
            : this(new SvnTarget(file), null, revisionTo, stopOnCopyRename, null)
        {
        }

        public SvnLog(String file, SvnRevision revisionFrom, SvnRevision revisionTo)
            : this(new SvnTarget(file), revisionFrom, revisionTo, true, null)
        {
        }

        public SvnLog(SvnTarget target, Int16 limit)
            : this(target, null, null, true, limit)
        {
        }

        public SvnLog(SvnTarget target)
            : this(target, null, null, true, null)
        {
        }

        public SvnLog(SvnTarget target, Boolean stopOnCopyRename, Int16 limit)
            : this(target, null, null, stopOnCopyRename, limit)
        {
        }

        public SvnLog(SvnTarget target, SvnRevision revisionTo)
            : this(target, null, revisionTo, true, null)
        {
        }

        public SvnLog(SvnTarget target, SvnRevision revisionFrom, SvnRevision revisionTo)
            : this(target, revisionFrom, revisionTo, true, null)
        {
        }

        public SvnLog(SvnTarget target, SvnRevision revisionTo, Boolean stopOnCopyRename, Int32 limit)
            : this(target, null, revisionTo, stopOnCopyRename, limit)
        {
        }

        public SvnLog(SvnTarget target, SvnRevision revisionTo, Boolean stopOnCopyRename)
            : this(target, null, revisionTo, stopOnCopyRename, null)
        {
        }

        public SvnLog(SvnTarget target, SvnRevision revisionFrom, SvnRevision revisionTo, Boolean stopOnCopyRename, Int32? limit)
        {
            String args = "log --xml --verbose";

            if(limit.HasValue)
            {
                args += " --limit " + limit.ToString();
            }

            if (revisionTo != null && revisionTo.Revision > 0)
            {
                args += " --revision " + revisionTo;                
            }
            else
            {
                args += " --revision HEAD";
            }

            if (revisionFrom != null && revisionFrom.Revision > 0)
            {
                args += ":" + revisionFrom;
            }
            else
            {
                args += ":0";
            }           

            if (stopOnCopyRename)
            {
                args += " --stop-on-copy";
            }

            args += " \"" + target.Entry.URL + "\"";

            SvnLog log = null;
            CommandLineProcess result = CommandLineProcess.Execute("svn", args);
            if(result.Success)
            {
                log = Utils.Deserialize<SvnLog>(result.StandardOutput);
                if(log != null)
                {
                    LogEntries = log.LogEntries;
                }
            }
        }

        #endregion
    }

    #region Structs

    public struct SvnLogEntry
    {
        #region Fields

        private SvnRevision mRevision_;
        private String mAuthor_;
        private DateTime mTimeStamp_;
        private String mMessage_;
        private SvnLogEntryPaths mChangeList_;

        #endregion

        #region Properties

        [XmlAttribute("revision")]
        public Int32 Revision_Agg
        {
            get { return mRevision_.Revision; }
            set { mRevision_ = new SvnRevision(value); }
        }

        [XmlElement("author")]
        public String Author
        {
            get { return mAuthor_; }
            set { mAuthor_ = value; }
        }

        [XmlElement("date")]
        public DateTime TimeStamp
        {
            get { return mTimeStamp_; }
            set { mTimeStamp_ = value; }
        }

        [XmlElement("msg")]
        public String Message
        {
            get { return mMessage_; }
            set { mMessage_ = value; }
        }

        [XmlElement("paths")]
        public SvnLogEntryPaths ChangeList
        {
            get { return mChangeList_; }
            set { mChangeList_ = value; }
        }

        [XmlIgnore]
        public SvnRevision Revision
        {
            get { return mRevision_; }
        }

        #endregion
    }

    public struct SvnLogEntryPaths
    {
        #region Fields

        private SvnLogEntryPath[] mPath_;

        #endregion

        #region Properties

        [XmlElement("path")]
        public SvnLogEntryPath[] Path
        {
            get { return mPath_; }
            set { mPath_ = value; }
        }

        #endregion        
    }

    public struct SvnLogEntryPath
    {
        #region Fields

        private String mPath_;
        private SvnTargetType mKind_;
        private SvnLogEntryPathActionType mAction_;

        #endregion

        #region Properties

        [XmlText]
        public String Path
        {
            get { return mPath_; }
            set { mPath_ = value; }
        }

        [XmlAttribute("kind")]
        public String Kind_Agg
        {
            get { return mKind_ != null ? mKind_.ToString() : ""; }
            set { mKind_ = SvnTargetType.GetElement(value); }
        }

        [XmlAttribute("action")]
        public String Action_Agg
        {
            get { return mAction_ != null ? mAction_.ToString() : ""; }
            set { mAction_ = SvnLogEntryPathActionType.GetElement(value); }
        }

        [XmlIgnore]
        public SvnTargetType Kind
        {
            get { return mKind_; }
        }

        [XmlIgnore]
        public SvnLogEntryPathActionType Action
        {
            get { return mAction_; }
        }

        #endregion
    }

    #endregion
}
