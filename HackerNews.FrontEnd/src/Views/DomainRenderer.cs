using System.Threading.Tasks;
using Mosaik;
using Mosaik.Components;
using Mosaik.Schema;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;

namespace HackerNews
{
    public class DomainRenderer : INodeRenderer
    {
        public string NodeType => N.Domain.Type;
        public string DisplayName => "Domain";
        public string LabelField => "Host";
        public string Color => "#0e00fb";
        public string Icon => "link";

        public CardContent CompactView(Node node)
        {
            return CardContent(Header(this, node), null);
        }

        public async Task<CardContent> PreviewAsync(Node node, Parameters state)
        {
            return CardContent(Header(this, node), CreateView(node, state)).PreviewHeight(80.vh()).PreviewWidth(80.vw());
        }

        public async Task<IComponent> ViewAsync(Node node, Parameters state)
        {
            return (await PreviewAsync(node, state)).Merge();
        }

        private IComponent CreateView(Node node, Parameters state)
        {
            return Pivot().Pivot("submissions", PivotTitle("Stories"), () => Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Story.Type, E.DomainOf).Take(500_000).GetUIDsAsync(), showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, targetNodeTypes: new[] { N.Story.Type }))
                          .Pivot("authors",     PivotTitle("Authors"),     () => Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Story.Type, E.DomainOf).Out(N.User.Type, E.HasAuthor).Take(500_000).GetUIDsAsync(), showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, targetNodeTypes: new[] { N.User.Type })); ;

        }
    }
}