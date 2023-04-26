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
using Mosaik.Helpers;

namespace HackerNews
{
    internal static class HackernewsApp
    {
        private static void Main()
        {
            Mosaik.Admin.LazyLoad();

            App.ServerURL = "https://hn.curiosity.ai/api";

            Router.Register(Routes.TopStories,  state => App.ShowDefault(new TopStoriesSpace(state)));
            Router.Register(Routes.Story,       state => OpenStoryAsync(state));
            Router.Register(Routes.Author,      state => OpenAuthorAsync(state));
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

        private static async Task OpenStoryAsync(Parameters state)
        {
            try
            {
                var id = state["id"];
                var node = await Mosaik.API.Nodes.GetAsync("Story", id);
                App.ShowDefault(await new StoryRenderer().ViewAsync(node, state));
            }
            catch
            {
                Router.Navigate(DefaultRoutes.Home);
            }
        }

        private static async Task OpenAuthorAsync(Parameters state)
        {
            try
            {
                var id = state["id"];
                var node = await Mosaik.API.Nodes.GetAsync("User", id);
                App.ShowDefault(await new UserRenderer().ViewAsync(node, state));
            }
            catch
            {
                Router.Navigate(Routes.Authors);
            }
        }

        private static void Configure(App.DefaultSettings settings)
        {
            App.Sidebar.OnSidebarRebuild_AfterHeader += AddCustomSpaces;
            App.Settings.HomeView = (state) => { Router.Replace(Routes.TopStories); return new TopStoriesSpace(state); } ;

            Facets.RegisterCustomFacetRenderer("Related.Karma",         "Karma",          item => RenderKarma(item));
            Facets.RegisterCustomFacetRenderer("Related.PostFrequency", "Post Frequency", item => RenderPostFrequency(item));
            Facets.RegisterCustomFacetRenderer("Related.Upvotes",       "Upvotes",        item => RenderUpvotes(item));

            IComponent RenderKarma(IFacetItem item)
            {
                string text;
                string range;
                switch (item.UID)
                {
                    case "X1dPrLAzxSQCQQNMdCMeTN": text = "None";            range = "";          break;
                    case "axbALxPtH2eUiumVtAxYMG": text = "Low";             range = "1-49";      break;
                    case "2zVXoBzAMuXB8rsKBf8Nqy": text = "Medium";          range = "50-499";    break;
                    case "eEZY11R1bDcVjzgtoRtJva": text = "High";            range = "500-1000";  break;
                    case "8idDxvBRNpGQT1YEjJu5Np": text = "Very High";       range = "1000-5000"; break;
                    case "8SPJqptEcGXLXV5qSwGmMR": text = "Elite";           range = "5000-9000"; break;
                    case "KBQVXj5p4RuQbqLe3DXmBy": text = "It's Over 9000!"; range = "";          break;
                    default: throw new ArgumentOutOfRangeException();
                }

                return HStack().AlignItemsCenter().Class("custom-facet").Children(
                    TextBlock(text).PR(16),
                    TextBlock(range).Class("custom-facet-range"));
            }

            IComponent RenderUpvotes(IFacetItem item)
            {
                string text;
                string range;
                switch (item.UID)
                {
                    case "H4B2dvWgNpaEZMC1aQwq4W": text = "None";      range = "";          break;
                    case "CpshtDhTq5P463u19NTFL9": text = "Obscure";   range = "1-10";      break;
                    case "2SsRCQcscU7fEAdC6BvwFm": text = "Niche";     range = "10-25";     break;
                    case "EsbbfyxKg37B2GEpZkUhq9": text = "Trending";  range = "25-50";     break;
                    case "Sdvf7XddH9DD62Aef5xxYo": text = "Popular";   range = "50-100";    break;
                    case "KxYZufektURNPRBFkACJ8X": text = "Famous";    range = "100-500";   break;
                    case "dFheE7LKB25LhsMpggie4F": text = "Iconic";    range = "500-1000";  break;
                    case "FdsDw4yXyjDN8gWRyPjJKM": text = "Legendary"; range = "1000-2000"; break;
                    case "FsSgiszbtD1M9TbqQT5xxV": text = "Mythical";  range = "2000-5000"; break;
                    case "PHbEGkoQrru2Me2MM4CxQT": text = "Immortal";  range = "5000+";     break;
                    default: throw new ArgumentOutOfRangeException();
                }

                return HStack().AlignItemsCenter().Class("custom-facet").Children(
                    TextBlock(text).PR(16),
                    TextBlock(range).Class("custom-facet-range"));
            }

            IComponent RenderPostFrequency(IFacetItem item)
            {
                string text;
                string range;
                switch (item.UID)
                {
                    case "iLBHQpEoKBaEzWpitWYSTe": text = "None";    range = "";         break;
                    case "dzEYD4QVzziMpQN8wn5KW2": text = "Few";     range = "1-5";      break;
                    case "ALzUTiaB6hEgGy7XnCbEAC": text = "Several"; range = "5-10";     break;
                    case "CNdSY6H4nDheUYdL5CHwxV": text = "Pack";    range = "10-20";    break;
                    case "T83kLdofdGzHAGHToE9Zfc": text = "Lots";    range = "20-50";    break;
                    case "i3YkoWtzcnA2eseSFDk5kW": text = "Horde";   range = "50-100";   break;
                    case "NFtK1je7wX4dyJmyF1q6MU": text = "Throng";  range = "100-250";  break;
                    case "C4vtjnpcc3SHydeibcBWTJ": text = "Swarm";   range = "250-500";  break;
                    case "1qBjQauJcD18o592JsvqGm": text = "Zounds";  range = "500-1000"; break;
                    case "hMmWmf2nH9nFVvTJj88Py6": text = "Legion";  range = ">1000";    break;
                    default: throw new ArgumentOutOfRangeException();
                }

                return HStack().AlignItemsCenter().Class("custom-facet").Children(
                    TextBlock(text).PR(16),
                    TextBlock(range).Class("custom-facet-range"));
            }


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