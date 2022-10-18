/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.ViewModel.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class FolderProxyM : FolderM { }

    public class CollectionVM : Collection_BaseVM<List<BaseM>>
    {
        public CollectionVM(List<BaseM> inModel, Func<Type, BaseM> modelCreator, Func<BaseM, Notifier> viewModelCreator,
            Func<Notifier, Notifier> editorCreator, Func<Notifier, BaseM> modelExtractor, Action<Notifier> viewModelInformer) : base(inModel, modelCreator, viewModelCreator,
                editorCreator, modelExtractor)
        {            
            _viewModelInformer = viewModelInformer ?? throw new ArgumentNullException(nameof(viewModelInformer));

            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsVMs);

            if (view != null)
            {
                view.SortDescriptions.Add(new SortDescription(nameof(IsFolder), ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Ascending));
            }

            Context = new ObservableCollection<FolderM>();
            Context.Add(new FolderProxyM());
            Context.Add(new FolderM() { name = "root", content = inModel });

            foreach (var model in Model) { Add(null, _viewModelCreator(model)); }
        }

        private ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<Notifier>((viewModel) => _viewModelInformer(viewModel), (viewModel) => viewModel != null));

        private ICommand upContextCommand;
        public ICommand UpContextCommand => upContextCommand ?? (upContextCommand = new RelayCommand(() =>
        {
            Context.RemoveAt(Context.Count - 1);

            ItemsVMs.Clear();
            foreach (var model in Context.Last().content) { Add(null, _viewModelCreator(model)); }

            CommandManager.InvalidateRequerySuggested();

        }, () => Context.Count > 2));

        private ICommand setContextCommand;
        public ICommand SetContextCommand => setContextCommand ?? (setContextCommand = new RelayCommand<FolderM>((folderM) =>
        {
            if (Context.Count > 0 && Context[Context.Count - 1] == folderM) return;

            int cutIndex = -1;

            foreach (var contextEntry in Context)
            {
                cutIndex++;
                if (contextEntry == folderM) break;
            }

            if (cutIndex >= 0 && cutIndex + 1 < Context.Count)
            {
                for (int i = cutIndex + 1; i < Context.Count; i++)
                {
                    Context.RemoveAt(i);
                }
            }
            else
            {
                Context.Add(folderM);
            }

            Selection = null;

            ItemsVMs.Clear();
            foreach (var model in Context.Last().content) { Add(null, _viewModelCreator(model)); }

            CommandManager.InvalidateRequerySuggested();

        }, (folderM) => folderM != null));



        private readonly Action<Notifier> _viewModelInformer;



        public ObservableCollection<FolderM> Context { get; }
        public override IList GetContext(BaseM model) { return Context.Last().content; }
    }
}