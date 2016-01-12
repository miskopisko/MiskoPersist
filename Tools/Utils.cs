using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using Newtonsoft.Json.Linq;

namespace MiskoPersist.Tools
{
	public static class Utils
    {
        private static Logger Log = Logger.GetInstance(typeof(Utils));

        #region Fields

        private static Byte[] mSalt_ = Encoding.ASCII.GetBytes("b780gU&G&*GP&G&*)");
        private const String mSharedSecret_ = "bn89*(HG)*h80I&*(*)Y*Hjkjub";

		#endregion

		#region Public Static Methods

		public static SerializationType GetSerializationType(this String str)
		{
			try
			{
				XmlDocument document = new XmlDocument();
				document.LoadXml(str);
				return SerializationType.Xml;
			}
			catch
			{
				try
				{
					JObject.Parse(str);
					return SerializationType.Json;
				}
				catch
				{
					throw new MiskoException("Invalid input. Not a valid XML or Json string.");
				}
			}			
		}

		public static String ResolveTextParameters(String text, Object[] parameters)
        {
            if (text != null)
            {
                String param = (String)text.Clone();

                Regex reg = new Regex("[{]([0-9]+|D-[LS]{1}:[0-9]+|N-[0-9]+,[0-9]+:[0-9]+)[}]");
                Int32[] groups = reg.GetGroupNumbers();

                Match match = reg.Match(param);
                Int32 counter = 0;

                while (match.Success)
                {
                    if (groups.Length == 2)
                    {
                        CaptureCollection collect = match.Groups[groups[1]].Captures;
                        foreach (Object t in collect)
                        {
                            if (Regex.IsMatch(param, "[{]" + counter + "[}]")) // Text
                            {
                                String paramVal = parameters != null && parameters[counter] != null ? parameters[counter].ToString() : "";
                                param = Regex.Replace(param, "[{]" + counter + "[}]", paramVal);
                            }

                            counter++;
                        }
                    }

                    match = match.NextMatch();
                }

                return param;
            }

            return "";
        }

        public static String EncryptStringAES(String plainText)
        {
            if (String.IsNullOrEmpty(plainText))
            {
                return "";
            }

            String outStr = null;                       // Encrypted string to return
            RijndaelManaged aesAlg = null;              // RijndaelManaged object used to encrypt the data.

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(mSharedSecret_, mSalt_);

                // Create a RijndaelManaged object
                aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(Int32));
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                    }
                    outStr = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                }
            }

            // Return the encrypted bytes from the memory stream.
            return outStr;
        }

        public static String DecryptStringAES(String cipherText)
        {
            if (String.IsNullOrEmpty(cipherText))
            {
                return "";
            }

            // Declare the RijndaelManaged object
            // used to decrypt the data.
            RijndaelManaged aesAlg = null;

            // Declare the string used to hold
            // the decrypted text.
            String plaintext = null;

            try
            {
                // generate the key from the shared secret and the salt
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(mSharedSecret_, mSalt_);

                // Create the streams used for decryption.                
                Byte[] bytes = Convert.FromBase64String(cipherText);
                using (MemoryStream msDecrypt = new MemoryStream(bytes))
                {
                    // Create a RijndaelManaged object
                    // with the specified key and IV.
                    aesAlg = new RijndaelManaged();
                    aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                    // Get the initialization vector from the encrypted stream
                    aesAlg.IV = ReadByteArray(msDecrypt);
                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
            finally
            {
                // Clear the RijndaelManaged object.
                if (aesAlg != null)
                {
                    aesAlg.Clear();
                }
            }

            return plaintext;
        }
        
        public static DateTime AddWorkDays(DateTime date, Int32 workingDays, List<DateTime> holidays)
		{
        	holidays = holidays ?? new List<DateTime>();
        	
			Int32 direction = workingDays < 0 ? -1 : 1;
			DateTime newDate = date;
			
			while (workingDays != 0)
			{
				newDate = newDate.AddDays(direction);
				
				if (newDate.DayOfWeek != DayOfWeek.Saturday && newDate.DayOfWeek != DayOfWeek.Sunday && !holidays.Contains(newDate))
				{
					workingDays -= direction;
				}
			}
			return newDate;
		}
        
        public static Int32 WorkDaysBetween(DateTime startDate, DateTime endDate, List<DateTime> holidays)
		{
			Int32 result = 0;
			
			if(endDate < startDate)
			{
				return 0;
			}
        	
        	holidays = holidays ?? new List<DateTime>();
        	DateTime date = startDate;
        	
        	while (date <= endDate)
			{
				if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && !holidays.Contains(date))
				{
					result += 1;
				}
				
				date = date.AddDays(1);
			}
			return result;
		}

        #endregion

        #region Private Methods

        private static Byte[] ReadByteArray(Stream s)
        {
            Byte[] rawLength = new Byte[sizeof(Int32)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Stream did not contain properly formatted Byte array");
            }

            Byte[] buffer = new Byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Did not read Byte array properly");
            }

            return buffer;
        }

        #endregion
    }
}
