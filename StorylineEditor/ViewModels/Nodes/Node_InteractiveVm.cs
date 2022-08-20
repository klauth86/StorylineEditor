/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.GameEvents;
using StorylineEditor.ViewModels.Predicates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [XmlRoot]
    public abstract class Node_InteractiveVm : Node_BaseVm
    {
        public IEnumerable<Type> EventTypes => AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                    .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(GE_BaseVm)));
        public IEnumerable<Type> PredicateTypes => AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsSubclassOf(typeof(P_BaseVm)));

        public Node_InteractiveVm(TreeVm Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            GameEvents = new ObservableCollection<GE_BaseVm>();
            Predicates = new ObservableCollection<P_BaseVm>();

            GameEvents.CollectionChanged += OnCollectinChanged;
            Predicates.CollectionChanged += OnCollectinChanged;
        }

        public Node_InteractiveVm() : this(null, 0) { }

        ~Node_InteractiveVm()
        {
            GameEvents.CollectionChanged -= OnCollectinChanged;
            Predicates.CollectionChanged -= OnCollectinChanged;
        }

        private void OnCollectinChanged(object sender, NotifyCollectionChangedEventArgs e) { 
            NotifyIsValidChanged();
            Notify(nameof(HasEvents));
            Notify(nameof(HasPredicates));
        }

        public override bool IsValid => base.IsValid &&
                    GameEvents.All(gameEvent => gameEvent?.IsValid ?? false) &&
                    Predicates.All(predicate => predicate?.IsValid ?? false);

        public ObservableCollection<GE_BaseVm> GameEvents { get; set; }
        public bool HasEvents => GameEvents.Count > 0;

        public ObservableCollection<P_BaseVm> Predicates { get; set; }
        public bool HasPredicates => Predicates.Count > 0;

        ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<Type>((type) =>
                 {
                     var newItem = CustomByteConverter.CreateByName(type.Name, this, 0);

                     if (newItem is GE_BaseVm newEvent) GameEvents.Add(newEvent);

                     if (newItem is P_BaseVm newPredicate) Predicates.Add(newPredicate);

                 }, (type) => type != null));

        ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand<object>(
                    (argument) =>
                    {
                        if (argument is GE_BaseVm gameEvent) GameEvents.Remove(gameEvent);

                        if (argument is P_BaseVm predicate) Predicates.Remove(predicate);
                    }, (argument) => argument != null));

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is Node_InteractiveVm casted)
            {
                long counter = 0;

                foreach (var gameEvent in GameEvents)
                {
                    casted.GameEvents.Add(gameEvent.Clone<GE_BaseVm>(casted, additionalTicks + counter++));
                }

                foreach (var predicate in Predicates)
                {
                    casted.Predicates.Add(predicate.Clone<P_BaseVm>(casted, additionalTicks + counter++));
                }
            }
        }

        public override void SetupParenthood()
        {
            foreach (var gameEvent in GameEvents)
            {
                gameEvent.Parent = this;
                gameEvent.SetupParenthood();
            }

            foreach (var predicate in Predicates)
            {
                predicate.Parent = this;
                predicate.SetupParenthood();
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) ||
            Predicates.Any(predicate => predicate.PassFilter(filter)) ||
            GameEvents.Any(gameEvent => gameEvent.PassFilter(filter));
    }
}