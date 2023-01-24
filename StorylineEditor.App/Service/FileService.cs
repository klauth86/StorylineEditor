/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Win32;
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
        
        private Dictionary<string, string> cachedFiles = new Dictionary<string, string>();

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
        public void GetFileFromStorage(byte fileStorageType, string fileHttpRef, Action<string> successCallback, Action failureCallback)
        {
            if (fileStorageType == STORAGE_TYPE.GOOGLE_DRIVE)
            {
                GetFileFromGoogleDrive(fileHttpRef, successCallback, failureCallback);
            }
            else
            {
                failureCallback();
            }
        }
        private void GetFileFromGoogleDrive(string fileHttpRef, Action<string> successCallback, Action failureCallback)
        {
            GoogleCredential credential = GoogleCredential.GetApplicationDefault().CreateScoped(DriveService.Scope.Drive);

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "" ////// TODO
            });

            var fileResource = service.Files.Get(fileHttpRef);
            var file = fileResource.Execute();

            MemoryStream memoryStream = new MemoryStream();

            fileResource.MediaDownloader.ProgressChanged += (progress) =>
            {
                switch (progress.Status)
                {
                    case Google.Apis.Download.DownloadStatus.Completed:
                        {
                            string cachedFilePath = Environment.CurrentDirectory + "\\" + file.Name;

                            using (var fileStream = OpenFile(cachedFilePath, FileMode.CreateNew))
                            {
                                memoryStream.Position = 0;
                                memoryStream.CopyTo(fileStream);
                            }

                            cachedFiles.Add(fileHttpRef, cachedFilePath);
                            successCallback(cachedFilePath);
                        }
                        break;

                    case Google.Apis.Download.DownloadStatus.Failed:
                        {
                            failureCallback();
                        }
                        break;

                    default:
                        break;
                }
            };

            fileResource.DownloadAsync(memoryStream);
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