/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using Microsoft.Win32;

namespace StorylineEditor.FileDialog
{
    public class DefaultDialogService : IDialogService
    {
        public static void Init() { DialogService = new DefaultDialogService(); }

        public override string OpenFileDialog(string filter, bool refreshPath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;

            if (openFileDialog.ShowDialog() == true)
            {
                return refreshPath ? Path = openFileDialog.FileName : openFileDialog.FileName;
            }

            return null;
        }

        public override string SaveFileDialog(string filter, bool refreshPath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filter;

            if (saveFileDialog.ShowDialog() == true)
            {
                return refreshPath ? Path = saveFileDialog.FileName : saveFileDialog.FileName;
            }

            return null;
        }
    }
}