/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Service;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public static class ActiveContext
    {
        private static double _viewWidth;
        public static double ViewWidth { get => _viewWidth; set => _viewWidth = value > 0 ? value : _viewWidth; }

        private static double _viewHeight;
        public static double ViewHeight { get => _viewHeight; set => _viewHeight = value > 0 ? value : _viewHeight; }

        public static IDialogService DialogService { get; set; }
        public static ILocalizationService LocalizationService { get; set; }
        public static ISerializationService SerializationService { get; set; }
        public static ITaskService TaskService { get; set; }
        public static IFileService FileService { get; set; }
        public static ISoundPlayerService SoundPlayerService { get; set; }

        public static HistoryVM History { get; }


        static ActiveContext()
        {
            History = new HistoryVM();
        }


        private static StorylineVM _storyline;
        public static StorylineVM Storyline
        {
            get => _storyline;
            set
            {
                if (_storyline != value)
                {
                    Reset();
                    _storyline = value;
                }
            }
        }


        public static IPartiallyStored LocationsTab { get; set; }
        public static IPartiallyStored CharactersTab { get; set; }
        public static IPartiallyStored ItemsTab { get; set; }
        public static IPartiallyStored ActorsTab { get; set; }
        public static IPartiallyStored JournalTab { get; set; }
        public static IPartiallyStored DialogsTab { get; set; }
        public static IPartiallyStored ReplicasTab { get; set; }


        public static event EventHandler ActiveTabChanged = delegate { };

        private static IPartiallyStored _activeTab;
        public static IPartiallyStored ActiveTab
        {
            get => _activeTab;
            set
            {
                if (value != _activeTab)
                {
                    _activeTab?.OnLeave();

                    _activeTab = value;

                    _activeTab?.OnEnter();

                    ActiveTabChanged(null, EventArgs.Empty);

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }


        public static ICopyPaste ActiveCopyPaste { get; set; }
        public static IGraph ActiveGraph { get; set; }


        public static IEnumerable<BaseM> GetEnumerator(params List<BaseM>[] collectionSet)
        {
            for (int i = 0; i < collectionSet.Length; i++)
            {
                foreach (var item in collectionSet[i])
                {
                    if (item is FolderM folder)
                    {
                        foreach (var contentItem in GetEnumerator(folder.content))
                        {
                            yield return contentItem;
                        }
                    }
                    else
                    {
                        yield return item;
                    }
                }
            }
        }

        public static IEnumerable<BaseM> Locations => GetEnumerator(Storyline?.Model.locations);
        public static BaseM GetLocation(string id) => Locations?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> Characters => GetEnumerator(Storyline?.Model.characters);
        public static BaseM GetCharacter(string id) => Characters?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> Items => GetEnumerator(Storyline?.Model.items);
        public static BaseM GetItem(string id) => Items?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> Actors => GetEnumerator(Storyline?.Model.actors);
        public static BaseM GetActor(string id) => Actors?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> Quests => GetEnumerator(Storyline?.Model.journal);
        public static BaseM GetQuest(string id) => Quests?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> DialogsAndReplicas => GetEnumerator(Storyline?.Model.dialogs, Storyline?.Model.replicas);
        public static BaseM GetDialogOrReplica(string id) => DialogsAndReplicas?.FirstOrDefault((model) => model.id == id);

        public static IEnumerable<BaseM> Dialogs => GetEnumerator(Storyline?.Model.dialogs);
        public static BaseM GetDialog(string id) => Dialogs?.FirstOrDefault((model) => model.id == id);


        public static void Reset()
        {
            ActiveGraph = null;

            ActiveTab = null;

            LocationsTab = null;
            CharactersTab = null;
            ItemsTab = null;
            ActorsTab = null;
            JournalTab = null;
            DialogsTab = null;
            ReplicasTab = null;

            History.Clear();
        }
    }
}