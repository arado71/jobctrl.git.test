using log4net;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace Tct.ActivityRecorderClient.Serialization
{
	public static class ProtectedDataSerializationHelper
	{
		public static bool Save<T>(string path, T itemToSave)
		{
			byte[] protectedData;
			return ProtectedDataHelper.Protect(itemToSave, out protectedData) && IsolatedStorageSerializationHelper.Save(path, protectedData);
		}

		public static bool Load<T>(string path, out T value)
		{
			byte[] protectedData;
			if (IsolatedStorageSerializationHelper.Load(path, out protectedData))
			{
				return ProtectedDataHelper.Unprotect(protectedData, out value);
			}
			else
			{
				value = default(T);
				return false;
			}
		}

		public static bool Exists(string path)
		{
			return IsolatedStorageSerializationHelper.Exists(path);
		}

		public static bool Delete(string path)
		{
			return IsolatedStorageSerializationHelper.Delete(path);
		}
	}

	public static class ProtectedDataHelper
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static bool Protect<T>(T itemToProtect, out byte[] protectedData)
		{
			try
			{
				using (var memStream = new MemoryStream())
				{
					var serializer = new DataContractSerializer(typeof(T));
					serializer.WriteObject(memStream, itemToProtect);
					if (OperatingSystem.IsWindows())
					{
						protectedData = ProtectedData.Protect(memStream.ToArray(), null, DataProtectionScope.CurrentUser);
					}
					else
					{
						protectedData = ProtectDataMac.Protect(memStream.ToArray());
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to protect item.", ex);
				protectedData = null;
				return false;
			}
		}

		public static bool Unprotect<T>(byte[] dataToUnprotect, out T unprotectedItem)
		{
			try
			{
				using (var memStream = new MemoryStream())
				{
					byte[] unprotectedData;
					if (OperatingSystem.IsWindows())
					{
						unprotectedData = ProtectedData.Unprotect(dataToUnprotect, null, DataProtectionScope.CurrentUser);
					}
					else
					{
						unprotectedData = ProtectDataMac.Unprotect(dataToUnprotect);
					}
					memStream.Write(unprotectedData, 0, unprotectedData.Length);
					memStream.Seek(0, SeekOrigin.Begin);
					var serializer = new DataContractSerializer(typeof(T));
					unprotectedItem = (T)serializer.ReadObject(memStream);
					return true;
				}
			}
			catch (Exception ex)
			{
				log.Error("Unable to unprotect data.", ex);
				unprotectedItem = default(T);
				return false;
			}
		}
	}

	public sealed class ProtectDataMac
	{
		private const int KeySizeBytes = 32; // 256-bit AES key

		public static byte[] Protect(byte[] data)
		{
			var key = GetOrCreateKey();

			// AES-GCM requires nonce + tag + ciphertext.
			byte[] nonce = RandomNumberGenerator.GetBytes(12);
			byte[] tag = new byte[16];
			byte[] ciphertext = new byte[data.Length];

			using var aes = new AesGcm(key, 16);
			aes.Encrypt(nonce, data, ciphertext, tag);

			// Final format: [nonce | tag | ciphertext]
			byte[] output = new byte[nonce.Length + tag.Length + ciphertext.Length];
			Buffer.BlockCopy(nonce, 0, output, 0, nonce.Length);
			Buffer.BlockCopy(tag, 0, output, nonce.Length, tag.Length);
			Buffer.BlockCopy(ciphertext, 0, output, nonce.Length + tag.Length, ciphertext.Length);

			return output;
		}

		public static byte[] Unprotect(byte[] data)
		{
			var key = GetOrCreateKey();

			byte[] nonce = new byte[12];
			byte[] tag = new byte[16];
			byte[] ciphertext = new byte[data.Length - nonce.Length - tag.Length];

			Buffer.BlockCopy(data, 0, nonce, 0, nonce.Length);
			Buffer.BlockCopy(data, nonce.Length, tag, 0, tag.Length);
			Buffer.BlockCopy(data, nonce.Length + tag.Length, ciphertext, 0, ciphertext.Length);

			byte[] plaintext = new byte[ciphertext.Length];

			using var aes = new AesGcm(key, 16);
			aes.Decrypt(nonce, ciphertext, tag, plaintext);

			return plaintext;
		}

		private static byte[] EncryptionKey = null;
		private static readonly byte[] FixedSalt = "JobCTRLSaltForProtectDataMac"u8.ToArray();
		// Ideally we would use keychain here, but KISS for now
		private static byte[] GetOrCreateKey()
		{
			if (EncryptionKey == null)
			{
				string username = Environment.UserName ?? "unknown-user";

				// Derive AES key from username
				using var kdf = new Rfc2898DeriveBytes(
					password: username,
					salt: FixedSalt,
					iterations: 200_000,
					hashAlgorithm: HashAlgorithmName.SHA256);

				EncryptionKey = kdf.GetBytes(KeySizeBytes);
			}
			return EncryptionKey;
		}
	}
}
