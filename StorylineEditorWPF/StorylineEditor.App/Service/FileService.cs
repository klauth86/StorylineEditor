/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.Win32;
using StorylineEditor.Model;
using StorylineEditor.Service.StorageProvider;
using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace StorylineEditor.Service
{
    public class FileService : IFileService
    {
        private Regex _fileNameRegex = new Regex("filename=\"([^\"]*)\";", RegexOptions.IgnoreCase);

        private const string _configXmlPath = "ConfigM.xml";
        
        private Dictionary<string, string> _cachedFiles = new Dictionary<string, string>();

        private Dictionary<byte, IStorageProvider> _storageProviders = new Dictionary<byte, IStorageProvider> { { STORAGE_TYPE.GOOGLE_DRIVE, new GoogleDrveStorageProvider() } };

        private WebClient _webClient;

        private string _nodeId;

        private string _fileUrl;

        private Uri _downloadUri;

        private string _downloadPath;

        private Action<string> _successCallback = null;

        private Action _failureCallback = null;

        public FileService()
        {
            _webClient = new WebClient();

            _webClient.DownloadDataCompleted += OnDownloadDataCompleted;
            _webClient.DownloadFileCompleted += OnDownloadFileCompleted;
        }

        public void Dispose()
        {
            _webClient.DownloadFileCompleted -= OnDownloadFileCompleted;
            _webClient.DownloadDataCompleted -= OnDownloadDataCompleted;

            _webClient.Dispose();

            foreach (var cachedFileEntry in _cachedFiles) ////// TODO Think
            {
                File.Delete(cachedFileEntry.Value);
            }
        }

        private void OnDownloadDataCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
            }
            else if (e.Error != null)
            {
                _failureCallback();
            }
            else if (!string.IsNullOrEmpty(_webClient.ResponseHeaders["Content-Disposition"]))
            {
                string contentDisposition = _webClient.ResponseHeaders["Content-Disposition"];

                Match match = _fileNameRegex.Match(contentDisposition);

                if (match.Success && match.Groups.Count > 1)
                {
                    _downloadPath = GetDownloadPath(match.Groups[1].ToString());
                    _webClient.DownloadFileAsync(_downloadUri, _downloadPath);
                }
                else
                {
                    _failureCallback();
                }
            }
            else
            {
                _failureCallback();
            }
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
            }
            else if (e.Error != null)
            {
                _failureCallback();
            }
            else
            {
                _cachedFiles.Add(_fileUrl, _downloadPath);
                _successCallback(_downloadPath);
            }
        }

        private string GetDownloadPath(string fileName) { return string.Format("{0}\\{1}", new FileInfo(Path).Directory.FullName, fileName); }

        // Open Save logic
        public string Path { get; protected set; }
        public string OpenFile(string filter, bool refreshPath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;

            if (openFileDialog.ShowDialog() == true)
            {
                return refreshPath ? Path = openFileDialog.FileName : openFileDialog.FileName;
            }

            return null;
        }
        public string SaveFile(string filter, bool refreshPath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;

            if (saveFileDialog.ShowDialog() == true)
            {
                return refreshPath ? Path = saveFileDialog.FileName : saveFileDialog.FileName;
            }

            return null;
        }

        // File Storage logic
        public void GetFileFromStorage(string nodeId, byte storageType, string fileUrl, Action<string> successCallback, Action failureCallback)
        {
            if (_cachedFiles.ContainsKey(fileUrl) && File.Exists(_cachedFiles[fileUrl]))
            {
                successCallback(_cachedFiles[fileUrl]);
            }
            else if (_storageProviders.ContainsKey(storageType))
            {
                _nodeId = nodeId;
                _fileUrl = fileUrl;
                _successCallback = successCallback;
                _failureCallback = failureCallback;

                _downloadUri = new Uri(_storageProviders[storageType].GetDownloadUrlFromBasicUrl(ref fileUrl));
                _webClient.DownloadDataAsync(_downloadUri);
            }
            else
            {
                failureCallback();
            }
        }

        // Config logic
        public void LoadConfig()
        {
            if (File.Exists(_configXmlPath))
            {
                using (var fileStream = OpenFile(_configXmlPath, FileMode.Open))
                {
                    ConfigM.Config = ActiveContext.SerializationService.Deserialize<ConfigM>(fileStream);
                }

                if (ConfigM.CompleteToDefaultConfig())
                {
                    SaveConfig();
                }
            }
            else
            {
                ConfigM.InitDefaultConfig();

                SaveConfig();
            }
        }
        public void SaveConfig()
        {
            using (var fileStream = OpenFile(_configXmlPath, FileMode.Create))
            {
                ActiveContext.SerializationService.Serialize(fileStream, ConfigM.Config);
            }
        }

        private FileStream OpenFile(string path, FileMode mode) { return File.Open(path, mode); }
    }
}