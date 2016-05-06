using System;
using System.Security.Cryptography;
using log4net;

namespace MiskoPersist.Tools
{
	public static class PasswordHash
	{
		private static ILog Log = LogManager.GetLogger(typeof(PasswordHash));
		
		private const Char DELIMITER = ':';
		
		private const Int32 ITERATION_INDEX = 0;
		private const Int32 SALT_INDEX = 1;
		private const Int32 PBKDF2_INDEX = 2;
		
		// The following constants may be changed without breaking existing hashes.
		private const Int32 SALT_BYTE_SIZE = 24;
		private const Int32 HASH_BYTE_SIZE = 24;
		private const Int32 PBKDF2_ITERATIONS = 1000;

		#region Public Static Methods
		
		public static String CreateHash(String password)
		{
			// Generate a random salt
			RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider();
			Byte[] salt = new Byte[SALT_BYTE_SIZE];
			csprng.GetBytes(salt);

			// Hash the password and encode the parameters
			Byte[] hash = PBKDF2(password, salt, PBKDF2_ITERATIONS, HASH_BYTE_SIZE);
			return PBKDF2_ITERATIONS + ":" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
		}

		public static Boolean ValidatePassword(String password, String correctHash)
		{
			// Extract the parameters from the hash
			String[] split = correctHash.Split(DELIMITER);
			Int32 iterations = Int32.Parse(split[ITERATION_INDEX]);
			Byte[] salt = Convert.FromBase64String(split[SALT_INDEX]);
			Byte[] hash = Convert.FromBase64String(split[PBKDF2_INDEX]);

			Byte[] testHash = PBKDF2(password, salt, iterations, hash.Length);
			return SlowEquals(hash, testHash);
		}
		
		#endregion

		#region Private Static Methods
		
		private static Boolean SlowEquals(Byte[] a, Byte[] b)
		{
			UInt32 diff = (UInt32)a.Length ^ (UInt32)b.Length;
			for (Int32 i = 0; i < a.Length && i < b.Length; i++)
			{
				diff |= (UInt32)(a[i] ^ b[i]);
			}
			return diff == 0;
		}

		private static Byte[] PBKDF2(String password, Byte[] salt, Int32 iterations, Int32 outputBytes)
		{
			Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
			return pbkdf2.GetBytes(outputBytes);
		}
		
		#endregion
	}
}
