using UID;
using System;
using System.Linq;
using System.Threading.Tasks;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using Mosaik.Views;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;

namespace HackerNews
{
    public class StatusRenderer : INodeRenderer
    {
        public string NodeType    => N.Status.Type;
        public string DisplayName => "Status";
        public string LabelField  => "Name";
        public string Color       => "#106ebe";
        public string Icon        => "tag";

        public CardContent CompactView(Node node)
        {
            return CardContent(Header(this, node), null);
        }

        public async Task<CardContent> PreviewAsync(Node node, Parameters state)
        {
            return CardContent(Header(this, node), CreateView(node, state));
        }

        public async Task<IComponent> ViewAsync(Node node, Parameters state)
        {
            return (await PreviewAsync(node, state)).Merge();
        }

        private IComponent CreateView(Node node, Parameters state)
        {
            return VStack().S().ScrollY().Children(
                        Label().WS().Inline().AutoWidth().SetContent(TextBlock(node.GetString(N.Status.Name)))
               );

        }
    }
}