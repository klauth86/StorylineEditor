/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Graphs;
using StorylineEditor.ViewModel.Common;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class QuestVM : BaseVM<QuestM>
    {
        public QuestVM(QuestM model) : base(model) { }
    }

    public class QuestEditorVM : QuestVM
    {
        public QuestEditorVM(QuestM model) : base(model) { }

        private Notifier selection;
        public Notifier Selection
        {
            get => selection;
            set
            {
                if (selection != value)
                {
                    if (selection != null) selection.IsSelected = false;

                    selection = value;

                    if (selection != null) selection.IsSelected = true;

                    Notify(nameof(Selection));
                    Notify(nameof(SelectionEditor));

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Notifier SelectionEditor => selection != null ? _editorCreator(selection) : null;

        private Notifier _editorCreator(Notifier viewModel)
        {
            if (viewModel is Node_Journal_StepVM nodeJournalStepVM)
            {
                return new Node_Journal_StepEditorVM(nodeJournalStepVM.Model);
            }

            if (viewModel is Node_Journal_AlternativeVM nodeJournalAlternativeVM)
            {
                return new Node_Journal_AlternativeEditorVM(nodeJournalAlternativeVM.Model);
            }

            return null;
        }
    }
}