﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

namespace StorylineEditor.Model.Nodes
{
    public abstract class Node_BaseM : BaseM
    {
        public Node_BaseM(long additionalTicks) : base(additionalTicks)
        {
            gender = GENDER.UNSET;
            positionX = 0;
            positionY = 0;
        }

        public Node_BaseM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_BaseM casted)
            {
                casted.gender = gender;
                casted.positionX = positionX;
                casted.positionY = positionY;
            }
        }

        public byte gender { get; set; }
        public double positionX { get; set; }
        public double positionY { get; set; }
    }
}