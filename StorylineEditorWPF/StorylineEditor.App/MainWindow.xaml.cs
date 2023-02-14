/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.App.Config;
using StorylineEditor.App.Service;
using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Config;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;

namespace StorylineEditor.App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDialogService
    {
        public const string XmlFilter = "XML files (*.xml)|*.xml";

        static MainWindow()
        {
            ActiveContext.SerializationService = new SerializationService();
            ActiveContext.TaskService = new TaskService();
            ActiveContext.FileService = new FileService();
            ActiveContext.SoundPlayerService = new SoundPlayerService();

            ActiveContext.FileService.LoadConfig();
        }

        public MainWindow()
        {
            InitializeComponent();

            ActiveContext.DialogService = this;

            StorylineM storylineModel = new StorylineM();
            storylineModel.characters.Add(new CharacterM() { id = CharacterM.PLAYER_ID, name = "Основной персонаж" });

            SetDataContext(new StorylineVM(storylineModel));

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            Title = string.Format("{0} [{1}]", assemblyName.Name, "new document");
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ActiveContext.SoundPlayerService.Dispose();
            ActiveContext.FileService.Dispose();
            ActiveContext.TaskService.Dispose();

            base.OnClosing(e);
        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ActiveContext.FileService.OpenFile(XmlFilter, true))) OpenFile(ActiveContext.FileService.Path);
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ActiveContext.FileService.Path)) SaveFile(ActiveContext.FileService.Path);

            else if (!string.IsNullOrEmpty(ActiveContext.FileService.SaveFile(XmlFilter, true))) SaveFile(ActiveContext.FileService.Path);
        }

        private void OpenFile(string path)
        {
            SetDataContext(null);

            if (File.Exists(path))
            {
                using (FileStream fileStream = File.Open(path, FileMode.Open))
                {
                    try
                    {
                        var model = ActiveContext.SerializationService.Deserialize<StorylineM>(fileStream);
                        ValidateModel(model);

                        SetDataContext(new StorylineVM(model));

                        var assemblyName = Assembly.GetExecutingAssembly().GetName();
                        Title = string.Format("{0} [{1}]", assemblyName.Name, path);
                    }
                    catch (InvalidDataException idExc)
                    {
                        MessageBox.Show(idExc.Message, "Error", MessageBoxButton.OK);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Can not open file!", "Error", MessageBoxButton.OK);
                    }
                }
            }
        }

        private void SaveFile(string path)
        {
            if (DataContext is StorylineVM storylineVM)
            {
                using (var fileStream = File.Open(path, FileMode.Create))
                {
                    Dictionary<string, string> namesMapping = new Dictionary<string, string>();

                    // In fact we dont store any useful info in dialogs and replicas nodes name field
                    // It is generated during work and used in combobox search as string representation of item
                    // So no need to save it - just clear them before save and fix up after save

                    ClearUpNodesNames(storylineVM.Model.dialogs, namesMapping);
                    ClearUpNodesNames(storylineVM.Model.replicas, namesMapping);

                    ActiveContext.SerializationService.Serialize(fileStream, storylineVM.Model);

                    FixUpNodesNames(storylineVM.Model.replicas, namesMapping);
                    FixUpNodesNames(storylineVM.Model.dialogs, namesMapping);

                    var assemblyName = Assembly.GetExecutingAssembly().GetName();
                    Title = string.Format("{0} [{1}]", assemblyName.Name, path);
                }
            }
        }

        private void ClearUpNodesNames(List<BaseM> items, Dictionary<string, string> namesMapping)
        {
            foreach (var item in items)
            {
                if (item is FolderM folder)
                {
                    ClearUpNodesNames(folder.content, namesMapping);
                }
                else if (item is GraphM graph)
                {
                    foreach (var node in graph.nodes)
                    {
                        namesMapping.Add(node.id, node.name);
                        node.name = null;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
        }

        private void FixUpNodesNames(List<BaseM> items, Dictionary<string, string> namesMapping)
        {
            foreach (var item in items)
            {
                if (item is FolderM folder)
                {
                    FixUpNodesNames(folder.content, namesMapping);
                }
                else if (item is GraphM graph)
                {
                    foreach (var node in graph.nodes)
                    {
                        node.name = namesMapping[node.id];
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
        }

        private void FillNodesNames(List<BaseM> items)
        {
            foreach (var item in items)
            {
                if (item is FolderM folder)
                {
                    FillNodesNames(folder.content);
                }
                else if (item is GraphM graph)
                {
                    foreach (var node in graph.nodes)
                    {
                        if (node is Node_RegularM regularNode)
                        {
                            node.name = string.Format("[{0}]: {1}", ActiveContext.GetCharacter(regularNode.characterId)?.name ?? "???", node.rtDescription.ToString()); ////// TODO DUPLICATION
                        }
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
        }

        private void SetDataContext(StorylineVM storylineVM)
        {
            ActiveContext.Storyline = storylineVM;
            DataContext = storylineVM;
            
            if (storylineVM != null)
            {
                FillNodesNames(storylineVM.Model.dialogs);
                FillNodesNames(storylineVM.Model.replicas);
            }
        }

        private void btn_Config_Click(object sender, RoutedEventArgs e)
        {
            new DlgWindow()
            {
                Owner = this,
                DataContext = new ConfigVM(ConfigM.Config),
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStyle = WindowStyle.ToolWindow,
                MinWidth = 256,
                ResizeMode = ResizeMode.NoResize,
                Title = App.Current.Resources["String_Tag_Config_Title"]?.ToString()
            }.ShowDialog();
        }

        private void IterateThrough(List<BaseM> modelCollection, Dictionary<string, int> idsDictionary, List<string> invalidLinks)
        {
            foreach (var itemModel in modelCollection)
            {
                if (!idsDictionary.ContainsKey(itemModel.id)) idsDictionary.Add(itemModel.id, 0);
                idsDictionary[itemModel.id]++;

                if (itemModel is FolderM folderModel)
                {
                    IterateThrough(folderModel.content, idsDictionary, invalidLinks);
                }
                else if (itemModel is GraphM graphModel)
                {
                    foreach (var linkModel in graphModel.links)
                    {
                        if (!idsDictionary.ContainsKey(linkModel.id)) idsDictionary.Add(linkModel.id, 0);
                        idsDictionary[linkModel.id]++;

                        if (string.IsNullOrEmpty(linkModel.fromNodeId) ||
                            string.IsNullOrEmpty(linkModel.toNodeId) ||
                            linkModel.fromNodeId == linkModel.toNodeId)
                        {
                            invalidLinks.Add(linkModel.id);
                        }
                        else
                        {
                            string linkModelSignature = linkModel.fromNodeId + " --->>> " + linkModel.toNodeId;
                            if (!idsDictionary.ContainsKey(linkModelSignature)) idsDictionary.Add(linkModelSignature, 0);
                            idsDictionary[linkModelSignature]++;
                        }
                    }

                    foreach (var nodeModel in graphModel.nodes)
                    {
                        if (!idsDictionary.ContainsKey(nodeModel.id)) idsDictionary.Add(nodeModel.id, 0);
                        idsDictionary[nodeModel.id]++;

                        if (nodeModel is Node_InteractiveM interactiveNodeModel)
                        {
                            foreach (var predicateModel in interactiveNodeModel.predicates)
                            {
                                if (!idsDictionary.ContainsKey(predicateModel.id)) idsDictionary.Add(predicateModel.id, 0);
                                idsDictionary[predicateModel.id]++;
                            }

                            foreach (var gameEventsModel in interactiveNodeModel.gameEvents)
                            {
                                if (!idsDictionary.ContainsKey(gameEventsModel.id)) idsDictionary.Add(gameEventsModel.id, 0);
                                idsDictionary[gameEventsModel.id]++;
                            }
                        }
                    }
                }
            }
        }

        private void ValidateModel(StorylineM storylineModel)
        {
            Dictionary<string, int> idsDictionary = new Dictionary<string, int>();
            idsDictionary.Add(storylineModel.id, 1);

            List<string> invalidLinks = new List<string>();

            IterateThrough(storylineModel.locations, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.characters, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.items, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.actors, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.journal, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.dialogs, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.replicas, idsDictionary, invalidLinks);

            List<string> duplicateIds = new List<string>();

            foreach (var idsEntry in idsDictionary)
            {
                if (idsEntry.Value > 1) duplicateIds.Add(idsEntry.Key);
            }

            string invalidLinksStr = string.Join(Environment.NewLine, invalidLinks);
            string duplicateIdsStr = string.Join(Environment.NewLine, duplicateIds);

            if (duplicateIds.Count + invalidLinks.Count > 0)
            {
                throw new InvalidDataException("Data validation fault! Model contains duplicates or invalid elements!");
            }
        }

        public void ShowDialog(object dataContext)
        {
            if (dataContext is HistoryVM)
            {
                new DlgWindow()
                {
                    Owner = this,
                    DataContext = dataContext,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStyle = WindowStyle.ToolWindow,
                    MinWidth = 256,
                    ResizeMode = ResizeMode.NoResize,
                    Title = App.Current.Resources["String_Tag_Player_Title"]?.ToString()
                }.ShowDialog();
            }
            else
            {
                new DlgWindow()
                {
                    Owner = this,
                    DataContext = dataContext,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStyle = WindowStyle.ToolWindow,
                    MinWidth = 256,
                    ResizeMode = ResizeMode.NoResize,
                    ContentTemplate = App.Current.Resources["DT_Graph_Stats"] as DataTemplate
                }.Show();
            }
        }
    }
}