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
using System;

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
        double LocalFromX { get; set; }
        double LocalFromY { get; set; }
        double LocalToX { get; set; }
        double LocalToY { get; set; }

        string Description { get; set; }
    }

    public class LinkVM : BaseVM<LinkM>
    {
        public LinkVM(LinkM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public sealed class PreviewLinkVM : SimpleVM<object>, ILinkVM
    {
        public PreviewLinkVM(ICallbackContext callbackContext) : base(null, callbackContext) { }

        private double _fromX;
        public double FromX
        {
            get => _fromX;
            set
            {
                if (value != _fromX)
                {
                    _fromX = value;
                    CallbackContext?.Callback(this, nameof(FromX));
                }
            }
        }

        private double _fromY;
        public double FromY
        {
            get => _fromY;
            set
            {
                if (value != _fromY)
                {
                    _fromY = value;
                    CallbackContext?.Callback(this, nameof(FromY));
                }
            }
        }

        private double _toX;
        public double ToX
        {
            get => _toX;
            set
            {
                if (value != _toX)
                {
                    _toX = value;
                    CallbackContext?.Callback(this, nameof(ToX));
                }
            }
        }

        private double _toY;
        public double ToY
        {
            get => _toY;
            set
            {
                if (value != _toY)
                {
                    _toY = value;
                    CallbackContext?.Callback(this, nameof(ToY));
                }
            }
        }

        private double _localFromX;
        public double LocalFromX
        {
            get => _localFromX;
            set
            {
                if (_localFromX != value)
                {
                    _localFromX = value;
                    Notify(nameof(LocalFromX));
                }
            }
        }

        private double _localFromY;
        public double LocalFromY
        {
            get => _localFromY;
            set
            {
                if (_localFromY != value)
                {
                    _localFromY = value;
                    Notify(nameof(LocalFromY));
                }
            }
        }

        private double _localToX;
        public double LocalToX
        {
            get => _localToX;
            set
            {
                if (_localToX != value)
                {
                    _localToX = value;
                    Notify(nameof(LocalToX));
                }
            }
        }

        private double _localToY;
        public double LocalToY
        {
            get => _localToY;
            set
            {
                if (_localToY != value)
                {
                    _localToY = value;
                    Notify(nameof(LocalToY));
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