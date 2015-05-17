using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using MiskoPersist.Tools;

namespace MiskoPersist.SVN
{
    [XmlRoot("status")]
    public class SvnStatus
    {
        private static Logger Log = Logger.GetInstance(typeof(SvnStatus));

        #region Fields

        private SvnStatusTarget mTarget_;

        #endregion

        #region Parameters

        [XmlElement("target")]
        public SvnStatusTarget Target
        {
            get { return mTarget_; }
            set { mTarget_ = value; }
        }

        [XmlIgnore]
        public Int32 Count
        {
            get { return mTarget_.Entries != null ? mTarget_.Entries.Count : 0; }
        }

        #endregion

        #region Constructors

        public SvnStatus()
        {
        }

        public SvnStatus(String file)
            : this(new SvnTarget(file))
        {
        }

        public SvnStatus(SvnTarget target)
        {
            if (target != null && target.IsSet)
            {
                String args = "status --xml \"" + target.Entry.Path + "\"";

                CommandLineProcess result = CommandLineProcess.Execute("svn", args);
                if (result.Success)
                {
                    Target = Utils.Deserialize<SvnStatus>(result.StandardOutput).Target;
                }

                // Conflicted files also bring up their working files (mine, theirs and merged) 
                // We dont want these to show up in the listing so remove them from the list
                // We also want to recurse into Non-Versioned directories
                List<SvnStatusEntry> entries = new List<SvnStatusEntry>(mTarget_.Entries.ToArray());
                foreach (SvnStatusEntry entry in entries)
                {
                    // If an entry is "Conflicted" then remove its sub-entries
                    if (entry.WorkingCopyStatus.Status.Equals(SvnStatusType.Conflicted))
                    {
                        SvnTarget t = new SvnTarget(entry.Path.ToString());

                        foreach (SvnStatusEntry conflictItem in entries)
                        {
                            if (conflictItem.Path.Name.Equals(t.Entry.Conflict.CurrentBase.Name) ||
                                conflictItem.Path.Name.Equals(t.Entry.Conflict.PreviousBase.Name) ||
                                conflictItem.Path.Name.Equals(t.Entry.Conflict.PreviousWorkingCopy.Name))
                            {
                                mTarget_.Entries.Remove(conflictItem); // remove the entry
                            }
                        }
                    }

                    // If an entry is a new un-versioned directory then recurse into that directory and get sub objects
                    if (entry.Path.Attributes.Equals(FileAttributes.Directory) && entry.WorkingCopyStatus.Status.Equals(SvnStatusType.NotVersioned))
                    {
                        List<String> objects = new List<String>(Directory.GetDirectories(entry.Path.FullName, "*.*", SearchOption.AllDirectories));
                        objects.AddRange(Directory.GetFiles(entry.Path.FullName, "*.*", SearchOption.AllDirectories));

                        foreach (String filename in objects)
                        {
                            Target.Entries.Add(new SvnStatusEntry(filename));
                        }
                    }
                }
            }
        }

        #endregion
    }

    #region Structs

    public struct SvnStatusTarget
    {
        #region Fields

        private List<SvnStatusEntry> mEntries;
        private FileInfo mPath_;

        #endregion

        #region Properties

        [XmlElement("entry")]
        public List<SvnStatusEntry> Entries
        {
            get { return mEntries; }
            set { mEntries = value; }
        }

        [XmlAttribute("path")]
        public String Path_Agg
        {
            get { return mPath_ != null ? mPath_.ToString() : ""; }
            set { mPath_ = new FileInfo(value); }
        }

        [XmlIgnore]
        public FileInfo Path
        {
            get { return mPath_; }
        }

        #endregion

        public override string ToString()
        {
            return mPath_.ToString();
        }
    }

    public struct SvnStatusEntry
    {
        #region Fields

        private FileInfo mPath_;
        private SvnStatusWorkingCopyStatus mWorkingCopyStatus_;

        #endregion

        #region Properties

        [XmlAttribute("path")]
        public String Path_Agg
        {
            get { return mPath_ != null ? mPath_.ToString() : ""; }
            set { mPath_ = new FileInfo(value); }
        }

        [XmlIgnore]
        public FileInfo Path
        {
            get { return mPath_; }
        }

        [XmlElement("wc-status")]
        public SvnStatusWorkingCopyStatus WorkingCopyStatus
        {
            get { return mWorkingCopyStatus_; }
            set { mWorkingCopyStatus_ = value; }
        }

