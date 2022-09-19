using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StorylineEditor.Views.Controls
{
    public class InstancedGrid : Grid
    {
        public static HashSet<InstancedGrid> Instances = new HashSet<InstancedGrid>();

        public InstancedGrid()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!Instances.Contains(this)) Instances.Add(this);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (Instances.Contains(this)) Instances.Remove(this);
        }
    }
}