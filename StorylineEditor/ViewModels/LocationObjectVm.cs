/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Tabs;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class LocationObjectVm : NonFolderVm
    {
        public LocationObjectVm(LocationObjectsTabVm inParent) : base(inParent)
        {
            IsActor = true;

            FullContextVm.OnSearchFilterChangedEvent += OnSearchFilterChanged;
        }

        public LocationObjectVm() : this(null) { }

        ~LocationObjectVm() { FullContextVm.OnSearchFilterChangedEvent -= OnSearchFilterChanged; }

        protected bool isActor;
        public bool IsActor
        {
            get => isActor;
            set
            {
                if (isActor != value)
                {
                    isActor = value;
                    NotifyWithCallerPropName();

                    if (value)
                    {
                        IsActivationPoint = false;
                        IsMiniGame = false;
                    }
                }
            }
        }

        protected bool isActivationPoint;
        public bool IsActivationPoint
        {
            get => isActivationPoint;
            set
            {
                if (isActivationPoint != value)
                {
                    isActivationPoint = value;
                    NotifyWithCallerPropName();
                    Notify(nameof(CanBeFoundByClass));

                    if (value)
                    {
                        IsActor = false;
                        IsMiniGame = false;
                    }
                }
            }
        }

        protected bool isMiniGame;
        public bool IsMiniGame
        {
            get => isMiniGame;
            set
            {
                if (isMiniGame != value)
                {
                    isMiniGame = value;
                    NotifyWithCallerPropName();

                    if (value)
                    {
                        IsActor = false;
                        IsActivationPoint = false;
                    }
                }
            }
        }

        public bool CanBeFoundByClass => !isActivationPoint;

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is LocationObjectVm casted)
            {
                casted.isActor = isActor;
                casted.isActivationPoint = isActivationPoint;
                casted.isMiniGame = isMiniGame;
            }
        }
    }
}