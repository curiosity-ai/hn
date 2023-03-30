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

            Router.Register(Routes.TopStories,  state => App.ShowDefault(new TopStoriesSpace(state)));
            Router.Register(Routes.NewStories,  state => App.ShowDefault(new NewStoriesSpace(state)));
            Router.Register(Routes.BestStories, state => App.ShowDefault(new BestStoriesSpace(state)));
            Router.Register(Routes.ShowHN,      state => App.ShowDefault(new ShowHNSpace(state)));
            Router.Register(Routes.AskHN,       state => App.ShowDefault(new AskHNSpace(state)));
            Router.Register(Routes.Hiring,      state => App.ShowDefault(new HiringSpace(state)));
            Router.Register(Routes.AllStories,  state => App.ShowDefault(new AllStoriesSpace(state)));
            Router.Register(Routes.Authors,     state => App.ShowDefault(new AuthorsSpace(state)));
            Router.Register(Routes.Trends,      state => App.ShowDefault(new TrendsSpace(state)));

            App.Initialize(Configure, OnLoad);
        }

        private static void Configure(App.DefaultSettings settings)
        {
            App.Sidebar.OnSidebarRebuild_AfterHeader += AddCustomSpaces;
            App.Settings.HomeView = (state) => { Router.Replace(Routes.TopStories); return new TopStoriesSpace(state); } ;
        }

        private static void AddCustomSpaces(Sidebar sidebar, App.Sidebar.RefreshTracker refreshTracker)
        {
            var topStoriesBtn  = new SidebarButton("ec ec-fire",                        "Top Stories").OnClick(() =>  Router.Navigate(Routes.TopStories));
            var newStoriesBtn  = new SidebarButton("ec ec-rocket",                      "New Stories").OnClick(() =>  Router.Navigate(Routes.NewStories));
            var bestStoriesBtn = new SidebarButton("ec ec-trophy",                      "Best Stories").OnClick(() => Router.Navigate(Routes.BestStories));
            var showHNBtn      = new SidebarButton("ec ec-eyes",                        "Show HN").OnClick(() =>      Router.Navigate(Routes.ShowHN));
            var askHNBtn       = new SidebarButton("ec ec-raised-eyebrow",              "Ask HN").OnClick(() =>       Router.Navigate(Routes.AskHN));
            var hiringBtn      = new SidebarButton("ec ec-construction-worker-man",     "Hiring").OnClick(() =>       Router.Navigate(Routes.Hiring));
            var allStoriesBtn  = new SidebarButton("ec ec-dizzy",                       "All Stories").OnClick(() =>  Router.Navigate(Routes.AllStories));
            var trendsBtn      = new SidebarButton("ec ec ec-chart-with-upwards-trend", "Trends").OnClick(() =>       Router.Navigate(Routes.Trends));
            var authorsBtn     = new SidebarButton("ec ec-writing-hand",                "Authors").OnClick(() =>      Router.Navigate(Routes.Authors));

            sidebar.AddContent(topStoriesBtn);
            sidebar.AddContent(bestStoriesBtn);
            sidebar.AddContent(newStoriesBtn);
            sidebar.AddContent(showHNBtn);
            sidebar.AddContent(askHNBtn);
            sidebar.AddContent(hiringBtn);
            sidebar.AddContent(allStoriesBtn);
            sidebar.AddContent(trendsBtn);
            sidebar.AddContent(authorsBtn);

            refreshTracker.Add(() => topStoriesBtn.IsSelected  = window.location.href.Contains(Routes.TopStories));
            refreshTracker.Add(() => bestStoriesBtn.IsSelected = window.location.href.Contains(Routes.BestStories));
            refreshTracker.Add(() => newStoriesBtn.IsSelected  = window.location.href.Contains(Routes.NewStories));
            refreshTracker.Add(() => showHNBtn.IsSelected      = window.location.href.Contains(Routes.ShowHN));
            refreshTracker.Add(() => askHNBtn.IsSelected       = window.location.href.Contains(Routes.AskHN));
            refreshTracker.Add(() => hiringBtn.IsSelected      = window.location.href.Contains(Routes.Hiring));
            refreshTracker.Add(() => allStoriesBtn.IsSelected  = window.location.href.Contains(Routes.AllStories));
            refreshTracker.Add(() => trendsBtn.IsSelected      = window.location.href.Contains(Routes.Trends));
            refreshTracker.Add(() => authorsBtn.IsSelected     = window.location.href.Contains(Routes.Authors));
        }

        private static void OnLoad()
        {

        }
    }
}