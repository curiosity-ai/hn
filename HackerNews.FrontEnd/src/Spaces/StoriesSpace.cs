using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Mosaik;
using UID;

namespace HackerNews
{
    public class StoriesSpace : IComponent
    {
        private IComponent _content;

        public StoriesSpace(Parameters state)
        {
            //TODO: Add content from current hacker news landing page if empty search

            _content = HubStack("Stories", Routes.Stories, DefaultRoutes.Home)
                            .Section(SearchArea().OnSearch(sr => sr.SetBeforeTypesFacet(N.Story.Type)
                                                                   .SetRelatedFacet("Status", new[] { new UID128("cfmdg4M86HWRDkSX25Z3JQ"), new UID128("huV7xMXdjsXHMGstERz2gy") }, invertedBehaviour: true, applyBefore: true))
                                                 .WithFacets().S(), grow: true, customPadding: "0 8px 0 0");
        }

        public dom.HTMLElement Render() => _content.Render();
    }
}