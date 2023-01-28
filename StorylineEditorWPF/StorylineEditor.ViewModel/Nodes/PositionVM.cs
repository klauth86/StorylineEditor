/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.Nodes
{
    public class PositionVM : IPositioned
    {
        public PositionVM(double inPositionX, double inPositionY, string inId, string inCharacterId)
        {
            positionX = inPositionX;
            positionY = inPositionY;
            id = inId;
            characterId = inCharacterId;
        }

        protected readonly double positionX;
        public double PositionX { get => positionX; set => throw new NotImplementedException(); }

        protected readonly double positionY;
        public double PositionY { get => positionY; set => throw new NotImplementedException(); }

        protected readonly string id;
        public string Id { get => id; }

        protected readonly string characterId;
        public string CharacterId { get => characterId; }
    }
}