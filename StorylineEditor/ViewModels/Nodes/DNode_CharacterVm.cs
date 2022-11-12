/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.Predicates;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Регулярная вершина")]
    [XmlRoot]
    public class DNode_CharacterVm : Node_InteractiveVm, IOwnered
    {
        public DNode_CharacterVm(TreeVm Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            OwnerId = CharacterVm.PlayerId;
        }

        public DNode_CharacterVm() : this(null, 0) { }


        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var documentDescription = Name;

            if (!string.IsNullOrEmpty(documentDescription) && !documentDescription.Contains("FlowDocument"))
            {
                var document = new FlowDocument();
                document.Blocks.Add(new Paragraph(new Run(Name)));
                documentDescription = XamlWriter.Save(document);
            }

            var newNode = new Node_ReplicaM(ticks)
            {
                name = null,
                characterId = Owner?.GetModel(ticks, idReplacer)?.id,
                overrideName = OverrideOwnerName,
                fileHttpRef = AttachedFile,
                description = documentDescription,
                shortDescription = description,
                gender = (byte)Gender,
                positionX = PositionX,
                positionY = PositionY,
            };

            int k = 0;
            foreach (var gameEvent in GameEvents)
            {
                if (gameEvent != null)
                {
                    var gameEventModel = gameEvent.GetModel(k, idReplacer);
                    newNode.gameEvents.Add((GE_BaseM)gameEventModel);
                    if (!idReplacer.ContainsKey(gameEvent.Id)) idReplacer.Add(gameEvent.Id, gameEventModel.id);
                }
                k++;
            }

            k = 0;
            foreach (var predicate in Predicates)
            {
                if (predicate != null)
                {
                    var predicateModel = predicate.GetModel(k, idReplacer);
                    newNode.predicates.Add((P_BaseM)predicateModel);
                    if (!idReplacer.ContainsKey(predicate.Id)) idReplacer.Add(predicate.Id, predicateModel.id);
                }
                k++;
            }

            model = newNode;

            var times = id.Replace("DNode_CharacterVm_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            return model;
        }

        public string OwnerId { get; set; }


        protected string overrideOwnerName;
        public string OverrideOwnerName {
            get => overrideOwnerName;
            set
            {
                if (value != overrideOwnerName)
                {
                    overrideOwnerName = value;
                    NotifyWithCallerPropName();

                    Notify(nameof(OwnerName));
                }
            } 
        }


        public string OwnerName => string.IsNullOrEmpty(OverrideOwnerName) ? Owner?.Name : string.Format("{0} [{1}]", Owner?.Name, OverrideOwnerName);


        [XmlIgnore]
        public FolderedVm Owner
        {
            get => Parent?.Parent?.Parent?.Characters.FirstOrDefault(item => item?.Id == OwnerId);
            set
            {
                if (OwnerId != value?.Id)
                {
                    OwnerId = value?.Id;

                    NotifyWithCallerPropName();
                    Notify(nameof(OwnerId));
                    Notify(nameof(OwnerName));
                    NotifyIsValidChanged();
                }
            }
        }

        protected string attachedFile;
        public string AttachedFile
        {
            get => string.IsNullOrEmpty(attachedFile) ? null : attachedFile;
            set
            {
                if (value != attachedFile)
                {
                    attachedFile = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public override bool IsValid => base.IsValid && Owner != null;

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is DNode_CharacterVm casted)
            {
                casted.OwnerId = OwnerId;                
                casted.attachedFile = attachedFile;
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) ||
            Owner != null && Owner.PassFilter(filter);
    }
}