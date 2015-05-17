using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;

namespace MiskoPersist.Core
{
    public class Logger
    {
        #region Fields

        private const String LOGLEVEL = "Log.Level";
        private const String LOGLISTENER = "Log.Listener";

        private const String DEBUG = "DEBUG";
        private const String WARN = "WARN";
        private const String ERROR = "ERROR";

        private const String PREFIX = "GENERAL";
        private const String SUFFIX = ".xml";

        private readonly Type mClass_;
        private static DirectoryInfo mBaseDir_;

        #endregion

        #region Constructor

        private Logger(Type clazz, DirectoryInfo baseDirectory)
        {
            mClass_ = clazz;
            ChangeBase(baseDirectory);
        }

        #endregion

        #region Static GetInstance Methods

        public static Logger GetInstance(Type clazz)
        {
            return new Logger(clazz, null);
        }

        public static Logger GetInstance(Type clazz, DirectoryInfo baseDirectory)
        {
            return new Logger(clazz, baseDirectory);
        }

        #endregion

        public static void ChangeBase(DirectoryInfo baseDirectory)
        {
            if (baseDirectory != null)
            {
                String oldBase = mBaseDir_.FullName;

                if (!baseDirectory.Exists)
                {
                    try
                    {
                        baseDirectory.Create();
                    }
                    catch
                    {
                        baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                    }
                }

                mBaseDir_ = baseDirectory;

                if (oldBase != null && !oldBase.Equals(mBaseDir_))
                {
                    String[] NewLogs = Directory.GetFiles(oldBase, PREFIX + "*" + SUFFIX);
                    String[] OldLogs = Directory.GetFiles(mBaseDir_.FullName, PREFIX + "*" + SUFFIX);

                    Int32 Counter = NewLogs.Length;

                    // Move existing log files to N where N is how many new logs are being moved into the folder
                    foreach (String OldLog in OldLogs)
                    {
                        try
                        {
                            FileInfo OldLogFile = new FileInfo(OldLog);
                            String oldLogFile = mBaseDir_.FullName + @"\" + OldLogFile.Name;

                            while (File.Exists(oldLogFile))
                            {
                                oldLogFile = mBaseDir_.FullName + @"\" + PREFIX + "_" + (Counter++).ToString().PadLeft(4, '0') + SUFFIX;
                            }

                            File.Move(OldLog, oldLogFile);
                        }
                        catch
                        {
                        }
                    }

                    foreach (String NewLog in NewLogs)
                    {
                        try
                        {
                            FileInfo NewLogFile = new FileInfo(NewLog);
                            String newLogFile = mBaseDir_.FullName + @"\" + NewLogFile.Name;

                            File.Move(NewLog, newLogFile);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            else
            {
                mBaseDir_ = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        #region Logger Levels

        public void Debug(String message)
        {
            AddEntry(DEBUG, message, null);
        }

        public void Debug(String message, Exception ex)
        {
            AddEntry(DEBUG, message, ex);
        }

        public void Warn(String message)
        {
            AddEntry(WARN, message, null);
        }

        public void Warn(String message, Exception ex)
        {
            AddEntry(WARN, message, ex);
        }

        public void Error(String message)
        {
            AddEntry(ERROR, message, null);
        }

        public void Error(String message, Exception ex)
        {
            AddEntry(ERROR, message, ex);
        }

        #endregion

        #region Logging Methods

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AddEntry(String level, String message, Exception ex)
        {
            if (mBaseDir_ != null && mBaseDir_.Exists)
            {
                // Fix this at some point
                String LogLevel = ERROR;

                // Default to highest level
                if (!LogLevel.Equals(DEBUG) && !LogLevel.Equals(WARN) && !LogLevel.Equals(ERROR))
                {
                    LogLevel = ERROR;
                }

                if (IsLogable(level) && message != null && message.Trim().Length > 0)
                {
                    try
                    {
                        FileInfo log = new FileInfo(mBaseDir_.FullName + @"\" + PREFIX + SUFFIX);
                        TextWriter mLogger_ = null;

                        String Line = "";

                        if (log.Exists)
                        {
                            StreamReader i = new StreamReader(mBaseDir_ + @"\" + PREFIX + SUFFIX);
                            String data = i.ReadToEnd().Trim();
                            i.Close();

                            StreamWriter o = new StreamWriter(mBaseDir_ + @"\" + PREFIX + SUFFIX, false);
                            o.Write(data.Substring(0, data.Length - 10));
                            o.Close();

                            mLogger_ = new StreamWriter(mBaseDir_ + @"\" + PREFIX + SUFFIX, true);
                        }

                        if (!log.Exists) // Write the header if nothing exists or it was moved above.
                        {
                            mLogger_ = new StreamWriter(mBaseDir_ + @"\" + PREFIX + SUFFIX, false);
                            Line += "<?xml version='1.0' encoding='ISO-8859-1'?>\n";
                            Line += "<logfile>\n";
                        }

                        if (mLogger_ != null && message != null)
                        {
                            String Date = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                            String Level = level;

                            Line += "\t<log timestamp='" + Date + "' level='" + Level + "' class='" + mClass_.FullName + "' machine='" + Environment.MachineName + "'>\n";
                            Line += "\t\t<message>" + SecurityElement.Escape(message.Trim()).Replace("\r\n", "<br />") + "</message>\n";

                            if (ex != null && ex.StackTrace != null)
                            {
                                String[] lines = ex.StackTrace.Split(new[]
                                                                 {
                                                                     "\r\n"
                                                                 }, StringSplitOptions.RemoveEmptyEntries);

                                if (lines.Length > 0)
                                {
                                    Line += "\t\t<stacktrace>\n";
                                }

                                foreach (string t in lines)
                                {
                                    String[] Details = t.Split(new[]
                                                           {
                                                               " in "
                                                           }, StringSplitOptions.RemoveEmptyEntries);

                                    if (Details.Length == 1)
                                    {
                                        Line += "\t\t\t<at>" + SecurityElement.Escape(t).Trim() + "</at>\n";
                                    }
                                    else if (Details.Length == 2)
                                    {
                                        String Method = Details[0].Trim().Substring(3);
                                        Method = Method.Substring(Method.LastIndexOf('.') + 1);

                                        String[] MoreDetails = Details[1].Split(new[]
                                                                            {
                                                                                ":line"
                                                                            }, StringSplitOptions.RemoveEmptyEntries);

                                        if (MoreDetails.Length == 2)
                                        {
                                            Line += "\t\t\t<at class='" + MoreDetails[0].Trim() + "' line='" + MoreDetails[1].Trim() + "' method='"
                                                    + SecurityElement.Escape(Method) + "' />\n";
                                        }
                                        else
                                        {
                                            Line += "\t\t\t<at class='" + Details[1] + "' method='" + Method + "' />\n";
                                        }
                                    }
                                }
                                if (lines.Length > 0)
                                {
                                    Line += "\t\t</stacktrace>\n";
                                }
                            }

                            Line += "\t</log>\n";
                            Line += "</logfile>\n";

                            mLogger_.WriteLine(Line);
                            mLogger_.Close();
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private Boolean IsLogable(String level)
        {
            if (level.Equals(ERROR))
            {
                return true;
            }

            // Fix this at some point
            String LogLevel = ERROR;

            // Default to highest level
            if (!LogLevel.Equals(DEBUG) && !LogLevel.Equals(WARN) && !LogLevel.Equals(ERROR))
            {
                LogLevel = ERROR;
            }

            // Fix this at some point
            String ListeningClasses = "";

            if (!String.IsNullOrEmpty(ListeningClasses))
            {
                return (ListeningClasses.Contains(mClass_.Name)) && ((LogLevel.Equals(WARN) && !level.Equals(DEBUG)) || LogLevel.Equals(DEBUG));
            }

            return (LogLevel.Equals(WARN) && !level.Equals(DEBUG)) || LogLevel.Equals(DEBUG);
        }

        #endregion
    }
}
