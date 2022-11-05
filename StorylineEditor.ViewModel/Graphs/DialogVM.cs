/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.ViewModel.Common;
using System;
using System.Windows;

namespace StorylineEditor.ViewModel.Graphs
{
    public class DialogVM : BaseVM<DialogM>
    {
        public DialogVM(DialogM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class DialogEditorVM : Graph_BaseVM<DialogM>
    {
        public DialogEditorVM(DialogVM viewModel, ICallbackContext callbackContext, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
           Func<Notifier, ICallbackContext, Notifier> editorCreator, Func<Notifier, BaseM> modelExtractor, Type defaultNodeType) : base(viewModel.Model, callbackContext,
                modelCreator, viewModelCreator, editorCreator, modelExtractor, defaultNodeType)
        { }
    }
}