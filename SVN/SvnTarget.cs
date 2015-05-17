using System;
using System.IO;
using System.Xml.Serialization;
using Newtonsoft.Json;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using MiskoPersist.Tools;

namespace MiskoPersist.SVN
{   
    [XmlRoot("info")]
	public class SvnTarget
	{
		private static Logger Log = Logger.GetInstance(typeof(SvnTarget));
		
		#region Fields

        private Boolean mSet_ = false;

        #endregion
		
		#region Properties

        [XmlElement("entry")]
        public SvnInfoEntry Entry {
        	get;
        	set;
        }

        [XmlIgnore]
        public Boolean IsSet
        {
            get { return mSet_; }
            set { mSet_ = value; }
        }

		#endregion       
        
        #region Constructors

        public SvnTarget()
        {
        }

        public SvnTarget(String file)
            : this(file, null)
        {
        }
        
        public SvnTarget(String file, SvnRevision revision) 
        {
            String args = "info --xml \"" + file + "\"";

            if (revision != null && revision.Revision > 0)
            {
                args += " --revision \"" + revision.Revision + "\"";
            }
            else if (revision != null && revision.Equals(SvnRevision.HEAD))
            {
                args += " --revision HEAD";
            }
            else if (revision != null && revision.Equals(SvnRevision.BASE))
            {
                args += " --revision BASE";
            }
            else if (revision != null && revision.Equals(SvnRevision.COMMITTED))
            {
                args += " --revision COMMITTED";
            }
            else if (revision != null && revision.Equals(SvnRevision.PREV))
            {
                args += " --revision PREV";
            }

            SvnTarget target = null;
            CommandLineProcess result = CommandLineProcess.Execute("svn", args);
            if (result.Success)
            {
                target = Utils.Deserialize<SvnTarget>(result.StandardOutput);
                if (target != null)
                {
                    Entry = target.Entry;
                    mSet_ = true;
                }
            }            
        }

        #endregion        
    }

    #region Structs

    public struct SvnInfoEntry
    {
        #region Fields

        private FileInfo mPath_;
        private SvnTargetType mKind_;
        private Uri mURL_;
        private SvnRevision mRevision_;
        private SvnInfoRepository mRepository_;
        private SvnInfoWorkingCopyInfo mWorkingCopyInfo_;
        private SvnInfoCommit mCommit_;
        private SvnInfoConflict mConflict_;

        #endregion

        #region Properties

        [XmlAttribute("path")]
        public String Path_Agg
        {
            get { return mPath_ != null ? mPath_.ToString() : ""; }
            set { mPath_ = new FileInfo(value); }
        }

        [XmlAttribute("kind")]
        public String Kind_Agg
        {
            get { return mKind_ != null ? mKind_.ToString() : ""; }
            set { mKind_ = SvnTargetType.GetElement(value); }
        }

        [XmlElement("url")]
        public String URL_Agg
        {
            get { return mURL_ != null ? mURL_.ToString() : ""; }
            set { mURL_ = new Uri(value); }
        }

        [XmlAttribute("revision")]
        public Int32 Revision_Agg
        {
            get { return mRevision_.Revision; }
            set { mRevision_ = new SvnRevision(value); }
        }

        [XmlElement("repository")]
        public SvnInfoRepository Repository
        {
            get { return mRepository_; }
            set { mRepository_ = value; }
        }

        [XmlElement("wc-info")]
        public SvnInfoWorkingCopyInfo WorkingCopyInfo
        {
            get { return mWorkingCopyInfo_; }
            set { mWorkingCopyInfo_ = value; }
        }

        [XmlElement("commit")]
        public SvnInfoCommit Commit
        {
            get { return mCommit_; }
            set { mCommit_ = value; }
        }

        [XmlElement("conflict")]
        public SvnInfoConflict Conflict
        {
            get { return mConflict_; }
            set { mConflict_ = value; }
        }

        [XmlIgnore]
        public FileInfo Path
        {
            get { return mPath_; }
        }        
        
        [XmlIgnore]
        public SvnTargetType Kind
        {
            get { return mKind_; }
        }       

        [XmlIgnore]
        public Uri URL
        {
            get { return mURL_; }
        }

        [XmlIgnore]
        public SvnRevision Revision
        {
            get { return mRevision_; }
        }


        #endregion

