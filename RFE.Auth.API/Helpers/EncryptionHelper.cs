using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
namespace RFE.Auth.API.Helpers
{
    /// <summary>
    /// EncryptionHelper
    /// </summary>
    public static class EncryptionHelper
    {
        /// <summary>
        /// HashPassword
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashPassword(string password)
        {
            // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
            byte[] salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
        private static void EncryptData(string inName, string outName, byte[] tdesKey, byte[] tdesIV)
        {
            //Create the file streams to handle the input and output files.
            FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
            FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
            fout.SetLength(0);

            //Create variables to help with read and write.
            byte[] bin = new byte[100]; //This is intermediate storage for the encryption.
            long rdlen = 0;              //This is the total number of bytes written.
            long totlen = fin.Length;    //This is the total length of the input file.
            int len;                     //This is the number of bytes to be written at a time.

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            CryptoStream encStream = new CryptoStream(fout, tdes.CreateEncryptor(tdesKey, tdesIV), CryptoStreamMode.Write);

            Console.WriteLine("Encrypting...");

            //Read from the input file, then encrypt and write to the output file.
            while(rdlen < totlen)
            {
                len = fin.Read(bin, 0, 100);
                encStream.Write(bin, 0, len);
                rdlen = rdlen + len;
                Console.WriteLine("{0} bytes processed", rdlen);
            }

            encStream.Close();
        }
        //this function Convert to Encord your Password 
        public static string EncodePasswordToBase64(string password) 
        {
        try 
        {
            byte[] encData_byte = new byte[password.Length]; 
            encData_byte = System.Text.Encoding.UTF8.GetBytes(password); 
            string encodedData = Convert.ToBase64String(encData_byte); 
            return encodedData; 
        } 
        catch (Exception ex) 
        { 
            throw new Exception("Error in base64Encode" + ex.Message); 
        } 
        }
        //this function Convert to Decord your Password
        public static string DecodeFrom64(string encodedData) 
        {
        System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding(); 
        System.Text.Decoder utf8Decode = encoder.GetDecoder();
        byte[] todecode_byte = Convert.FromBase64String(encodedData); 
        int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length); 
        char[] decoded_char = new char[charCount]; 
        utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0); 
        string result = new String(decoded_char); 
        return result;
        }
    }
}