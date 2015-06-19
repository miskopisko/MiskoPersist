using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MiskoPersist.Core;
using MiskoPersist.Tools;

namespace MiskoPersist.SVN
{
	public class Subversion
	{
		private static Logger Log = Logger.GetInstance(typeof(Subversion));

		// Get the currently installed version of SVN
		public static String InstalledVersion()
		{
			return CommandLineProcess.Execute("svn", "--non-interactive --version --quiet").StandardOutput.Trim();
		}

		// Get the latest revision of the working copy
		public static SvnRevision WorkingCopyRevision(FileInfo file)
		{
			String result = CommandLineProcess.Execute("svnversion", "-c -q -n \"" + file.FullName + "\"").StandardOutput;

			return new SvnRevision(Regex.Replace(result.Substring(result.IndexOf(':') + 1), "[^0-9]", ""));
		}
				
		// Call SVN cat command to get a local file
		public static String Cat(FileInfo file)
		{
			return CommandLineProcess.Execute("svn", "--non-interactive cat \"" + file.FullName + "\"").StandardOutput;
		}

		// Call SVN cat command to get a repo file
		public static String Cat(Uri file)
		{
			return CommandLineProcess.Execute("svn", "--non-interactive cat \"" + file.AbsoluteUri + "\"").StandardOutput;
		}
		
		// Call SVN command line to export a file to a specific location
		public static FileInfo Export(SvnTarget target, String location)
		{
			CommandLineProcess result = CommandLineProcess.Execute("svn", "--non-interactive export --force --quiet --revision \"" + target.Entry.Revision + "\" \"" + target.Entry.URL + "\" \"" + location + "\"");

			if(result.Success)
			{
				return new FileInfo(location);
			}

			return null;
		}
		
		// Call SVN add command to add a file to the repo
		public static Boolean Add(FileInfo file)
		{			
			return CommandLineProcess.Execute("svn", "--non-interactive add -q \"" + file.FullName + "\"").Success;
		}

		// Call SVN revert command to revert a file to the latest version
		public static Boolean Revert(FileInfo file)
		{
			//return CommandLineProcess.Execute("svn", "--non-interactive revert -q -R \"" + file.FullName + "\"").Success;
			return CommandLineProcess.Execute("svn", "--non-interactive revert -q \"" + file.FullName + "\"").Success;
		}
				
		// Call SVN delete command to delete a file from the repo
		public static Boolean Delete(FileInfo file)
		{
			return CommandLineProcess.Execute("svn", "--non-interactive delete -q \"" + file.FullName + "\"").Success;
		}

		// Call SVN cleanup command to delete a file from the repo
		public static Boolean Cleanup(String workingCopy)
		{
			return CommandLineProcess.Execute("svn", "--non-interactive cleanup \"" + workingCopy + "\"").Success;
		}
		
		// Call SVN commit to commit the changelist
		public static Int32 Commit(List<FileInfo> files, String message, String username, String password)
		{			
			String fileList = "";
			foreach (FileInfo file in files)
			{
				fileList += "\"" + file.FullName + "\" ";
			}
			
			// Generate a new temp file to store the message
			String messageFile = Environment.GetEnvironmentVariable("TEMP") + Path.DirectorySeparatorChar + Guid.NewGuid() + ".txt";
			File.WriteAllText(messageFile, message);

			// Execute the commiy
			CommandLineProcess result = CommandLineProcess.Execute("svn", "--non-interactive commit --file \"" + messageFile + "\" --username \"" + username + "\" --password \"" + password + "\" " + fileList.Trim());

			// Pull the revision number from the StdOut
			if(result.Success)
			{
				String[] lines = result.StandardOutput.Split(Environment.NewLine.ToCharArray());
				foreach (String line in lines)
				{
					if (line.StartsWith("Committed revision "))
					{
						return Convert.ToInt32(line.Replace("Committed revision ", "").Replace(".", ""));
					}
				}
			}

			return 0;
		}
	}
}