        #endregion

        public SvnStatusEntry(String path)
        {
            mPath_ = new FileInfo(path);
            mWorkingCopyStatus_ = new SvnStatusWorkingCopyStatus(SvnStatusType.NotVersioned);
        }

        public override string ToString()
        {
            return mWorkingCopyStatus_.Status + " - " + mPath_.ToString();
        }
    }

    public struct SvnStatusWorkingCopyStatus
    {
        #region Fields

        private SvnPropsStatusType mProps_;
        private SvnStatusType mStatus_;
        private Int32 mRevision_;
        private Boolean mLocked_;
        private Boolean mCopied_;
        private Boolean mSwitched_;
        private Boolean mTreeConflicted_;
        private SvnStatusCommit mCommit_;
        private SvnStatusLock mLock_;

        #endregion

        #region Properties

        [XmlAttribute("props")]
        public String Props_Agg
        {
            get { return mProps_ != null ? mProps_.ToString() : ""; }
            set { mProps_ = SvnPropsStatusType.GetElement(value); }
        }

        [XmlIgnore]
        public SvnPropsStatusType Props
        {
            get { return mProps_; }
        }

        [XmlAttribute("item")]
        public String Status_Agg
        {
            get { return mStatus_ != null ? mStatus_.ToString() : ""; }
            set { mStatus_ = SvnStatusType.GetElement(value); }
        }

        [XmlIgnore]
        public SvnStatusType Status
        {
            get
            {
                if (mStatus_.Equals(SvnStatusType.NoChange) && Props.Equals(SvnPropsStatusType.Modified))
                {
                    mStatus_ = SvnStatusType.ModifiedProperty;
                }
                if (mStatus_.Equals(SvnStatusType.NoChange) && (mLocked_ == true || mLock_.GetHashCode() > 0))
                {
                    mStatus_ = SvnStatusType.Locked;
                }
                return mStatus_;
            }
        }

        [XmlAttribute("revision")]
        public Int32 Revision
        {
            get { return mRevision_; }
            set { mRevision_ = value; }
        }

        [XmlAttribute("wc-locked")]
        public Boolean Locked
        {
            get { return mLocked_; }
            set { mLocked_ = value; }
        }

        [XmlAttribute("copied")]
        public Boolean Copied
        {
            get { return mCopied_; }
            set { mCopied_ = value; }
        }

        [XmlAttribute("switched")]
        public Boolean Switched
        {
            get { return mSwitched_; }
            set { mSwitched_ = value; }
        }

        [XmlAttribute("tree-conflicted")]
        public Boolean TreeConflicted
        {
            get { return mTreeConflicted_; }
            set { mTreeConflicted_ = value; }
        }

        [XmlElement("commit")]
        public SvnStatusCommit Commit
        {
            get { return mCommit_; }
            set { mCommit_ = value; }
        }

        [XmlElement("lock")]
        public SvnStatusLock Lock
        {
            get { return mLock_; }
            set { mLock_ = value; }
        }

        #endregion

        public SvnStatusWorkingCopyStatus(SvnStatusType status)
            : this()
        {
            mStatus_ = status;
        }
    }

    public struct SvnStatusCommit
    {
        #region Fields

        private Int32 mRevision_;
        private String mAuthor_;
        private DateTime mTimeStamp_;

        #endregion

        #region Properties

        [XmlAttribute("revision")]
        public Int32 Revision
        {
            get { return mRevision_; }
            set { mRevision_ = value; }
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

        #endregion
    }

    public struct SvnStatusLock
    {
        #region Fields

        private String mToken_;
        private String mOwner_;
        private String mComment_;
        private DateTime mCreated_;
        private DateTime mExpires_;

        #endregion

        #region Properties

        [XmlAttribute("token")]
        public String Token
        {
            get { return mToken_; }
            set { mToken_ = value; }
        }

        [XmlElement("owner")]
        public String Owner
        {
            get { return mOwner_; }
            set { mOwner_ = value; }
        }

        [XmlElement("comment")]
        public String Comment
        {
            get { return mComment_; }
            set { mComment_ = value; }
        }

        [XmlElement("created")]
        public DateTime Created
        {
            get { return mCreated_; }
            set { mCreated_ = value; }
        }

        [XmlElement("expires")]
        public DateTime Expires
        {
            get { return mExpires_; }
            set { mExpires_ = value; }
        }

        #endregion
    }

    #endregion
}
