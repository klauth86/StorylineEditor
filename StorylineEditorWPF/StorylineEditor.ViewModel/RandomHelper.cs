using System;

namespace StorylineEditor.ViewModel
{
    public static class RandomHelper
    {
        private static readonly Random random = new Random();

        public static int Next(int maxValue) { return random.Next(maxValue); }
        public static double NextDouble() { return random.NextDouble(); }
    }
}
