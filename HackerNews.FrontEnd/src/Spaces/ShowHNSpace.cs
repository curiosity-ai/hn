using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Mosaik;
using UID;

namespace HackerNews
{
    public class ShowHNSpace : IComponent
    {
        private IComponent _content;

        public ShowHNSpace(Parameters state)
        {
            _content = HubStack("Show HN", Routes.ShowHN, DefaultRoutes.Home)
                            .Section(SearchArea().OnSearch(sr => sr.SetBeforeTypesFacet(N.Story.Type)
                                                                     .SetRelatedFacet("Status", new[] { new UID128("cfmdg4M86HWRDkSX25Z3JQ"), new UID128("huV7xMXdjsXHMGstERz2gy") }, invertedBehaviour: true, applyBefore: true)
                                                                     .SetRelatedFacet("SubmissionType", new[] { new UID128("WGr4VNGvC3PZnVwkqzWTB5") }, applyBefore: true)
                                                                     .StoreOnRoute())
                                                   .WithFacets()
                                                   .InitializeFromRoute(state)
                                                   .S(), grow: true, customPadding: "0 8px 0 0");
        }

        public dom.HTMLElement Render() => _content.Render();
    }

}