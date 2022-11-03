/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StorylineEditor.App.Controls
{
    public class RichTextBoxExt : RichTextBox
    {
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            Application.Current.Dispatcher.BeginInvoke(new Action(() => SetDocumentChangedFlag(this, !GetDocumentChangedFlag(this))));
        }

        private static readonly DependencyProperty BindableDocumentProperty = DependencyProperty.Register
            (
            "BindableDocument",
            typeof(FlowDocument),
            typeof(RichTextBoxExt),
            new PropertyMetadata(null, OnBindableDocumentPropertyChanged)
            );

        public static void SetBindableDocument(DependencyObject dp, FlowDocument value) { dp.SetValue(BindableDocumentProperty, value); }
        public static FlowDocument GetBindableDocument(DependencyObject dp) { return (FlowDocument)dp.GetValue(BindableDocumentProperty); }

        private static void OnBindableDocumentPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != DependencyProperty.UnsetValue && !string.IsNullOrEmpty(args.NewValue?.ToString()))
            {
                if (dp is RichTextBoxExt rtbExt)
                {
                    if (args.NewValue != DependencyProperty.UnsetValue && args.NewValue != null)
                    {
                        rtbExt.Document = args.NewValue as FlowDocument;
                    }
                }
            }
        }

        private static readonly DependencyProperty DocumentChangedFlagProperty = DependencyProperty.Register
            (
            "DocumentChangedFlag",
            typeof(bool),
            typeof(RichTextBoxExt),
            new PropertyMetadata(false)
            );

        public static void SetDocumentChangedFlag(DependencyObject dp, bool value) { dp.SetValue(DocumentChangedFlagProperty, value); }
        public static bool GetDocumentChangedFlag(DependencyObject dp) { return (bool)dp.GetValue(DocumentChangedFlagProperty); }
    }
}