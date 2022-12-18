﻿/*
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
    public class OriginVM : IPositioned
    {
        public static OriginVM GetOrigin() { return _instance ?? (_instance = new OriginVM()); }
        public double PositionX { get => 0; set => throw new NotImplementedException(); }
        public double PositionY { get => 0; set => throw new NotImplementedException(); }
        public string Id => null;

        private OriginVM() { }

        private static OriginVM _instance;
    }
}