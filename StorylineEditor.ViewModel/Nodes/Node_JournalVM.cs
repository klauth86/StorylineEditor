﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Nodes;

namespace StorylineEditor.ViewModel
{
    public abstract class Node_JournalVM<T> : Node_BaseVM<T> where T : Node_JournalM
    {
        public Node_JournalVM(T model) : base(model) { }

        public string Result
        {
            get => Model.result;
            set
            {
                if (Model.result != value)
                {
                    Model.result = value;
                    OnModelChanged(Model, nameof(Result));
                }
            }
        }
    }

    public class Node_Journal_StepVM : Node_JournalVM<Node_StepM>
    {
        public Node_Journal_StepVM(Node_StepM model) : base(model) { }
    }

    public class Node_Journal_StepEditorVM : Node_Journal_StepVM
    {
        public Node_Journal_StepEditorVM(Node_StepM model) : base(model) { }
    }

    public class Node_Journal_AlternativeVM : Node_JournalVM<Node_AlternativeM>
    {
        public Node_Journal_AlternativeVM(Node_AlternativeM model) : base(model) { }
    }

    public class Node_Journal_AlternativeEditorVM : Node_Journal_AlternativeVM
    {
        public Node_Journal_AlternativeEditorVM(Node_AlternativeM model) : base(model) { }
    }
}