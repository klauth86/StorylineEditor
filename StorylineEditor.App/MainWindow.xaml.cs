/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.ViewModel;
using System.IO;
using System.Reflection;
using System.Windows;

namespace StorylineEditor.App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string xmlFilter = "XML files (*.xml)|*.xml";

        public MainWindow()
        {
            InitializeComponent();

            StorylineM storylineModel = new StorylineM();
            storylineModel.characters.Add(new CharacterM() { id = CharacterM.PLAYER_ID, name = "Основной персонаж" });
            
            SetDataContext(new StorylineVM(storylineModel));

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
                SetDataContext(new StorylineVM(model));

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
    }
}