        public override string ToString()
        {
            return mKind_ + " - " + mPath_;
        }
    }

    public struct SvnInfoRepository
    {
        #region Fields

        private Uri mRoot_;
        private Guid mUUID_;

        #endregion

        #region Properties

        [XmlElement("root")]
        public String Root_agg
        {
            get { return mRoot_ != null ? mRoot_.ToString() : ""; }
            set { mRoot_ = new Uri(value); }
        }

        [XmlElement("uuid")]
        public Guid UUID
        {
            get { return mUUID_; }
            set { mUUID_ = value; }
        }

        [XmlIgnore]
        public Uri Root
        {
            get { return mRoot_; }
        }

        #endregion
    }

    public struct SvnInfoWorkingCopyInfo
    {
        #region Fields

        private FileInfo mAbsolutePath_;
        private String mSchedule_;
        private String mDepth_;
        private DateTime mTextUpdated_;
        private String mChecksum_;

        #endregion

        #region Properties

        [XmlElement("wcroot-abspath")]
        public String AbsolutePath_Agg
        {
            get { return mAbsolutePath_ != null ? mAbsolutePath_.ToString() : ""; }
            set { mAbsolutePath_ = new FileInfo(value); }
        }

        [XmlElement("schedule")]
        public String Schedule
        {
            get { return mSchedule_; }
            set { mSchedule_ = value; }
        }

        [XmlElement("depth")]
        public String Depth
        {
            get { return mDepth_; }
            set { mDepth_ = value; }
        }

        [XmlElement("text-updated")]
        public DateTime TextUpdated
        {
            get { return mTextUpdated_; }
            set { mTextUpdated_ = value; }
        }

        [XmlElement("checksum")]
        public String Checksum
        {
            get { return mChecksum_; }
            set { mChecksum_ = value; }
        }

        [XmlIgnore]
        public FileInfo AbsolutePath
        {
            get { return mAbsolutePath_; }
        }

        #endregion
    }

    public struct SvnInfoCommit
    {
        #region Fields

        private String mAuthor_;
        private DateTime mTimestamp_;
        private SvnRevision mRevision_;

        #endregion

        #region Properties

        [XmlElement("author")]
        public String Author
        {
            get { return mAuthor_; }
            set { mAuthor_ = value; }
        }

        [XmlElement("date")]
        public DateTime Timestamp
        {
            get { return mTimestamp_; }
            set { mTimestamp_ = value; }
        }

        [XmlAttribute("revision")]
        public Int32 Revision_Agg
        {
            get { return mRevision_.Revision; }
            set { mRevision_ = new SvnRevision(value); }
        }

        [XmlIgnore]
        public SvnRevision Revision
        {
            get { return mRevision_; }
        }

        #endregion
    }

    [JsonObjectAttribute(MemberSerialization.OptOut)]
    public struct SvnInfoConflict
    {
        #region Fields

        private FileInfo mPreviousBase_;
        private FileInfo mPreviousWorkingCopy_;
        private FileInfo mCurrentBase_;

        #endregion

        #region Properties

        [XmlElement("prev-base-file")]
        [JsonIgnore]
        public String PreviousBase_Agg
        {
            get { return mPreviousBase_ != null ? mPreviousBase_.ToString() : ""; }
            set { mPreviousBase_ =  new FileInfo(value); }
        }

        [XmlElement("prev-wc-file")]
        [JsonIgnore]
        public String PreviousWorkingCopy_Agg
        {
            get { return mPreviousWorkingCopy_ != null ? mPreviousWorkingCopy_.ToString() : ""; }
            set { mPreviousWorkingCopy_ = new FileInfo(value); }
        }

        [XmlElement("cur-base-file")]
        [JsonIgnore]
        public String CurrentBase_Agg
        {
            get { return mCurrentBase_ != null ? mCurrentBase_.ToString() : ""; }
            set { mCurrentBase_ = new FileInfo(value); }
        }

        [XmlIgnore]
        public FileInfo PreviousBase
        {
            get { return mPreviousBase_; }
        }

        [XmlIgnore]
        public FileInfo PreviousWorkingCopy
        {
            get { return mPreviousWorkingCopy_; }
        }

        [XmlIgnore]
        public FileInfo CurrentBase
        {
            get { return mCurrentBase_; }
        }

        #endregion
    }
    
    #endregion
}