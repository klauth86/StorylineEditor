/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Nodes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Шаг квеста")]
    [XmlRoot]
    public class JNode_StepVm : Node_BaseVm
    {
        public JNode_StepVm(TreeVm Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            Outs = new ObservableCollection<NodePairVm>();

            if (Parent != null) Parent.Links.CollectionChanged += OnLinksChanged;
        }

        public JNode_StepVm() : this(null, 0) { }

        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var documentDescription = Description;

            if (!string.IsNullOrEmpty(documentDescription) && !documentDescription.Contains("FlowDocument"))
            {
                var document = new FlowDocument();
                document.Blocks.Add(new Paragraph(new Run(Description)));
                documentDescription = XamlWriter.Save(document);
            }

            var newNode = new Node_StepM(ticks)
            {
                name = Name,
                description = documentDescription,
                gender = (byte)Gender,
                positionX = PositionX,
                positionY = PositionY,
                result = null,
            };
            
            model = newNode;

            var times = id.Replace("JNode_StepVm_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            return model;
        }

        ~JNode_StepVm()
        {
            if (Parent != null) Parent.Links.CollectionChanged -= OnLinksChanged;
        }

        private void OnLinksChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (NodePairVm nodePair in e.NewItems)
                {
                    if (nodePair.FromId == id)
                    {
                        Outs.Add(nodePair);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (NodePairVm nodePair in e.OldItems)
                {
                    if (nodePair.FromId == id)
                    {
                        Outs.Remove(nodePair);
                    }
                }
            }
        }

        [XmlIgnore]
        public ObservableCollection<NodePairVm> Outs { get; }

        public override void SetupParenthood()
        {
            Parent.Links.CollectionChanged += OnLinksChanged;

            foreach (var nodePair in Parent.Links.Where(link => link.FromId == id)) Outs.Add(nodePair);
        }
    }
}