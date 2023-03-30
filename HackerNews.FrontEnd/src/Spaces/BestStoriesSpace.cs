using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Mosaik;
using UID;

namespace HackerNews
{
    public class BestStoriesSpace : IComponent
    {
        private IComponent _content;

        public BestStoriesSpace(Parameters state)
        {
            _content = HubStack("Best Stories", Routes.BestStories, DefaultRoutes.Home)
                            .Section(Defer(async () => 
                            {
                                var currentLatestNode = await Mosaik.API.Nodes.GetAsync(N.Property.Type, "LastWhen");

                                return SearchArea().OnSearch(sr => sr.SetBeforeTypesFacet(N.Story.Type)
                                                                 .WithTargetQuery(Mosaik.API.Query.StartAt(N.BestStories.Type, currentLatestNode.GetString(N.Property.ValueString)).Out(N.Story.Type).Take(500))
                                                                 .SetRelatedFacet("Status", new[] { new UID128("cfmdg4M86HWRDkSX25Z3JQ"), new UID128("huV7xMXdjsXHMGstERz2gy") }, invertedBehaviour: true, applyBefore: true)
                                                                 .StoreOnRoute())
                                                   .WithFacets()
                                                   .InitializeFromRoute(state)
                                                   .S();
                            }).S(), grow: true, customPadding: "0 8px 0 0");
        }

        public dom.HTMLElement Render() => _content.Render();
    }
}