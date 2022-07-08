/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels.Nodes;
using System;
using System.Globalization;
using System.Windows.Data;

namespace StorylineEditor.Views.Converters
{
    class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Type)
            {
                if ((Type)value == typeof(DNode_CharacterVm)) return "💬";
                if ((Type)value == typeof(DNode_DialogVm)) return "💬";
                if ((Type)value == typeof(DNode_RandomVm)) return "⇝";
                if ((Type)value == typeof(DNode_TransitVm)) return "⇴";
                if ((Type)value == typeof(DNode_VirtualVm)) return "👤";

                if ((Type)value == typeof(JNode_AlternativeVm)) return "💡";
                if ((Type)value == typeof(JNode_StepVm)) return "✔";
            }
            else if (value != null)
            {
                if (value.GetType() == typeof(DNode_CharacterVm)) return "💬";
                if (value.GetType() == typeof(DNode_DialogVm)) return "💬";
                if (value.GetType() == typeof(DNode_RandomVm)) return "⇝";
                if (value.GetType() == typeof(DNode_TransitVm)) return "⇴";
                if (value.GetType() == typeof(DNode_VirtualVm)) return "👤";

                if (value.GetType() == typeof(JNode_AlternativeVm)) return "💡";
                if (value.GetType() == typeof(JNode_StepVm)) return "✔";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
