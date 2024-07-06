/******************************************************************************
 *
 * Copyright 2019 Tim Brogden
 * All rights reserved. This program and the accompanying materials   
 * are made available under the terms of the Eclipse Public License v1.0  
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *           
 * Contributors: 
 * Tim Brogden - Keysticks application and installer
 *
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Keysticks.Config
{
    public class SymmetricCrypto
    {
        private string _seed;
        private readonly int _defaultSeedLenBytes = 50;        // Longer than necessary to avoid short message if the seed is sent using RSA encryption
        private readonly int _keyLenBytes = 16;
        private readonly int _ivLenBytes = 8;
        private SymmetricAlgorithm _algorithm;

        public string Seed { get { return _seed; } }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public SymmetricCrypto()
        {            
            // Generate a random seed
            byte[] seedBytes = new byte[_defaultSeedLenBytes];
            Random rand = new Random();
            for (int i = 0; i < seedBytes.Length; i++)
            {
                seedBytes[i] = (byte)rand.Next();
            }

            // Store seed
            _seed = Convert.ToBase64String(seedBytes);

            // Initialise
            Initialise();
        }

        /// <summary>
        /// Constructor with specified seed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public SymmetricCrypto(string seed)
        {
            // Store the seed
            _seed = seed;            

            // Initialise
            Initialise();
        }

        /// <summary>
        /// Create the crypto
        /// </summary>
        /// <param name="seedBytes"></param>
        private void Initialise()
        {
            byte[] seedBytes = Convert.FromBase64String(_seed);             
            byte[] encryptionKey = new byte[_keyLenBytes];
            byte[] encryptionIV = new byte[_ivLenBytes];

            // Manipulate bytes a bit
            byte prevByte = 0;
            for (int i = 0; i < Math.Min(_keyLenBytes, seedBytes.Length); i++)
            {
                encryptionKey[i] = (byte)(seedBytes[i] ^ prevByte);
                prevByte = encryptionKey[i];
            }
            prevByte = 0;
            for (int i = _keyLenBytes; i < Math.Min(_keyLenBytes + _ivLenBytes, seedBytes.Length); i++)
            {
                encryptionIV[i - _keyLenBytes] = (byte)(seedBytes[i] ^ prevByte);
                prevByte = encryptionIV[i - _keyLenBytes];
            }

            _algorithm = new TripleDESCryptoServiceProvider();
            _algorithm.Key = encryptionKey;
            _algorithm.IV = encryptionIV;
        }

        /// <summary>
        /// Encrypt a plain text string
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return "";
            }

            // Convert to bytes
            byte[] plainTextData = Encoding.UTF8.GetBytes(plainText);

            // Calculate a checksum byte
            byte checksum = 0;
            foreach (byte b in plainTextData)
            {
                checksum += b;
            }

            // Add the checksum to each byte
            for (int i=0; i<plainTextData.Length; i++)
            {
                plainTextData[i] += checksum;
            }

            // Create a memory stream
            MemoryStream ms = new MemoryStream();

            // Create a CryptoStream using the memory stream and the CSP DES key
            CryptoStream encStream = new CryptoStream(ms, _algorithm.CreateEncryptor(), CryptoStreamMode.Write);

            // Write data
            encStream.WriteByte(checksum);
            encStream.Write(plainTextData, 0, plainTextData.Length);            
            encStream.Close();

            // Get an array of bytes that represents the memory stream
            byte[] cipherData = ms.ToArray();

            // Format as string
            string cipherText = Convert.ToBase64String(cipherData);

            // Close the memory stream
            ms.Close();

            return cipherText;
        }

        /// <summary>
        /// Decrypt a text string
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return "";
            }

            byte[] cipherData = Convert.FromBase64String(cipherText);
            
            // Create a memory stream
            MemoryStream ms = new MemoryStream(cipherData);

            // Create a CryptoStream using the memory stream and the CSP DES key
            CryptoStream encStream = new CryptoStream(ms, _algorithm.CreateDecryptor(), CryptoStreamMode.Read);

            MemoryStream decryptedStream = new MemoryStream();
            int nRead;
            byte[] buffer = new byte[1024];
            do
            {
                nRead = encStream.Read(buffer, 0, 1024);
                decryptedStream.Write(buffer, 0, nRead);
            }
            while (nRead == 1024);

            encStream.Close();
            ms.Close();

            byte[] decryptedData = decryptedStream.ToArray();
            int len = decryptedData.Length;

            string plainText = ""; 
            if (len > 0)
            {
                byte checksum = decryptedData[0];

                // Subtract checksum from each data byte
                for (int i = 1; i < len; i++)
                {
                    decryptedData[i] -= checksum;
                }

                // Check that the checksum is correct
                byte sum = 0;
                for (int i = 1; i < len; i++)
                {
                    sum += decryptedData[i];
                }

                if (sum == checksum)
                {
                    plainText = Encoding.UTF8.GetString(decryptedData, 1, len - 1);
                }
            }

            decryptedStream.Close();

            return plainText;
        }

        /// <summary>
        /// Write to file
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="filePath"></param>
        public void WriteEncryptedFile(string plainText, string filePath)
        {
            string cipherText = EncryptString(plainText);
            File.WriteAllText(filePath, cipherText);
        }

        /// <summary>
        /// Read file
        /// </summary>
        /// <param name="cypherText"></param>
        /// <returns></returns>
        public string ReadEncryptedFile(string filePath)
        {
            string cypherText = File.ReadAllText(filePath);
            return DecryptString(cypherText);
        }
    }
}
