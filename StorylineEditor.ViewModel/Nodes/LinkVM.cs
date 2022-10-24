/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel.Common;

namespace StorylineEditor.ViewModel.Nodes
{
    public interface ILinkVM
    {
        // Absoulute
        double FromX { get; set; }
        double FromY { get; set; }
        double ToX { get; set; }
        double ToY { get; set; }

        // Local
        double Left { get; set; }
        double Top { get; set; }
        double HandleX { get; set; }
        double HandleY { get; set; }

        string Description { get; set; }
    }

    public class LinkVM : BaseVM<LinkM>
    {
        public LinkVM(LinkM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public sealed class PreviewLinkVM : SimpleVM<object>, ILinkVM
    {
        public PreviewLinkVM(ICallbackContext callbackContext) : base(new object(), callbackContext) { }

        public double FromX { get; set; }
        public double FromY { get; set; }
        public double ToX { get; set; }
        public double ToY { get; set; }

        private double _localFromX;
        public double Left
        {
            get => _localFromX;
            set
            {
                if (_localFromX != value)
                {
                    _localFromX = value;
                    Notify(nameof(Left));
                }
            }
        }

        private double _localFromY;
        public double Top
        {
            get => _localFromY;
            set
            {
                if (_localFromY != value)
                {
                    _localFromY = value;
                    Notify(nameof(Top));
                }
            }
        }

        private double _localToX;
        public double HandleX
        {
            get => _localToX;
            set
            {
                if (_localToX != value)
                {
                    _localToX = value;
                    Notify(nameof(HandleX));
                }
            }
        }

        private double _localToY;
        public double HandleY
        {
            get => _localToY;
            set
            {
                if (_localToY != value)
                {
                    _localToY = value;
                    Notify(nameof(HandleY));
                }
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    Notify(nameof(Description));
                }
            }
        }
    }
}