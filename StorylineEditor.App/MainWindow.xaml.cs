/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
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
            ActiveContext.FlowDocumentService = new FlowDocumentService();
            ActiveContext.TaskService = new TaskService();
            ActiveContext.FileService = new FileService();

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

            FileStream fileStream = null;

            try
            {
                fileStream = File.Open(path, FileMode.Open);
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
            finally
            {
                fileStream?.Dispose();
            }
        }

        private void SaveFile(string path)
        {
            if (DataContext is StorylineVM storylineVM)
            {
                using (var fileStream = File.Open(path, FileMode.Create))
                {
                    ActiveContext.SerializationService.Serialize(fileStream, storylineVM.Model);

                    var assemblyName = Assembly.GetExecutingAssembly().GetName();
                    Title = string.Format("{0} [{1}]", assemblyName.Name, path);
                }
            }
        }

        private void SetDataContext(StorylineVM storylineViewModel)
        {
            ActiveContext.Storyline = storylineViewModel;
            DataContext = storylineViewModel;
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