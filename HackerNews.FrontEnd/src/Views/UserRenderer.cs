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
    public class UserRenderer : INodeRenderer
    {
        public string NodeType    => N.User.Type;
        public string DisplayName => "User";
        public string LabelField  => "Name";
        public string Color       => "#106ebe";
        public string Icon        => "user";

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
            return Pivot().Pivot("submissions", PivotTitle("Submissions"), () => Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Story.Type, E.AuthorOf).Take(100_000).GetUIDsAsync(), showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, targetNodeTypes: new[] { N.Story.Type }))
                          .Pivot("comments", PivotTitle("Comments"), () => Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Comment.Type, E.AuthorOf).Take(100_000).GetUIDsAsync(), showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, targetNodeTypes: new[] { N.Comment.Type })); ;

        }
    }
}