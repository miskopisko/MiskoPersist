using System;
using System.Diagnostics;
using log4net;
using MiskoPersist.Core;

namespace MiskoPersist.Tools
{
	public class CommandLineProcess
    {
        private static ILog Log = LogManager.GetLogger(typeof(CommandLineProcess));

        #region Fields

        

        #endregion

        #region Properties

        public String Command { get; set; }
		public String Args { get; set; }
		public Int32 ReturnCode  { get; set; }
        public String StandardOutput { get; set; }
		public String StandardError { get; set; }
        public Boolean Success { get { return String.IsNullOrEmpty(StandardError); } }

        #endregion

        #region Constructor

        public CommandLineProcess(String command, String args)
        {
            Command = command;
            Args = args;
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
                ProcessStartInfo procStartInfo = new ProcessStartInfo(Command, Args);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.RedirectStandardError = true;
                procStartInfo.RedirectStandardInput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                Process mProcess_ = new Process();
                mProcess_.StartInfo = procStartInfo;
                mProcess_.Start();

                mProcess_.StandardInput.Close(); // Close any input streams as they just lock the app
                StandardOutput = mProcess_.StandardOutput.ReadToEnd();
                StandardError = mProcess_.StandardError.ReadToEnd();

                // Wait for the process to complete
                mProcess_.WaitForExit();

                ReturnCode = mProcess_.ExitCode;
                mProcess_.Close();
            }
            catch (Exception e)
            {
                StandardError = e.Message;
                StandardOutput = null;
                ReturnCode = -1;
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
