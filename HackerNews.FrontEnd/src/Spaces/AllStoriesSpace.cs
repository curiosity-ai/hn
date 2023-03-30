using H5.Core;
using Tesserae;
using static Mosaik.UI;
using Mosaik;
using UID;

namespace HackerNews
{
    public class AllStoriesSpace : IComponent
    {
        private IComponent _content;

        public AllStoriesSpace(Parameters state)
        {
            _content = HubStack("Stories", Routes.AllStories, DefaultRoutes.Home)
                            .Section(SearchArea().OnSearch(sr => sr.SetBeforeTypesFacet(N.Story.Type)
                                                                   .SetRelatedFacet("Status", new[] { new UID128("cfmdg4M86HWRDkSX25Z3JQ"), new UID128("huV7xMXdjsXHMGstERz2gy") }, invertedBehaviour: true, applyBefore: true)
                                                                   .StoreOnRoute())
                                                 .WithFacets()
                                                 .InitializeFromRoute(state)
                                                 .S(), grow: true, customPadding: "0 8px 0 0");
        }

        public dom.HTMLElement Render() => _content.Render();
    }
}