using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.Nodes
{
    public class OriginVM : IPositioned
    {
        public static OriginVM GetOrigin() { return _instance ?? (_instance = new OriginVM()); }
        public double PositionX { get => 0; set => throw new NotImplementedException(); }
        public double PositionY { get => 0; set => throw new NotImplementedException(); }
        
        private OriginVM() { }

        private static OriginVM _instance;
    }
}
