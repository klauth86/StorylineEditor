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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Xml.Serialization;
using System.Windows.Input;
using StorylineEditor.CopyPasteService;
using StorylineEditor.Additive;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class FullContextVm : BaseVm
    {
        public event Action OnClosingEvent = delegate { };
        public void OnClosing() { OnClosingEvent(); }


        public List<Action_BaseVm> Actions { get; set; }


        protected ObservableCollection<BaseVm<FullContextVm>> tabs;
        public ObservableCollection<BaseVm<FullContextVm>> Tabs => tabs;

        public T GetTab<T>() where T : class => tabs.FirstOrDefault(item => item is T) as T;

        public CharactersTabVm CharactersTab => GetTab<CharactersTabVm>();
        public ItemsTabVm ItemsTab => GetTab<ItemsTabVm>();
        public LocationObjectsTabVm LocationObjectsTab => GetTab<LocationObjectsTabVm>();
        public GlobalTagsTabVm GlobalTagsTab => GetTab<GlobalTagsTabVm>();
        public JournalRecordsTabVm JournalRecordsTab => GetTab<JournalRecordsTabVm>();
        public PlayerDialogsTabVm PlayerDialogsTab => GetTab<PlayerDialogsTabVm>();
        public ReplicasTabVm ReplicasTab => GetTab<ReplicasTabVm>();

        public IEnumerable<FolderedVm> Characters
        {
            get
            {
                foreach (FolderedVm foldered in CharactersTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
            }
        }

        public IEnumerable<FolderedVm> Items
        {
            get
            {
                foreach (FolderedVm foldered in ItemsTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
            }
        }

        public IEnumerable<BaseVm> MiniGames
        {
            get
            {
                foreach (FolderedVm foldered in ItemsTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        if (subFoldered is LocationObjectVm locationObject && locationObject.IsMiniGame) yield return subFoldered;
                    }
                }
            }
        }
        public IEnumerable<BaseVm> ObjectsWithActivation
        {
            get
            {
                foreach (FolderedVm foldered in ItemsTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
                foreach (FolderedVm foldered in LocationObjectsTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
            }
        }
        public IEnumerable<BaseVm> AllActors
        {
            get
            {
                foreach (FolderedVm foldered in ItemsTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
                foreach (FolderedVm foldered in LocationObjectsTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
                foreach (var item in CharactersTab.Items) yield return item;
            }
        }

        public IEnumerable<FolderedVm> JournalRecords
        {
            get
            {
                foreach (FolderedVm foldered in JournalRecordsTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
            }
        }

        public IEnumerable<FolderedVm> Replicas
        {
            get
            {
                foreach (FolderedVm foldered in ReplicasTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
            }
        }
        public IEnumerable<FolderedVm> DialogsAndReplicas
        {
            get
            {
                foreach (FolderedVm foldered in PlayerDialogsTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }

                foreach (FolderedVm foldered in ReplicasTab.Items)
                {
                    foreach (var subFoldered in foldered.FoldersTraversal())
                    {
                        yield return subFoldered;
                    }
                }
            }
        }

        public FullContextVm() : base() { Actions = new List<Action_BaseVm>(); tabs = new ObservableCollection<BaseVm<FullContextVm>>(); }

        public bool IsEmpty() =>
            (null == CharactersTab || CharactersTab.Items.Count == 0 || CharactersTab.Items.All(item => item.Id == CharacterVm.PlayerId)) &&
            (null == ItemsTab || ItemsTab.Items.Count == 0) &&
            (null == LocationObjectsTab || LocationObjectsTab.Items.Count == 0) &&
            (null == PlayerDialogsTab || PlayerDialogsTab.Items.Count == 0) &&
            (null == ReplicasTab || ReplicasTab.Items.Count == 0) &&
            (null == JournalRecordsTab || JournalRecordsTab.Items.Count == 0) &&
            (null == GlobalTagsTab || GlobalTagsTab.Items.Count == 0);

        public void AddWorkTabs()
        {
            var charactersTab = new CharactersTabVm(this, 0) { Name = "Персонажи" };
            CharacterVm.AddPlayerIfHasNoOne(charactersTab);
            tabs.Add(charactersTab);

            tabs.Add(new ItemsTabVm(this, 0) { Name = "Предметы" });
            tabs.Add(new LocationObjectsTabVm(this, 0) { Name = "Объекты локации" });

            tabs.Add(new GlobalTagsTabVm(this, 0) { Name = "Глобальные тэги" });

            tabs.Add(new JournalRecordsTabVm(this, 0) { Name = "Журнальные записи" });
            
            tabs.Add(new PlayerDialogsTabVm(this, 0) { Name = "Диалоги" });
            tabs.Add(new ReplicasTabVm(this, 0) { Name = "Реплики" });
        }

        public void Init()
        {
            foreach (var tab in tabs)
            {
                tab.Parent = this;
                tab.SetupParenthood();
            }
        }

        protected ICommand copyCommand;
        public ICommand CopyCommand => copyCommand ?? (copyCommand = new RelayCommand(() => ICopyPasteService.Context?.Copy()));

        protected ICommand pasteCommand;
        public ICommand PasteCommand => pasteCommand ?? (pasteCommand = new RelayCommand(() => ICopyPasteService.Context?.Paste()));


        public static event Action<string> OnSearchFilterChangedEvent = delegate { };
        public string SearchFilter { set => OnSearchFilterChangedEvent(value?.ToLower()); }
    }
}