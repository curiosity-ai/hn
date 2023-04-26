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
using System.Collections.Generic;
using Mosaik.Helpers;

namespace HackerNews
{
    public class UserRenderer : INodeRenderer
    {
        public string NodeType => N.User.Type;
        public string DisplayName => "Author";
        public string LabelField => "Name";
        public string Color => "#106ebe";
        public string Icon => "user";

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
            Router.Replace(Routes.AuthorId(node.GetString("Name")));

            return (await PreviewAsync(node, state)).Merge();
        }

        private IComponent CreateView(Node node, Parameters state)
        {
            return Pivot().Pivot("submissions", PivotTitle("Submissions"), () => Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Story.Type, E.AuthorOf).Take(500_000).GetUIDsAsync(), showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, targetNodeTypes: new[] { N.Story.Type }))
                          .Pivot("domains",     PivotTitle("Domains"),     () => Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Story.Type, E.AuthorOf).Out(N.Domain.Type, E.HasDomain).Take(100_000).GetUIDsAsync(), showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, targetNodeTypes: new[] { N.Domain.Type }))
                          .Pivot("comments",    PivotTitle("Comments"),    () => Neighbors(() => Mosaik.API.Query.StartAt(node.UID).Out(N.Comment.Type, E.AuthorOf).Take(500_000).GetUIDsAsync(), showSearchBox: true, facetDisplay: FacetDisplayOptions.Visible, targetNodeTypes: new[] { N.Comment.Type }))
                          .Pivot("activity",    PivotTitle("Activity"),    () => CreateActivityDashboard(node));

        }

        private IComponent CreateActivityDashboard(Node node)
        {
            return Defer(async () =>
            {
                var activity = await Mosaik.API.Endpoints.CallAsync<ActivityResponse>("author-activity", node.UID);
                await ExternalLibraries.LoadPlotlyAsync();

                var charts = VStack().WS().ScrollY();



                return charts;
            }).S();
        }

        class ActivityResponse
        {
            public IDictionary<string, int> SubmissionsTimeline { get; set; }
            public IDictionary<string, int> CommentsTimeline { get; set; }
            public IDictionary<string, int> SubmissionsHourlyActivity { get; set; }
            public IDictionary<string, int> CommentsHourlyActivity { get; set; }
            public IDictionary<string, int> SubmissionsWeeklyActivity { get; set; }
            public IDictionary<string, int> CommentsWeeklyActivity { get; set; }
        }
    }
}