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

using StorylineEditor.App.Config;
using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.RichText;
using StorylineEditor.Service;
using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Config;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDialogService, ILocalizationService
    {
        public const string XmlFilter = "XML files (*.xml)|*.xml";

        static MainWindow()
        {
            ActiveContext.SerializationService = new SerializationService();
            ActiveContext.TaskService = new TaskService();
            ActiveContext.FileService = new FileService();
            ActiveContext.SoundPlayerService = new SoundPlayerService();

            ActiveContext.FileService.LoadConfig();
        }

        public MainWindow()
        {
            InitializeComponent();

            ActiveContext.DialogService = this;
            ActiveContext.LocalizationService = this;

            InitializeLoc();

            StorylineM storylineModel = new StorylineM();
            storylineModel.characters.Add(new CharacterM() { id = CharacterM.PLAYER_ID, name = App.Current.Resources["String_Player"]?.ToString() });

            SetDataContext(new StorylineVM(storylineModel));

            SetTitleForMainWindow();
        }

        protected void SetTitleForMainWindow()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            App.Current.Resources["String_MainWindow_Title"] = string.Format("{0} [{1}]", assemblyName.Name, ActiveContext.FileService.Path ?? GetLocalizedString("String_New_Storyline"));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ActiveContext.SoundPlayerService.Dispose();
            ActiveContext.FileService.Dispose();
            ActiveContext.TaskService.Dispose();

            base.OnClosing(e);
        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ActiveContext.FileService.OpenFile(XmlFilter, true))) OpenFile(ActiveContext.FileService.Path);
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ActiveContext.FileService.Path)) SaveFile(ActiveContext.FileService.Path);

            else if (!string.IsNullOrEmpty(ActiveContext.FileService.SaveFile(XmlFilter, true))) SaveFile(ActiveContext.FileService.Path);
        }

        private void OpenFile(string path)
        {
            SetDataContext(null);

            if (File.Exists(path))
            {
                using (FileStream fileStream = File.Open(path, FileMode.Open))
                {
                    try
                    {
                        var model = ActiveContext.SerializationService.Deserialize<StorylineM>(fileStream);
                        ValidateModel(model);

                        SetDataContext(new StorylineVM(model));

                        SetTitleForMainWindow();
                    }
                    catch (InvalidDataException idExc)
                    {
                        MessageBox.Show(idExc.Message, "Error", MessageBoxButton.OK);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Can not open file!", "Error", MessageBoxButton.OK);
                    }
                }
            }
        }

        private void SaveFile(string path)
        {
            if (DataContext is StorylineVM storylineVM)
            {
                try
                {
                    Dictionary<string, string> beforeRtbMapping = new Dictionary<string, string>();
                    Dictionary<string, string> afterRtbMapping = new Dictionary<string, string>();

                    SimplifyRichText_Actors(storylineVM.Model.characters, beforeRtbMapping, afterRtbMapping);
                    SimplifyRichText_Items(storylineVM.Model.items, beforeRtbMapping, afterRtbMapping);
                    SimplifyRichText_Actors(storylineVM.Model.actors, beforeRtbMapping, afterRtbMapping);

                    SimplifyRichText_Nodes(storylineVM.Model.journal, beforeRtbMapping, afterRtbMapping);
                    SimplifyRichText_Nodes(storylineVM.Model.dialogs, beforeRtbMapping, afterRtbMapping);
                    SimplifyRichText_Nodes(storylineVM.Model.replicas, beforeRtbMapping, afterRtbMapping);

                    if (beforeRtbMapping.Count != afterRtbMapping.Count)
                    {
                        throw new InvalidDataException(string.Format("Data simplify fault! Simplified items count mismatch {0}/{1}!", beforeRtbMapping.Count, afterRtbMapping.Count));
                    }
                    else
                    {
                        foreach (var key in beforeRtbMapping.Keys)
                        {
                            if (!afterRtbMapping.ContainsKey(key))
                            {
                                throw new InvalidDataException(string.Format("Data simplify fault! Simplified item not found {0}!", key));
                            }

                            if (beforeRtbMapping[key] != afterRtbMapping[key])
                            {
                                throw new InvalidDataException(string.Format("Data simplify fault! Simplified item corrupted {0}[\n{1}\n=>\n{2}\n]!", key, beforeRtbMapping[key], afterRtbMapping[key]));
                            }
                        }
                    }
                }
                catch (InvalidDataException idExc)
                {
                    MessageBox.Show(idExc.Message, "Error", MessageBoxButton.OK);
                    return;
                }

                using (var fileStream = File.Open(path, FileMode.Create))
                {
                    Dictionary<string, string> namesMapping = new Dictionary<string, string>();

                    // In fact we dont store any useful info in dialogs and replicas nodes name field
                    // It is generated during work and used in combobox search as string representation of item
                    // So no need to save it - just clear them before save and fix up after save

                    ClearUpNodesNames(storylineVM.Model.dialogs, namesMapping);
                    ClearUpNodesNames(storylineVM.Model.replicas, namesMapping);

                    ActiveContext.SerializationService.Serialize(fileStream, storylineVM.Model);

                    FixUpNodesNames(storylineVM.Model.replicas, namesMapping);
                    FixUpNodesNames(storylineVM.Model.dialogs, namesMapping);

                    SetTitleForMainWindow();
                }
            }
        }

        private void SimplifyRichText_Actors(List<BaseM> items, Dictionary<string, string> beforeRtbMapping, Dictionary<string, string> afterRtbMapping)
        {
            foreach (var listItem in items)
            {
                if (listItem is FolderM folder)
                {
                    SimplifyRichText_Actors(folder.content, beforeRtbMapping, afterRtbMapping);
                }
                else if (listItem is ActorM actor)
                {
                    beforeRtbMapping.Add(actor.id, actor.rtDescription.ToString());
                    actor.rtDescription = SimplifyRichText(actor.rtDescription);
                    afterRtbMapping.Add(actor.id, actor.rtDescription.ToString());

                    beforeRtbMapping.Add(actor.id + "F", actor.rtDescriptionFemale.ToString());
                    actor.rtDescriptionFemale = SimplifyRichText(actor.rtDescriptionFemale);
                    afterRtbMapping.Add(actor.id + "F", actor.rtDescriptionFemale.ToString());
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(listItem));
                }
            }
        }

        private void SimplifyRichText_Items(List<BaseM> items, Dictionary<string, string> beforeRtbMapping, Dictionary<string, string> afterRtbMapping)
        {
            foreach (var listItem in items)
            {
                if (listItem is FolderM folder)
                {
                    SimplifyRichText_Items(folder.content, beforeRtbMapping, afterRtbMapping);
                }
                else if (listItem is ItemM item)
                {
                    beforeRtbMapping.Add(item.id, item.rtDescription.ToString());
                    item.rtDescription = SimplifyRichText(item.rtDescription);
                    afterRtbMapping.Add(item.id, item.rtDescription.ToString());

                    beforeRtbMapping.Add(item.id + "F", item.rtDescriptionFemale.ToString());
                    item.rtDescriptionFemale = SimplifyRichText(item.rtDescriptionFemale);
                    afterRtbMapping.Add(item.id +"F", item.rtDescriptionFemale.ToString());

                    beforeRtbMapping.Add(item.id + "I", item.rtInternalDescription.ToString());
                    item.rtInternalDescription = SimplifyRichText(item.rtInternalDescription);
                    afterRtbMapping.Add(item.id + "I", item.rtInternalDescription.ToString());

                    beforeRtbMapping.Add(item.id + "IF", item.rtInternalDescriptionFemale.ToString());
                    item.rtInternalDescriptionFemale = SimplifyRichText(item.rtInternalDescriptionFemale);
                    afterRtbMapping.Add(item.id + "IF", item.rtInternalDescriptionFemale.ToString());
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(listItem));
                }
            }
        }

        private void SimplifyRichText_Nodes(List<BaseM> items, Dictionary<string, string> beforeRtbMapping, Dictionary<string, string> afterRtbMapping)
        {
            foreach (var listItem in items)
            {
                if (listItem is FolderM folder)
                {
                    SimplifyRichText_Nodes(folder.content, beforeRtbMapping, afterRtbMapping);
                }
                else if (listItem is GraphM graph)
                {
                    foreach (var node in graph.nodes)
                    {
                        beforeRtbMapping.Add(node.id, node.rtDescription.ToString());
                        node.rtDescription = SimplifyRichText(node.rtDescription);
                        afterRtbMapping.Add(node.id, node.rtDescription.ToString());
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(listItem));
                }
            }
        }

        private TextRangeM SimplifyRichText(TextRangeM textRangeModel)
        {
            RemoveEmptySubRanges(ref textRangeModel);
            CollapseSiblingSubRanges(ref textRangeModel);

            return textRangeModel;
        }

        private void RemoveEmptySubRanges(ref TextRangeM textRangeModel)
        {
            if (textRangeModel.IsSubRanged)
            {
                for (int i = textRangeModel.subRanges.Count - 1; i >= 0; i--)
                {
                    TextRangeM subTextRangeModel = textRangeModel.subRanges[i];
                    RemoveEmptySubRanges(ref subTextRangeModel);
                    textRangeModel.subRanges[i] = subTextRangeModel;

                    if (textRangeModel.subRanges[i].IsEmpty)
                    {
                        textRangeModel.subRanges.RemoveAt(i);
                    }
                }
            }
        }

        private void CollapseSiblingSubRanges(ref TextRangeM textRangeModel)
        {
            if (textRangeModel.IsSubRanged)
            {
                for (int i = textRangeModel.subRanges.Count - 1; i > 0; i--)
                {
                    TextRangeM subTextRangeModel = textRangeModel.subRanges[i];
                    CollapseSiblingSubRanges(ref subTextRangeModel);
                    textRangeModel.subRanges[i] = subTextRangeModel;
                }

                for (int i = textRangeModel.subRanges.Count - 1; i > 0; i--)
                {
                    if (textRangeModel.subRanges[i - 1] | textRangeModel.subRanges[i])
                    {
                        TextRangeM textRangeModelA = textRangeModel.subRanges[i - 1];
                        TextRangeM textRangeModelB = textRangeModel.subRanges[i];

                        if (!textRangeModelB.isNewLine)
                        {
                            MergeRanges(ref textRangeModel, ref textRangeModelA, ref textRangeModelB);
                            textRangeModel.subRanges[i - 1] = textRangeModelA;
                            textRangeModel.subRanges.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void MergeRanges(ref TextRangeM textRangeModel, ref TextRangeM A, ref TextRangeM B)
        {
            if (A.IsContent && B.IsContent)
            {
                A.content += B.content;
            }
            else if (A.IsContent && B.IsSubRanged)
            {
                A.subRanges.Add(new TextRangeM() { content = A.content });
                A.subRanges.AddRange(B.subRanges);

                A.content = string.Empty;
            }
            else if (A.IsSubRanged && B.IsContent)
            {
                A.subRanges.Add(new TextRangeM() { content = B.content });
            }
            else if (A.IsSubRanged && B.IsSubRanged)
            {
                A.subRanges.AddRange(B.subRanges);
            }
        }

        private void ClearUpNodesNames(List<BaseM> items, Dictionary<string, string> namesMapping)
        {
            foreach (var item in items)
            {
                if (item is FolderM folder)
                {
                    ClearUpNodesNames(folder.content, namesMapping);
                }
                else if (item is GraphM graph)
                {
                    foreach (var node in graph.nodes)
                    {
                        namesMapping.Add(node.id, node.name);
                        node.name = null;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
        }

        private void FixUpNodesNames(List<BaseM> items, Dictionary<string, string> namesMapping)
        {
            foreach (var item in items)
            {
                if (item is FolderM folder)
                {
                    FixUpNodesNames(folder.content, namesMapping);
                }
                else if (item is GraphM graph)
                {
                    foreach (var node in graph.nodes)
                    {
                        node.name = namesMapping[node.id];
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
        }

        private void FillNodesNames(List<BaseM> items)
        {
            foreach (var item in items)
            {
                if (item is FolderM folder)
                {
                    FillNodesNames(folder.content);
                }
                else if (item is GraphM graph)
                {
                    foreach (var node in graph.nodes)
                    {
                        if (node is Node_RegularM regularNode)
                        {
                            node.name = string.Format("[{0}]: {1}", ActiveContext.GetCharacter(regularNode.characterId)?.name ?? "???", node.rtDescription.ToString()); ////// TODO DUPLICATION
                        }
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
        }

        private void SetDataContext(StorylineVM storylineVM)
        {
            ActiveContext.Storyline = storylineVM;
            DataContext = storylineVM;
            
            if (storylineVM != null)
            {
                FillNodesNames(storylineVM.Model.dialogs);
                FillNodesNames(storylineVM.Model.replicas);
            }
        }

        private void btn_Config_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Resources["String_DlgWindow_Title"] = GetLocalizedString("String_Tag_Config_Title");

            new DlgWindow()
            {
                Owner = this,
                DataContext = new ConfigVM(ConfigM.Config),
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStyle = WindowStyle.ToolWindow,
                MinWidth = 256,
                ResizeMode = ResizeMode.NoResize,
            }.ShowDialog();
        }

        private void IterateThrough(List<BaseM> modelCollection, Dictionary<string, int> idsDictionary, List<string> invalidLinks)
        {
            foreach (var itemModel in modelCollection)
            {
                if (!idsDictionary.ContainsKey(itemModel.id)) idsDictionary.Add(itemModel.id, 0);
                idsDictionary[itemModel.id]++;

                if (itemModel is FolderM folderModel)
                {
                    IterateThrough(folderModel.content, idsDictionary, invalidLinks);
                }
                else if (itemModel is GraphM graphModel)
                {
                    foreach (var linkModel in graphModel.links)
                    {
                        if (!idsDictionary.ContainsKey(linkModel.id)) idsDictionary.Add(linkModel.id, 0);
                        idsDictionary[linkModel.id]++;

                        if (string.IsNullOrEmpty(linkModel.fromNodeId) ||
                            string.IsNullOrEmpty(linkModel.toNodeId) ||
                            linkModel.fromNodeId == linkModel.toNodeId)
                        {
                            invalidLinks.Add(linkModel.id);
                        }
                        else
                        {
                            string linkModelSignature = linkModel.fromNodeId + " --->>> " + linkModel.toNodeId;
                            if (!idsDictionary.ContainsKey(linkModelSignature)) idsDictionary.Add(linkModelSignature, 0);
                            idsDictionary[linkModelSignature]++;
                        }
                    }

                    foreach (var nodeModel in graphModel.nodes)
                    {
                        if (!idsDictionary.ContainsKey(nodeModel.id)) idsDictionary.Add(nodeModel.id, 0);
                        idsDictionary[nodeModel.id]++;

                        if (nodeModel is Node_InteractiveM interactiveNodeModel)
                        {
                            foreach (var predicateModel in interactiveNodeModel.predicates)
                            {
                                if (!idsDictionary.ContainsKey(predicateModel.id)) idsDictionary.Add(predicateModel.id, 0);
                                idsDictionary[predicateModel.id]++;
                            }

                            foreach (var gameEventsModel in interactiveNodeModel.gameEvents)
                            {
                                if (!idsDictionary.ContainsKey(gameEventsModel.id)) idsDictionary.Add(gameEventsModel.id, 0);
                                idsDictionary[gameEventsModel.id]++;
                            }
                        }
                    }
                }
            }
        }

        private void ValidateModel(StorylineM storylineModel)
        {
            Dictionary<string, int> idsDictionary = new Dictionary<string, int>();
            idsDictionary.Add(storylineModel.id, 1);

            List<string> invalidLinks = new List<string>();

            IterateThrough(storylineModel.locations, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.characters, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.items, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.actors, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.journal, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.dialogs, idsDictionary, invalidLinks);
            IterateThrough(storylineModel.replicas, idsDictionary, invalidLinks);

            List<string> duplicateIds = new List<string>();

            foreach (var idsEntry in idsDictionary)
            {
                if (idsEntry.Value > 1) duplicateIds.Add(idsEntry.Key);
            }

            string invalidLinksStr = string.Join(Environment.NewLine, invalidLinks);
            string duplicateIdsStr = string.Join(Environment.NewLine, duplicateIds);

            if (duplicateIds.Count + invalidLinks.Count > 0)
            {
                throw new InvalidDataException("Data validation fault! Model contains duplicates or invalid elements!");
            }
        }

        public void ShowDialog(object dataContext)
        {
            if (dataContext is HistoryVM)
            {
                App.Current.Resources["String_DlgWindow_Title"] = GetLocalizedString("String_Tag_Player_Title");

                new DlgWindow()
                {
                    Owner = this,
                    DataContext = dataContext,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStyle = WindowStyle.ToolWindow,
                    MinWidth = 256,
                    ResizeMode = ResizeMode.NoResize
                }.ShowDialog();
            }
            else
            {
                new DlgWindow()
                {
                    Owner = this,
                    DataContext = dataContext,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStyle = WindowStyle.ToolWindow,
                    MinWidth = 256,
                    ResizeMode = ResizeMode.NoResize,
                    ContentTemplate = App.Current.Resources["DT_Graph_Stats"] as DataTemplate,
                    Title = ((IWithModel)dataContext).GetModel<GraphM>().name
                }.Show();
            }
        }

        public void InitializeLoc()
        {
            if (ConfigM.Config.Language == LANGUAGE.UNSET)
            {
                if (CultureInfo.InstalledUICulture.ThreeLetterISOLanguageName == "rus")
                {
                    ConfigM.Config.Language = LANGUAGE.RUS;
                    ActiveContext.FileService.SaveConfig();
                    SwitchLocalization(LANGUAGE.RUS);
                }
                else if (CultureInfo.InstalledUICulture.ThreeLetterISOLanguageName == "ukr")
                {
                    ConfigM.Config.Language = LANGUAGE.UKR;
                    ActiveContext.FileService.SaveConfig();
                    SwitchLocalization(LANGUAGE.UKR);
                }
                else
                {
                    ConfigM.Config.Language = LANGUAGE.ENG;
                    ActiveContext.FileService.SaveConfig();
                    SwitchLocalization(LANGUAGE.ENG);
                }
            }
            else if (ConfigM.Config.Language == LANGUAGE.RUS)
            {
                SwitchLocalization(LANGUAGE.RUS);
            }
            else if (ConfigM.Config.Language == LANGUAGE.UKR)
            {
                SwitchLocalization(LANGUAGE.UKR);
            }
            else if (ConfigM.Config.Language == LANGUAGE.ENG)
            {
                SwitchLocalization(LANGUAGE.ENG);
            }
            else // Fix if have invalid value (out of enum)
            {
                ConfigM.Config.Language = LANGUAGE.ENG;
                ActiveContext.FileService.SaveConfig();
                SwitchLocalization(LANGUAGE.ENG);
            }
        }

        public void SwitchLocalization(byte language)
        {
            string suffix = GetLocalizationSuffix(language);

            SetWithSuffix("String_Tag_IsDownloading", suffix);
            SetWithSuffix("String_Tag_ComboBoxExt_Filter_Tooltip", suffix);
            SetWithSuffix("String_Stats_Characters", suffix);
            SetWithSuffix("String_Stats_Types", suffix);
            SetWithSuffix("String_Stats_Node_StepM_TmpDescription", suffix);
            SetWithSuffix("String_Stats_Node_AlternativeM_TmpDescription", suffix);
            SetWithSuffix("String_Stats_Node_RandomM_TmpDescription", suffix);
            SetWithSuffix("String_Stats_Node_TransitM_TmpDescription", suffix);
            SetWithSuffix("String_Stats_Node_GateM_TmpDescription", suffix);
            SetWithSuffix("String_Stats_Node_ExitM_TmpDescription", suffix);
            SetWithSuffix("String_Stats_Node_DialogM_TmpDescription", suffix);
            SetWithSuffix("String_Stats_Node_ReplicaM_TmpDescription", suffix);
            SetWithSuffix("String_Stats_Node_DelayM_TmpDescription", suffix);
            SetWithSuffix("String_Tag_Config_Language", suffix);
            SetWithSuffix("String_Tag_Config_UserActions", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_CreateNode", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_DuplicateNode", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_Link", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_DragAndScroll", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_SelectionSimple", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_SelectionAdditive", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_SelectionBox", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_AlignHor", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_AlignVer", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_Copy", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_Paste", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_Cut", suffix);
            SetWithSuffix("String_Tag_Config_UserActions_Delete", suffix);
            SetWithSuffix("String_Tag_GlobalFilter_Tooltip", suffix);
            SetWithSuffix("String_Tag_MainMenu_Locations", suffix);
            SetWithSuffix("String_Tag_MainMenu_Characters", suffix);
            SetWithSuffix("String_Tag_MainMenu_Items", suffix);
            SetWithSuffix("String_Tag_MainMenu_Actors", suffix);
            SetWithSuffix("String_Tag_MainMenu_Journal", suffix);
            SetWithSuffix("String_Tag_MainMenu_Dialogs", suffix);
            SetWithSuffix("String_Tag_MainMenu_Replicas", suffix);
            SetWithSuffix("String_Tag_MainMenu_DialogsAndReplicas", suffix);
            SetWithSuffix("String_Tag_Name", suffix);
            SetWithSuffix("String_Tag_CustomStringParam", suffix);
            SetWithSuffix("String_Tag_Description", suffix);
            SetWithSuffix("String_Tag_HasDescriptionFemale", suffix);
            SetWithSuffix("String_Tag_DescriptionMale", suffix);
            SetWithSuffix("String_Tag_DescriptionFemale", suffix);
            SetWithSuffix("String_Tag_Relation", suffix);
            SetWithSuffix("String_Tag_RelationFemale", suffix);
            SetWithSuffix("String_Tag_HasInternalDescription", suffix);
            SetWithSuffix("String_Tag_HasInternalDescriptionFemale", suffix);
            SetWithSuffix("String_Tag_InternalDescription", suffix);
            SetWithSuffix("String_Tag_InternalDescriptionMale", suffix);
            SetWithSuffix("String_Tag_InternalDescriptionFemale", suffix);
            SetWithSuffix("String_Tag_Graph_Tab_Props", suffix);
            SetWithSuffix("String_Tag_Graph_Tab_View", suffix);
            SetWithSuffix("String_Tag_Node_Step_Name", suffix);
            SetWithSuffix("String_Tag_Node_Step_Description", suffix);
            SetWithSuffix("String_Tag_Node_Step_Result", suffix);
            SetWithSuffix("String_Tag_Node_Step_Type", suffix);
            SetWithSuffix("String_Tag_Node_Alternative_Name", suffix);
            SetWithSuffix("String_Tag_Node_Alternative_Description", suffix);
            SetWithSuffix("String_Tag_Node_Alternative_Result", suffix);
            SetWithSuffix("String_Tag_Node_Alternative_Type", suffix);
            SetWithSuffix("String_Tag_Node_Tab_Props", suffix);
            SetWithSuffix("String_Tag_Node_Tab_Predicates", suffix);
            SetWithSuffix("String_Tag_Node_Tab_GameEvents", suffix);
            SetWithSuffix("String_Tag_Node_Tab_Behaviors", suffix);
            SetWithSuffix("String_Tag_Node_Random_Type", suffix);
            SetWithSuffix("String_Tag_Node_Transit_Type", suffix);
            SetWithSuffix("String_Tag_Node_Gate_Type", suffix);
            SetWithSuffix("String_Tag_Node_Exit_Type", suffix);
            SetWithSuffix("String_Tag_Node_Replica_Type", suffix);
            SetWithSuffix("String_Tag_Node_Dialog_Type", suffix);
            SetWithSuffix("String_Tag_Node_Delay_Type", suffix);
            SetWithSuffix("String_Tag_Dialog_DialogCharacter", suffix);
            SetWithSuffix("String_Tag_Dialog_DialogLocation", suffix);
            SetWithSuffix("String_Tag_Dialog_ReplicaLocation", suffix);
            SetWithSuffix("String_Tag_Node_Regular_Character", suffix);
            SetWithSuffix("String_Tag_Node_Gate_TargetDialog", suffix);
            SetWithSuffix("String_Tag_Node_Gate_TargetExitNode", suffix);
            SetWithSuffix("String_Tag_Node_Delay_Delay", suffix);
            SetWithSuffix("String_Tag_Node_Regular_OverrideName", suffix);
            SetWithSuffix("String_Tag_Node_Regular_FileStorageType", suffix);
            SetWithSuffix("String_Tag_Node_Regular_FileHttpRef", suffix);
            SetWithSuffix("String_Tag_Node_Regular_ShortDescription", suffix);
            SetWithSuffix("String_Tag_StorageType_GoogleDrive", suffix);
            SetWithSuffix("String_Tag_Predicate_IsInversed", suffix);
            SetWithSuffix("String_Tag_Predicate_P_CompositeM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_HasM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_Node_Has_ActiveSession_CmpM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_Node_Has_ActiveSessionM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_Node_Has_PrevSessions_CmpM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_Node_Has_PrevSessionsM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Item_HasM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Quest_AddedM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Quest_FinishedM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Quest_Node_AddedM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Quest_Node_PassedM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Relation_HasM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Predicate_P_CompositeM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_HasM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_Node_Has_ActiveSession_CmpM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_Node_Has_ActiveSessionM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_Node_Has_PrevSessions_CmpM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Dialog_Node_Has_PrevSessionsM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Item_HasM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Quest_AddedM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Quest_FinishedM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Quest_Node_AddedM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Quest_Node_PassedM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_P_Relation_HasM_Title", suffix);
            SetWithSuffix("String_Tag_Predicate_Compare", suffix);
            SetWithSuffix("String_Tag_Predicate_DialogOrReplica", suffix);
            SetWithSuffix("String_Tag_Predicate_Node", suffix);
            SetWithSuffix("String_Tag_Predicate_Item", suffix);
            SetWithSuffix("String_Tag_Predicate_Quest", suffix);
            SetWithSuffix("String_Tag_Predicate_Character", suffix);
            SetWithSuffix("String_Tag_Predicate_PredicateA", suffix);
            SetWithSuffix("String_Tag_Predicate_PredicateB", suffix);
            SetWithSuffix("String_Tag_GameEvent_IsExecutedOnLeave", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_CustomM_DisplayName", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Item_DropM_DisplayName", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Item_PickUpM_DisplayName", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Quest_AddM_DisplayName", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Quest_Node_AddM_DisplayName", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Quest_Node_PassM_DisplayName", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Relation_ChangeM_DisplayName", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_CustomM_Title", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Item_DropM_Title", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Item_PickUpM_Title", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Quest_AddM_Title", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Quest_Node_AddM_Title", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Quest_Node_PassM_Title", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Relation_ChangeM_Title", suffix);
            SetWithSuffix("String_Tag_GameEvent_GE_Relation_ChangeM", suffix);
            SetWithSuffix("String_Tag_Behavior_B_OptionalM_Title", suffix);
            SetWithSuffix("String_Tag_Behavior_B_OptionalM_DisplayName", suffix);
            SetWithSuffix("String_Tag_Behavior_B_OptionalM_SkipChance", suffix);
            SetWithSuffix("String_Tag_Player_Tab_Props", suffix);
            SetWithSuffix("String_Tag_Player_FullMode", suffix);
            SetWithSuffix("String_Tag_Player_Error", suffix);
            SetWithSuffix("String_Tag_Player_PlayRate", suffix);
            SetWithSuffix("String_Tag_Player_Duration", suffix);
            SetWithSuffix("String_Tag_Player_QuestNodeIsPassed", suffix);
            SetWithSuffix("String_MainMenu_Open_Tooltip", suffix);
            SetWithSuffix("String_MainMenu_Save_Tooltip", suffix);
            SetWithSuffix("String_MainMenu_Condig_Tooltip", suffix);
            SetWithSuffix("String_TabMenu_Up_Tooltip", suffix);
            SetWithSuffix("String_TabMenu_Add_Tooltip", suffix);
            SetWithSuffix("String_TabMenu_AddFolder_Tooltip", suffix);
            SetWithSuffix("String_TabMenu_Remove_Tooltip", suffix);
            SetWithSuffix("String_TabMenu_Cut_Tooltip", suffix);
            SetWithSuffix("String_TabMenu_Paste_Tooltip", suffix);
            SetWithSuffix("String_GraphMenu_PrevRootNode_Tooltip", suffix);
            SetWithSuffix("String_GraphMenu_NextRootNode_Tooltip", suffix);
            SetWithSuffix("String_GraphMenu_ResetScale_Tooltip", suffix);
            SetWithSuffix("String_GraphMenu_GoToOrigin_Tooltip", suffix);
            SetWithSuffix("String_GraphMenu_Play_Tooltip", suffix);
            SetWithSuffix("String_HasPredicates_Tooltip", suffix);
            SetWithSuffix("String_HasGameEvents_Tooltip", suffix);
            SetWithSuffix("String_HasBehaviors_Tooltip", suffix);
            SetWithSuffix("String_Player_Stop_Tooltip", suffix);
            SetWithSuffix("String_Player_Pause_Tooltip", suffix);
            SetWithSuffix("String_Player_Play_Tooltip", suffix);
            SetWithSuffix("String_Bold_Tooltip", suffix);
            SetWithSuffix("String_Italic_Tooltip", suffix);
            SetWithSuffix("String_Underline_Tooltip", suffix);
            SetWithSuffix("String_Error_Line1", suffix);
            SetWithSuffix("String_Error_Line2", suffix);
            SetWithSuffix("String_Error_Line3", suffix);
            SetWithSuffix("String_Error_Line4", suffix);
            SetWithSuffix("String_Tag_Config_Title", suffix);
            SetWithSuffix("String_Tag_Player_Title", suffix);
            SetWithSuffix("String_New_Storyline", suffix);
            SetWithSuffix("String_Player", suffix);
            SetWithSuffix("String_New_Folder", suffix);
            SetWithSuffix("String_New_Location", suffix);
            SetWithSuffix("String_New_Character", suffix);
            SetWithSuffix("String_New_Item", suffix);
            SetWithSuffix("String_New_Actor", suffix);
            SetWithSuffix("String_New_Quest", suffix);
            SetWithSuffix("String_New_Dialog", suffix);
            SetWithSuffix("String_New_Replica", suffix);

            // Update main window title if it is new project

            SetTitleForMainWindow();

            App.Current.Resources["String_DlgWindow_Title"] = GetLocalizedString("String_Tag_Config_Title");
        }

        protected void SetWithSuffix(string key, string suffix)
        {
            App.Current.Resources[key] = App.Current.Resources[string.Format("{0}_{1}", key, suffix)];
        }

        protected string GetLocalizationSuffix(byte language)
        {
            if (language == LANGUAGE.RUS) return "RUS";

            if (language == LANGUAGE.UKR) return "UKR";

            return "ENG"; // English is default
        }

        public string GetLocalizedString(string key)
        {
            string suffix = GetLocalizationSuffix(ConfigM.Config.Language);
            return App.Current.Resources[string.Format("{0}_{1}", key, suffix)]?.ToString();
        }
    }
}