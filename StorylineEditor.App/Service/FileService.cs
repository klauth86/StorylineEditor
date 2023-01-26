/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using Microsoft.Win32;
using StorylineEditor.App.Service.StorageProvider;
using StorylineEditor.Model;
using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Config;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace StorylineEditor.App.Service
{
    public class FileService : IFileService
    {
        private const string _configXmlPath = "ConfigM.xml";
        
        private Dictionary<string, string> _cachedFiles = new Dictionary<string, string>();

        private Dictionary<byte, IStorageProvider> _storageProviders = new Dictionary<byte, IStorageProvider> { { STORAGE_TYPE.GOOGLE_DRIVE, new GoogleDrveStorageProvider() } };

        private WebClient _webClient = new WebClient();

        private string _nodeId;

        private string _fileUrl;

        private Action<string> _successCallback = null;

        private Action _failureCallback = null;

        public FileService()
        {
            _webClient.DownloadFileCompleted += OnDownloadFileCompleted;
        }

        public void Dispose()
        {
            _webClient.Dispose();

            foreach (var cachedFileEntry in _cachedFiles) ////// TODO Think
            {
                File.Delete(cachedFileEntry.Value);
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
                _successCallback(GetDownloadPath(_nodeId));
            }
        }

        private string GetDownloadPath(string nodeId) { return string.Format("{0}\\{1}", new FileInfo(Path).Directory.FullName, "test"); }

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

                string downloadUrl = _storageProviders[storageType].GetDownloadUrlFromBasicUrl(ref fileUrl);              
                _webClient.DownloadFileAsync(new Uri(downloadUrl), GetDownloadPath(_nodeId));
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