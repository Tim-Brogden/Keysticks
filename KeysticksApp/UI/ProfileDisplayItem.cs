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
using System.Text.RegularExpressions;
using System.Globalization;
using System.Xml;
using Keysticks.Config;
using Keysticks.Core;

namespace Keysticks.UI
{
    /// <summary>
    /// Stores meta data for a profile
    /// </summary>
    public class ProfileDisplayItem : NamedItem
    {
        // Fields
        private EProfileStatus _status = EProfileStatus.Local;
        private DateTime _fileModifiedDate = DateTime.MinValue;
        private DateTime _lastDownloadModifiedDate = DateTime.MinValue;
        private MetaDataTable _metaData = new MetaDataTable();

        // Derived from meta data
        private string _shortName = "";
        private string _profileRef = "-";
        private string _addedBy = "";
        private DateTime _lastModifiedDate = DateTime.MinValue;
        private int _numDownloads = 0;

        // Properties
        public MetaDataTable MetaData { get { return _metaData; } }
        public EProfileStatus Status
        { 
            get { return _status; } 
            set 
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyPropertyChanged("Status");
                }
            } 
        }
        public DateTime FileModifiedDate { get { return _fileModifiedDate; } }
        public DateTime LastDownloadModifiedDate { get { return _lastDownloadModifiedDate; } set { _lastDownloadModifiedDate = value; } }
        
        // Properties derived from meta data
        public string ShortName
        {
            get { return _shortName; }
            private set
            {
                if (_shortName != value)
                {
                    _shortName = value;
                    NotifyPropertyChanged("ShortName");
                }
            }
        }
        public string ProfileRef
        {
            get { return _profileRef; }
            private set
            {
                if (_profileRef != value)
                {
                    _profileRef = value;
                    NotifyPropertyChanged("ProfileRef");
                }
            }
        }
        public string AddedBy
        {
            get { return _addedBy; }
            private set
            {
                if (_addedBy != value)
                {
                    _addedBy = value;
                    NotifyPropertyChanged("AddedBy");
                }
            }
        }
        public DateTime LastModifiedDate
        {
            get { return _lastModifiedDate; }
            private set
            {
                if (_lastModifiedDate != value)
                {
                    _lastModifiedDate = value;
                    NotifyPropertyChanged("LastModifiedDate");
                }
            }
        }
        public int Downloads
        {
            get { return _numDownloads; }
            set
            {
                if (_numDownloads != value)
                {
                    _numDownloads = value;
                    _metaData.SetIntVal(EMetaDataItem.Downloads.ToString(), value);
                    NotifyPropertyChanged("Downloads");
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProfileDisplayItem()
        {
        }

        /// <summary>
        /// Construct from full name (without file extension)
        /// E.g. 'P01 - My profile', or 'My profile'
        /// </summary>
        /// <param name="name"></param>
        public ProfileDisplayItem(string name, DateTime fileModifiedDate)
        {
            // Get the ID and name parts
            Name = name;
            Regex nameFormatRegex = new Regex("^[Pp]([0-9]+)[ -]+(.+)"); 
            Match match = nameFormatRegex.Match(name);
            if (match.Success && match.Groups.Count > 2)
            {
                // Has 'P' number
                ID = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                ShortName = match.Groups[2].Value;
            }
            else
            {
                // No 'P' number
                ID = 0;
                ShortName = name;
            }
            _fileModifiedDate = fileModifiedDate;

            // Update derived data
            ProfileRef = ID > 0 ? string.Format("P{0:00}", ID) : "-";
        }

        /// <summary>
        /// Construct from meta data
        /// </summary>
        /// <param name="profileMetaData"></param>
        public ProfileDisplayItem(MetaDataTable profileMetaData)
        {
            _metaData = profileMetaData;
            MetaDataChanged();
        }

        /// <summary>
        /// Read from Xml
        /// </summary>
        /// <param name="parentElement"></param>
        public override void FromXml(XmlElement parentElement)
        {
            base.FromXml(parentElement);

            // Status
            _status = (EProfileStatus)Enum.Parse(typeof(EProfileStatus), parentElement.GetAttribute("status"));   
         
            // Dates
            _fileModifiedDate = DateTime.Parse(parentElement.GetAttribute("filemodifieddate"), CultureInfo.InvariantCulture);
            _lastDownloadModifiedDate = DateTime.Parse(parentElement.GetAttribute("lastdownloadmodifieddate"), CultureInfo.InvariantCulture);

            // Meta data
            XmlElement metaDataElement = (XmlElement)parentElement.SelectSingleNode("metadata");
            if (metaDataElement != null)
            {
                _metaData.FromXml(metaDataElement);
                MetaDataChanged();
            }
        }

        /// <summary>
        /// Write to Xml
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="doc"></param>
        public override void ToXml(XmlElement parentElement, XmlDocument doc)
        {
            base.ToXml(parentElement, doc);

            // Status
            parentElement.SetAttribute("status", _status.ToString());

            // Modified date of last download
            parentElement.SetAttribute("filemodifieddate", _fileModifiedDate.ToString(CultureInfo.InvariantCulture));
            parentElement.SetAttribute("lastdownloadmodifieddate", _lastDownloadModifiedDate.ToString(CultureInfo.InvariantCulture));

            XmlElement metaDataElement = doc.CreateElement("metadata");
            _metaData.ToXml(metaDataElement, doc);
            parentElement.AppendChild(metaDataElement);
        }

        /// <summary>
        /// Merge in the meta data updates
        /// </summary>
        public void UpdateMetaData(MetaDataTable metaData)
        {
            Dictionary<string, string>.Enumerator eItem = metaData.GetEnumerator();
            while (eItem.MoveNext())
            {
                _metaData.SetStringVal(eItem.Current.Key, eItem.Current.Value);
            }

            MetaDataChanged();
        }

        /// <summary>
        /// Update properties when meta data changes
        /// </summary>
        private void MetaDataChanged()
        {
            ID = _metaData.GetIntVal(EMetaDataItem.ID.ToString(), 0);
            ShortName = _metaData.GetStringVal(EMetaDataItem.ShortName.ToString(), "");

            Name = ID > 0 ? string.Format("P{0:00} - {1}", ID, ShortName) : ShortName;
            ProfileRef = ID > 0 ? string.Format("P{0:00}", ID) : "-";
            AddedBy = _metaData.GetStringVal(EMetaDataItem.AddedBy.ToString(), "");

            string dateStr = _metaData.GetStringVal(EMetaDataItem.LastModifiedDate.ToString(), null);
            if (dateStr != null)
            {
                LastModifiedDate = DateTime.Parse(dateStr, CultureInfo.InvariantCulture);
            }

            Downloads = _metaData.GetIntVal(EMetaDataItem.Downloads.ToString(), 0);
        }
    }
}
