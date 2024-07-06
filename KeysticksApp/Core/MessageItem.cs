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
using System.Globalization;
using System.IO;

namespace Keysticks.Core
{
    public class MessageItem
    {
        public DateTime Time { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string Details { get; set; }

        public MessageItem(DateTime time,
            string type,
            string text,
            string details)
        {
            Time = time;
            Type = type;
            Text = text;
            Details = details;
        }

        public MessageItem()
        {
        }

        /// <summary>
        /// Create from error message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public MessageItem(string message, Exception ex)
        {
            Time = DateTime.UtcNow;
            Type = "Error";
            Text = message;
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                sb.Append("Details: ");
                sb.Append(ex.Message);
                if (ex.StackTrace != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("Stack trace: ");
                    sb.Append(ex.StackTrace);
                }
                ex = ex.InnerException;
            }
            Details = sb.ToString();
        }

        /// <summary>
        /// Parse from string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool TryParse(string input)
        {
            bool success = false;

            if (!string.IsNullOrEmpty(input))
            {
                string line;
                StringReader sr = new StringReader(input);
                if ((line = sr.ReadLine()) != null)
                {
                    int index = line.LastIndexOf(':');
                    if (index > 0 && index < line.Length - 1)
                    {
                        string timeStr = line.Substring(0, index);
                        DateTime timeVal;
                        if (DateTime.TryParse(timeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out timeVal))
                        {
                            Time = timeVal;
                            Type = line.Substring(index + 2);
                            if ((line = sr.ReadLine()) != null)
                            {
                                Text = line;
                                Details = sr.ReadToEnd().Trim();
                                success = true;
                            }
                        }
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Time.ToString(System.Globalization.CultureInfo.InvariantCulture));
            sb.Append(": ");
            sb.AppendLine(Type);
            sb.AppendLine(Text);
            sb.AppendLine(Details);

            return sb.ToString();
        }
    }

}
