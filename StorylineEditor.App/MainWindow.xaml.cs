/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.App.Config;
using StorylineEditor.Model;
using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using System.IO;
using System.Reflection;
using System.Windows;

namespace StorylineEditor.App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ICallbackContext
    {
        const string xmlFilter = "XML files (*.xml)|*.xml";
        const string configXmlPath = nameof(ConfigM) + ".xml";

        static MainWindow()
        {
            if (File.Exists(configXmlPath))
            {
                using (var fileStream = File.Open(configXmlPath, FileMode.Open))
                {
                    ConfigM.Config = SerializeService.Deserialize<ConfigM>(fileStream);
                }
            }
            else
            {
                ConfigM.InitDefaultConfig();

                SaveConfig();
            }
        }

        private static void SaveConfig()
        {
            using (var fileStream = File.Open(configXmlPath, FileMode.Create))
            {
                SerializeService.Serialize(fileStream, ConfigM.Config);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            StorylineM storylineModel = new StorylineM();
            storylineModel.characters.Add(new CharacterM() { id = CharacterM.PLAYER_ID, name = "Основной персонаж" });

            SetDataContext(new StorylineVM(storylineModel, this));

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            Title = string.Format("{0} [{1}]", assemblyName.Name, "new document");
        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ServiceFacade.FileService.OpenFile(xmlFilter, true))) OpenFile(ServiceFacade.FileService.Path);
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ServiceFacade.FileService.Path)) SaveFile(ServiceFacade.FileService.Path);

            else if (!string.IsNullOrEmpty(ServiceFacade.FileService.SaveFile(xmlFilter, true))) SaveFile(ServiceFacade.FileService.Path);
        }

        private void OpenFile(string path)
        {
            SetDataContext(null);

            StorylineM model = null;

            using (var fileStream = File.Open(path, FileMode.Open))
            {
                model = SerializeService.Deserialize<StorylineM>(fileStream);
            }

            if (model != null)
            {
                SetDataContext(new StorylineVM(model, this));

                var assemblyName = Assembly.GetExecutingAssembly().GetName();
                Title = string.Format("{0} [{1}]", assemblyName.Name, path);
            }
            else { } ////// TODO
        }

        private void SaveFile(string path)
        {
            if (DataContext is StorylineVM storylineVM)
            {
                using (var fileStream = File.Open(path, FileMode.Create))
                {
                    SerializeService.Serialize(fileStream, storylineVM.Model);

                    var assemblyName = Assembly.GetExecutingAssembly().GetName();
                    Title = string.Format("{0} [{1}]", assemblyName.Name, path);
                }
            }
        }

        private void SetDataContext(StorylineVM storylineViewModel)
        {
            ActiveContextService.ActiveStoryline = storylineViewModel;
            DataContext = storylineViewModel;
        }

        private void btn_Config_Click(object sender, RoutedEventArgs e)
        {
            new DlgWindow() {
                DataContext = new ConfigVM(ConfigM.Config, this),
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStyle = WindowStyle.ToolWindow,
                MinWidth = 256, MinHeight = 256, ResizeMode = ResizeMode.NoResize,
                Title = App.Current.Resources["String_Tag_Config_Title"]?.ToString()
            }.ShowDialog();
        }

        public void Callback(object viewModelObj, string propName)
        {
            if (propName == nameof(ICallbackContext))
            {

            }
            else
            {
                SaveConfig();
            }
        }
    }
}