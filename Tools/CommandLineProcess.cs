using System;
using System.Diagnostics;
using MiskoPersist.Core;

namespace MiskoPersist.Tools
{
	public class CommandLineProcess
    {
        private static Logger Log = Logger.GetInstance(typeof(CommandLineProcess));

        #region Fields

        private String mCommand_;
        private String mArgs_;
        private Int32 mReturnCode_;
        private String mStandardOutput_;
        private String mStandardError_;

        #endregion

        #region Properties

        public String Command { get { return mCommand_; } set { mCommand_ = value; } }
        public String Args { get { return mArgs_; } set { mArgs_ = value; } }
        public Int32 ReturnCode { get { return mReturnCode_; } set { mReturnCode_ = value; } }
        public String StandardOutput { get { return mStandardOutput_; } set { mStandardOutput_ = value; } }
        public String StandardError { get { return mStandardError_; } set { mStandardError_ = value; } }
        public Boolean Success { get { return String.IsNullOrEmpty(mStandardError_); } }

        #endregion

        #region Constructor

        public CommandLineProcess(String command, String args)
        {
            mCommand_ = command;
            mArgs_ = args;
        }

        #endregion

        #region Public Methods

        public static CommandLineProcess Execute(String command, String args)
        {
            CommandLineProcess result = new CommandLineProcess(command, args);
            result.Execute();
            return result;
        }

        #endregion

        #region Private Methods

        private void Execute()
        {
            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo(mCommand_, mArgs_);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.RedirectStandardInput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                Process mProcess_ = new Process();
                mProcess_.StartInfo = procStartInfo;
                mProcess_.Start();

                mProcess_.StandardInput.Close(); // Close any input streams as they just lock the app
                mStandardOutput_ = mProcess_.StandardOutput.ReadToEnd();
                mStandardError_ = mProcess_.StandardError.ReadToEnd();

                // Wait for the process to complete
                mProcess_.WaitForExit();

                mReturnCode_ = mProcess_.ExitCode;
                mProcess_.Close();
            }
            catch (Exception e)
            {
                mStandardError_ = e.Message;
                mStandardOutput_ = null;
                mReturnCode_ = -1;
            }
            finally
            {
                if (!Success)
                {
                    throw new MiskoException(StandardError);
                }
            }
        }

        #endregion
    }
}
