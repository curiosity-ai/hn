using Mosaik.Components;
using Mosaik.Schema;
using System.Linq;
using Tesserae;

namespace HackerNews
{
    public static class Routes
    {
        public const string AllStories  = "#/stories";
        public const string TopStories  = "#/top-stories";
        public const string NewStories  = "#/new-stories";
        public const string BestStories = "#/best-stories";
        public const string ShowHN      = "#/show-hn";
        public const string AskHN       = "#/ask-hn";
        public const string Hiring      = "#/hiring";
        public const string Authors     = "#/authors";
        public const string Trends      = "#/trends";
        public const string Story       = "#/story";
        public const string Author      = "#/author";

        public static string AuthorId(string id) => $"{Author}?id={id}";
        public static string StoryId(string id) => $"{Story}?id={id}";
        public static string TrendsFor(string[][] words) => Trends + "?terms=" + string.Join(";", words.Select(v => string.Join("+", v)));
    }
}