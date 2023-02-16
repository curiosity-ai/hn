using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Mosaik;

namespace HackerNews
{
    public class AuthorsSpace : IComponent
    {
        private IComponent _content;
        public AuthorsSpace(Parameters state)
        {
            _content = HubStack("Authors", Routes.Authors, DefaultRoutes.Home)
                            .Section(SearchArea().OnSearch(sr => sr.SetBeforeTypesFacet(N.User.Type)).WithFacets().S(), grow: true, customPadding: "0 8px 0 0");
        }

        public dom.HTMLElement Render() => _content.Render();
    }
}