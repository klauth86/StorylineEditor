/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using StorylineEditor.FileDialog;
using StorylineEditor.Models;
using StorylineEditor.ViewModels;
using StorylineEditor.ViewModels.Tabs;
using StorylineEditor.Views;

namespace StorylineEditor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static event Action<double> TickEvent = delegate { };

        public static event Action<bool, Key> FacadeKeyEvent = delegate { };

        readonly DispatcherTimer autosaveTimer;

        public MainWindow()
        {
            InitializeComponent();
            var fullContext = new FullContextVm();
            fullContext.AddWorkTabs();

            DataContext = fullContext;

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            Title = assemblyName.Name;

            autosaveTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 5, 0)
            };
            autosaveTimer.Tick += OnAutosaveTimer;
            autosaveTimer.Start();

            DefaultDialogService.Init();
        }

        ~MainWindow()
        {
            autosaveTimer.Stop();
            autosaveTimer.Tick -= OnAutosaveTimer;
        }

        private void OnAutosaveTimer(object sender, EventArgs e)
        {
            ////// TODO
            //////if (ckb_AutoSave.IsChecked ?? false)
            //////{
            //////    ////// var fullContext = DataContext as FullContextVm;
            //////}
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) { FacadeKeyEvent(true, e.Key); base.OnPreviewKeyDown(e); }

        protected override void OnPreviewKeyUp(KeyEventArgs e) { FacadeKeyEvent(false, e.Key); base.OnPreviewKeyUp(e); }


        const string xmlFilter = "XML files (*.xml)|*.xml";

        private void OpenXML_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(IDialogService.DialogService.OpenFileDialog(xmlFilter, true))) OpenAsXml(IDialogService.DialogService.Path);
        }

        private void SaveXML_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(IDialogService.DialogService.Path)) SaveAsXml(IDialogService.DialogService.Path);

            else if (!string.IsNullOrEmpty(IDialogService.DialogService.SaveFileDialog(xmlFilter, true))) SaveAsXml(IDialogService.DialogService.Path);
        }

        private void SaveXML_Click_M(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(IDialogService.DialogService.SaveFileDialog(xmlFilter, true))) SaveAsXmlModel(IDialogService.DialogService.Path);
        }

        private void Exit_Click(object sender, RoutedEventArgs e) { App.Current.Shutdown(); }

        private void OpenAsXml(string path)
        {
            DataContext = null;

            FullContextVm fullContext = null;
            using (var fileStream = File.Open(path, FileMode.Open))
            {
                fullContext = App.DeserializeXml<FullContextVm>(fileStream);
            }
            fullContext?.Init();

            ////// TODO If ordering needed before load uncomment this
            //////foreach (var tab in fullContext.Tabs)
            //////{
            //////    if (tab is FolderedTabVm folderedTab) { folderedTab.SortItems(); }
            //////}

            DataContext = fullContext;

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            Title = string.Format("{0} [{1}]", assemblyName.Name, path);
        }

        private void SaveAsXml(string path)
        {
            if (DataContext is FullContextVm fullContext)
            {
                using (var fileStream = File.Open(path, FileMode.Create))
                {
                    App.SerializeXml<FullContextVm>(fileStream, fullContext);
                }
            }
        }

        private void SaveAsXmlModel(string path)
        {
            if (DataContext is FullContextVm fullContext)
            {
                using (var fileStream = File.Open(path, FileMode.Create))
                {
                    App.SerializeXml<StorylineM>(fileStream, new StorylineM());
                }
            }
        }


        protected FrameworkElement DraggedElement;

        protected IDragOverable dragOvered;
        protected IDragOverable DragOvered
        {
            get => dragOvered;
            set
            {

                if (value != dragOvered)
                {
                    if (dragOvered != null)
                    {
                        dragOvered.IsDragOver = false;
                    }

                    dragOvered = value;

                    if (dragOvered != null)
                    {
                        dragOvered.IsDragOver = true;
                    }
                }
            }
        }

        protected ScrollViewer scrollViewer;

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            var frameworkElement = (FrameworkElement)e.OriginalSource;
            if (frameworkElement.DataContext is IDragOverable dragOvered)
            {
                DragOvered = dragOvered;

                if (frameworkElement is Grid && frameworkElement.TemplatedParent is ScrollBar)
                {
                    scrollViewer = (ScrollViewer)((FrameworkElement)frameworkElement.TemplatedParent).TemplatedParent;
                }
            }
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            var frameworkElement = (FrameworkElement)e.OriginalSource;
            if (frameworkElement != null)
            {
                DragOvered = null;

                scrollViewer = null;
            }
        }

        public void StartDrag(FrameworkElement draggedElement, Point screenPoint)
        {
            DraggedElement = draggedElement;

            DragDrop.DoDragDrop(this, new DataObject(typeof(FolderedVm).ToString(), DraggedElement.DataContext), DragDropEffects.Move);
        }

        private bool IsDataOk(FolderedVm draggedFoldered, FolderedVm dragOveredFoldered)
        {
            if (draggedFoldered != null && dragOveredFoldered != null && draggedFoldered != dragOveredFoldered)
            {
                if (draggedFoldered.IsFolder && dragOveredFoldered.IsFolder)
                    return !dragOveredFoldered.IsContaining(draggedFoldered, false) &&
                        !draggedFoldered.IsContaining(dragOveredFoldered, true);

                if (draggedFoldered.IsFolder && !dragOveredFoldered.IsFolder)
                    return draggedFoldered.Folder != dragOveredFoldered.Folder &&
                        !draggedFoldered.IsContaining(dragOveredFoldered, true);

                if (!draggedFoldered.IsFolder && dragOveredFoldered.IsFolder)
                    return !dragOveredFoldered.IsContaining(draggedFoldered, false);

                if (!draggedFoldered.IsFolder && !dragOveredFoldered.IsFolder)
                    return draggedFoldered.Folder != dragOveredFoldered.Folder;
            }

            return draggedFoldered.Folder != dragOveredFoldered; ;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            FolderedVm draggedFoldered = (FolderedVm)DraggedElement.DataContext;
            FolderedVm dragOveredFoldered = DragOvered as FolderedVm;

            bool isControlOk = DragOvered != null && DraggedElement.DataContext != DragOvered;
            bool isOk = isControlOk && IsDataOk(draggedFoldered, dragOveredFoldered);

            if (isOk)
            {
                FolderedTabVm.AddToItem(draggedFoldered, dragOveredFoldered);
            }

            DragOvered = null;
            DraggedElement = null;
        }

        private void Window_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;

            FolderedVm draggedFoldered = (FolderedVm)DraggedElement.DataContext;
            FolderedVm dragOveredFoldered = DragOvered as FolderedVm;

            bool isControlOk = DragOvered != null && DraggedElement.DataContext != DragOvered;
            bool isOk = isControlOk && IsDataOk(draggedFoldered, dragOveredFoldered);

            Mouse.SetCursor(isOk ? Cursors.ScrollAll : Cursors.No);

            e.Handled = true;
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (scrollViewer != null)
            {
                var mousePosition = e.GetPosition(scrollViewer);
                var ratio = mousePosition.Y / scrollViewer.ActualHeight;
                if (ratio < 0.2) scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 0.01 * scrollViewer.ActualHeight);
                if (ratio > 0.8) scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 0.01 * scrollViewer.ActualHeight);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) { GlobalFilterHelper.Filter = tb_Filter.Text; }
    }
}