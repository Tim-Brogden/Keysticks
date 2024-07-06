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
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System;

namespace Keysticks.Sys
{
    /// <summary>
    /// Gets strings to use as 'unique' identifiers for the computer
    /// </summary>
    public class IdentityManager
    {
        /// <summary>
        /// Get the identity with the specified hash code
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public string GetIdentity(int hashCode)
        {
            string identity = "";

            List<string> identities = GetPossibleIdentityList();
            foreach (string str in identities)
            {
                if (str.GetHashCode() == hashCode)
                {
                    identity = str;
                    break;
                }
            }

            return identity;
        }

        /// <summary>
        /// Get an identity string
        /// </summary>
        /// <returns></returns>
        public string GetAnIdentity()
        {
            string identity = "";

            List<string> identities = GetPossibleIdentityList();
            if (identities.Count != 0)
            {
                identity = identities[0];
            }

            return identity;
        }

        /// <summary>
        /// Gets a list of strings that could be used as an identity
        /// </summary>
        /// <returns></returns>
        private List<string> GetPossibleIdentityList()
        {
            List<string> identities = new List<string>();

            // Use mac addresses as identity strings
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string addr = nic.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(addr))
                {
                    identities.Add(addr);
                }
            }

            // If the list is empty, use the PC name
            if (identities.Count == 0)
            {
                identities.Add(Environment.MachineName);
            }

            return identities;
        }
    }
}
