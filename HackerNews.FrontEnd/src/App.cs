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
using static H5.Core.dom;

namespace HackerNews
{
    internal static class HackernewsApp
    {
        private static void Main()
        {
            Mosaik.Admin.LazyLoad();

            App.ServerURL = "https://hn.curiosity.ai/api";

            Router.Register(Routes.Stories, state => App.ShowDefault(new StoriesSpace(state)));
            Router.Register(Routes.Authors, state => App.ShowDefault(new AuthorsSpace(state)));

            App.Initialize(Configure, OnLoad);
        }

        private static void Configure(App.DefaultSettings settings)
        {
            App.Sidebar.OnSidebarRebuild_AfterHeader += AddCustomSpaces;
        }

        private static void AddCustomSpaces(Sidebar sidebar, App.Sidebar.RefreshTracker refreshTracker)
        {
            var storiesBtn = new SidebarButton("fal fa-notes", "Stories").OnClick(() => Router.Navigate(Routes.Stories));
            var authorsBtn = new SidebarButton("fal fa-users", "Authors").OnClick(() => Router.Navigate(Routes.Authors));

            sidebar.AddContent(storiesBtn);
            sidebar.AddContent(authorsBtn);

            refreshTracker.Add(() => storiesBtn.IsSelected = window.location.href.Contains(Routes.Stories));
            refreshTracker.Add(() => authorsBtn.IsSelected = window.location.href.Contains(Routes.Authors));
        }

        private static void OnLoad()
        {

        }
    }

    public static class Routes
    {
        public const string Stories = "#/stories";
        public const string Authors = "#/authors";
    }
}