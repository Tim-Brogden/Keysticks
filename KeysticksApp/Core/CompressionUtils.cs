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
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Keysticks.Core
{
    /// <summary>
    /// Compression utility class
    /// </summary>
    public class CompressionUtils
    {
        /// <summary>
        /// Compress a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Compress(string input)
        {
            byte[] inputBytes = Encoding.Unicode.GetBytes(input);
            byte[] outputBytes = Compress(inputBytes);
            string output = Convert.ToBase64String(outputBytes);

            return output;
        }

        /// <summary>
        /// Decompress a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string Decompress(string input)
        {
            byte[] inputBytes = Convert.FromBase64String(input);
            byte[] outputBytes = Decompress(inputBytes);
            string output = Encoding.Unicode.GetString(outputBytes);

            return output;
        }

        /// <summary>
        /// Compress a byte array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] Compress(byte[] input)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(input, 0, input.Length);
                }
                return memory.ToArray();
            }
        }

        /// <summary>
        /// Decompress a byte array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public byte[] Decompress(byte[] input)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(input), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count;
                    while ((count = stream.Read(buffer, 0, size)) > 0)
                    {
                        memory.Write(buffer, 0, count);
                    }
                    return memory.ToArray();
                }
            }
        }
    }
}
