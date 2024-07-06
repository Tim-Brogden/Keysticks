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
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Keysticks.Core
{
    /// <summary>
    /// Stores a list of items with IDs and names
    /// </summary>
    public class NamedItemList : ObservableCollection<NamedItem>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NamedItemList()
        {
        }
       
        /// <summary>
        /// Get the item with a particular ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public NamedItem GetItemByID(int id)
        {
            NamedItem matchedItem = null;
            foreach (NamedItem item in this)
            {
                if (item.ID == id)
                {
                    matchedItem = item;
                    break;
                }
            }

            return matchedItem;
        }

        /// <summary>
        /// Find the first item with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NamedItem GetItemByName(string name)
        {
            NamedItem matchedItem = null;
            foreach (NamedItem item in this)
            {
                if (item.Name.Equals(name))
                {
                    matchedItem = item;
                    break;
                }
            }

            return matchedItem;
        }

        /// <summary>
        /// Insert an item after the specified item
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newItem"></param>
        public void InsertAfterID(int id, NamedItem newItem)
        {
            for (int i=0; i<Count; i++)
            {
                if (this[i].ID == id)
                {
                    Insert(i+1, newItem);
                    break;
                }
            }
        }

        /// <summary>
        /// Insert an item in the right position according to its ID
        /// </summary>
        /// <param name="newItem"></param>
        public void OrderedInsert(NamedItem newItem)
        {
            int i = 0;
            while (i < Count && this[i].ID <= newItem.ID)
            {
                i++;
            }
            Insert(i, newItem);
        }

        /// <summary>
        /// Find the ID of the next item after the item with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int FindNextID(int id)
        {
            int nextID = id;

            // Find the specified ID in the list
            bool foundItem = false;
            foreach (NamedItem details in this)
            {
                if (foundItem)
                {
                    // Looking for the next item with a positive ID i.e. don't return default item
                    if (details.ID > -1)
                    {
                        nextID = details.ID;
                        break;
                    }
                }
                else if (details.ID == id)
                {
                    // Found specified ID
                    foundItem = true;
                }
                else if (details.ID > 0 && nextID == id)
                {
                    // Store the first positive ID in the list in case we need to wrap round
                    nextID = details.ID;
                }
            }

            return nextID;
        }

        /// <summary>
        /// Find the ID of the item before the item with the specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int FindPreviousID(int id)
        {
            int previousID = id;

            foreach (NamedItem details in this)
            {
                if (details.ID == id)
                {
                    // Found specified ID
                    // If we have found a positive previous ID then break,
                    // otherwise continue so that we find the last positive ID in the list (i.e. wrap round)
                    if (previousID != id)
                    {
                        break;
                    }
                }
                else if (details.ID > -1)
                {
                    // Found a positive ID so store
                    previousID = details.ID;
                }
            }

            return previousID;
        }

        /// <summary>
        /// Get a new ID
        /// </summary>
        /// <param name="startAt"></param>
        /// <returns></returns>
        public int GetFirstUnusedID(int startAt)
        {
            bool exists;
            int id = startAt;
            do
            {
                exists = false;
                foreach (NamedItem item in this)
                {
                    if (item.ID == id)
                    {
                        exists = true;
                        id++;
                        break;
                    }
                }
            }
            while (exists);

            return id;
        }

        /// <summary>
        /// Get a new name
        /// </summary>
        /// <param name="stem"></param>
        /// <returns></returns>
        public string GetFirstUnusedName(string baseName, bool allowStemAsName, bool spaceBeforeNumber, int startFromNumber)
        {
            int count = startFromNumber;

            // Remove any numbers at the end
            Regex regex = new Regex("[ ]*[0-9]+$");
            string stem = regex.Replace(baseName, "");            

            string name;
            if (allowStemAsName)
            {
                if (stem != "")
                {
                    name = stem;
                }
                else
                {
                    name = baseName;
                }
            }
            else
            {
                name = string.Format("{0}{1}{2}",
                                        stem,
                                        spaceBeforeNumber ? " " : "",
                                        count.ToString(System.Globalization.CultureInfo.InvariantCulture)).TrimStart();
            }

            while (GetItemByName(name) != null)
            {
                count++;
                name = string.Format("{0}{1}{2}",
                                        stem,
                                        spaceBeforeNumber ? " " : "",
                                        count.ToString(System.Globalization.CultureInfo.InvariantCulture)).TrimStart();
            }

            return name;
        }

        /// <summary>
        /// Get string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool isFirst = true;
            StringBuilder sb = new StringBuilder();
            foreach (NamedItem item in this)
            {
                if (!isFirst)
                {
                    sb.Append(",");
                }
                if (item != null)
                {
                    sb.Append(item.ToString());
                }
                isFirst = false;
            }

            return sb.ToString();
        }
    }
}
