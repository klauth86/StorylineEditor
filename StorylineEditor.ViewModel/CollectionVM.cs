﻿/*
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class FolderActionM : FolderM { }

    public class CutEntryVM : Notifier
    {
        public BaseM Model { get; set; }
        public Notifier ViewModel { get; set; }
        public FolderM Context { get; set; }
    }

    public class CollectionVM : BaseVM<List<BaseM>>
    {
        public CollectionVM(List<BaseM> model, Func<bool, BaseM> modelCreator, Func<BaseM, Notifier> viewModelCreator,
            Func<Notifier, Notifier> editorCreator, Func<Notifier, BaseM> modelExtractor, Action<Notifier> viewModelInformer) : base(model)
        {
            _modelCreator = modelCreator ?? throw new ArgumentNullException(nameof(modelCreator));
            _viewModelCreator = viewModelCreator ?? throw new ArgumentNullException(nameof(viewModelCreator));
            _editorCreator = editorCreator ?? throw new ArgumentNullException(nameof(editorCreator));
            
            _modelExtractor = modelExtractor ?? throw new ArgumentNullException(nameof(modelExtractor));
            
            _viewModelInformer = viewModelInformer ?? throw new ArgumentNullException(nameof(viewModelInformer));

            CutVMs = new ObservableCollection<CutEntryVM>();

            ItemsVMs = new ObservableCollection<Notifier>();

            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsVMs);

            if (view != null)
            {
                view.SortDescriptions.Add(new SortDescription(nameof(IsFolder), ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Ascending));
            }

            foreach (var itemM in Model) { Add(null, _viewModelCreator(itemM)); }

            Context = new ObservableCollection<FolderM>();
            Context.Add(new FolderActionM());
            Context.Add(new FolderM() { name = "root", content = model });
        }



        private ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<bool>((isFolder) =>
        {
            BaseM model = _modelCreator(isFolder);
            Notifier viewModel = _viewModelCreator(model);

            Add(model, viewModel);

            Selection = viewModel;

            CommandManager.InvalidateRequerySuggested();

        }));

        private ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand(() =>
        {
            Notifier prevSelection = Selection;

            Selection = null;

            Remove(prevSelection, _modelExtractor(prevSelection), Context.Last());

            CommandManager.InvalidateRequerySuggested();

        }, () => Selection != null));

        private ICommand cutCommand;
        public ICommand CutCommand => cutCommand ?? (cutCommand = new RelayCommand<Notifier>((itemVM) =>
        {
            CutVMs.Add(new CutEntryVM() { Model = _modelExtractor(itemVM), ViewModel = itemVM, Context = Context.Last() });
            itemVM.IsCut = true;

            CommandManager.InvalidateRequerySuggested();

        }, (itemVM) => itemVM != null && !itemVM.IsCut));

        private ICommand pasteCommand;
        public ICommand PasteCommand => pasteCommand ?? (pasteCommand = new RelayCommand(() =>
        {
            foreach (var cutEntryVM in CutVMs)
            {
                Remove(cutEntryVM.ViewModel, cutEntryVM.Model, cutEntryVM.Context);                
                Add(cutEntryVM.Model, cutEntryVM.ViewModel);

                cutEntryVM.ViewModel.IsCut = false;
            }

            Selection = CutVMs.Last().ViewModel;

            CutVMs.Clear();

            CommandManager.InvalidateRequerySuggested();

        }, () => CutVMs.Count > 0));

        private ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<Notifier>((item) => _viewModelInformer(item), (item) => item != null));

        private ICommand upContextCommand;
        public ICommand UpContextCommand => upContextCommand ?? (upContextCommand = new RelayCommand(() =>
        {
            Context.RemoveAt(Context.Count - 1);

            ItemsVMs.Clear();
            foreach (var itemM in Context.Last().content) { Add(null, _viewModelCreator(itemM)); }

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
            foreach (var itemM in Context.Last().content) { Add(null, _viewModelCreator(itemM)); }

            CommandManager.InvalidateRequerySuggested();

        }, (folderM) => folderM != null));

        private ICommand selectCommand;
        public ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((itemVM) =>
        {
            Selection = itemVM;

            CommandManager.InvalidateRequerySuggested();

        }, (itemVM) => itemVM != null));



        private void Add(BaseM model, Notifier viewModel) // pass null to one of params if want to add only model/only viewModel
        {
            if (model != null) Context.Last().content.Add(model);

            if (viewModel != null) ItemsVMs.Add(viewModel);
        }
        private void Remove(Notifier viewModel, BaseM model, FolderM context) // pass null to one of params if want to remove only model/only viewModel
        {
            if (viewModel != null) ItemsVMs.Remove(viewModel);

            if (model != null) context.content.Remove(model);
        }



        private readonly Func<bool, BaseM> _modelCreator;
        private readonly Func<BaseM, Notifier> _viewModelCreator;
        private readonly Func<Notifier, Notifier> _editorCreator;
        private readonly Func<Notifier, BaseM> _modelExtractor;
        private readonly Action<Notifier> _viewModelInformer;



        public ObservableCollection<CutEntryVM> CutVMs { get; }
        public ObservableCollection<Notifier> ItemsVMs { get; }
        public ObservableCollection<FolderM> Context { get; }



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
    }
}