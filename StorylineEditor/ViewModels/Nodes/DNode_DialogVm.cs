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
using System.Linq;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [XmlRoot]
    public class DNode_DialogVm : DNode_CharacterVm
    {
        public DNode_DialogVm(TreeVm Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            finInteractivePart = false;
        }

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

            var newNode = new Node_DialogM(ticks)
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

            model = newNode;

            var times = id.Replace("DNode_DialogVm_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            return model;
        }

    public DNode_DialogVm() : this(null, 0) { }

        public override bool IsValid
        {
            get
            {
                return base.IsValid && (!FinInteractivePart ||
                    Parent.NodesTraversal(this, false).ToList().TrueForAll(childNode => !(childNode is IOwnered ownered) || ownered.Owner.Id != CharacterVm.PlayerId));
            }
        }

        protected bool finInteractivePart;
        public bool FinInteractivePart
        {
            get
            {
                return finInteractivePart;
            }
            set
            {
                if (value != finInteractivePart)
                {
                    finInteractivePart = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is DNode_DialogVm casted)
            {
                casted.finInteractivePart = finInteractivePart;
            }
        }
    }
}
