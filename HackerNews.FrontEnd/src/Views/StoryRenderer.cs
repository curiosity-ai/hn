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
using System.Threading;
using Mosaik.Helpers;
using static H5.Core.es5;
using Mosaik.FrontEnd.Desktop;
using static PlotlyH5.Literals;
using static Retyped.zip_js.zip;
using Mosaik.Components.Nodes;

namespace HackerNews
{
    public class StoryRenderer : INodeRenderer, INodeCustomStyle
    {
        public string NodeType    => N.Story.Type;
        public string DisplayName => "Story";
        public string LabelField  => "Title";
        public string Color       => "#f35a29";
        public string Icon        => "notes";

        public CardContent CompactView(Node node)
        {
            var url    = node.GetString(N.Story.Url);

            var seeDiscussion = Button().PR(8).Collapse().LessPadding().Tiny().NoMinSize()
                                        .OnClick((b, e) => { StopEvent(e); NodePreview.For(node.UID); });

            var openDiscussionOnHN = Button("Open on HN").SetIcon(UIcons.ArrowUpRightFromSquare, size: TextSize.Tiny, afterText: true).LessPadding().Tiny().NoMinSize()
                                                         .OnClick((b, e) => { StopEvent(e); OpenOnHackerNews(node); });

            var similarStories = Button("Similar stories").LessPadding().Tiny().NoMinSize()
                                             .OnClick((b,e) => { StopEvent(e); OpenSimilarStories(node); });

            var secondLine = HStack().AlignItemsCenter().Class("no-default-margin").Class("story-status-line").Children(
                                    NeighborsLinks(node.UID, nodeType:"Domain", edgeType: "HasDomain").PR(8),
                                    If(node.GetInt(N.Story.Score) > 1, Button($"{node.GetInt(N.Story.Score):n0} points").PR(8).LessPadding().Tiny().NoMinSize().NoBorder().NoHover()),
                                    seeDiscussion,
                                    similarStories,
                                    openDiscussionOnHN);

            Mosaik.API.Aggregated.GetNeighborCount(node.UID, N.Comment.Type, E.HasComment, (c) =>
            {
                if (c > 0)
                {
                    seeDiscussion.SetText("See discussion").Show();
                }
            });

            var header = Header(this, node);

            var card = CardContent(header, secondLine.WS().PL(56));
            
            
            if (!string.IsNullOrEmpty(url))
            {
                card.WithExtraCommands(new[] 
                {
                    new CommandDefinition("Open Link", "l", UIcons.ArrowUpRightFromSquare, () => OpenLink(node, url)),
                    new CommandDefinition("Open on HN", "o", UIcons.ArrowUpRightFromSquare, () => OpenOnHackerNews(node)),
                    new CommandDefinition("Similar Stories", "s", UIcons.MagicWand, () => OpenSimilarStories(node)),
                    new CommandDefinition("See Discussion", "d", UIcons.Comments, () =>  NodePreview.For(node))
                });

                header.OnClick = () => OpenLink(node, url);
            }
            else
            {
                card.WithExtraCommands(new[]
                {
                    new CommandDefinition("Open on HN", "o", UIcons.ArrowUpRightFromSquare, () => OpenOnHackerNews(node)),
                    new CommandDefinition("Similar Stories", "s", UIcons.MagicWand, () => OpenSimilarStories(node)),
                    new CommandDefinition("See Discussion", "d", UIcons.Comments, () =>  NodePreview.For(node))
                });
            }

            return card;
        }

        private static void OpenLink(Node node, string url)
        {
            ElectronBridge.OpenNewWindow(url, true);
            LocalStorage.SetBool($"read-{node.UID}", true);
            SearchRenderer.MaybeRedraw(node.UID);
        }

        private static void OpenOnHackerNews(Node node)
        {
            ElectronBridge.OpenNewWindow($"https://news.ycombinator.com/item?id={node.GetString(N.Story.Id)}", true);
        }

        private static void OpenSimilarStories(Node node)
        {
            var md = Modal().W(80.vw()).H(80.vh()).LightDismiss().ShowCloseButton().SetHeader(TextBlock($"Similar stories to '{node.GetString("Title")}'").SemiBold());
            md.Content(GetSimilarStories(node, md));
            md.Show();
        }

        private static SearchArea GetSimilarStories(Node node, Modal md = null)
        {
            return SearchArea().WithFacets().SearchBox(sb => { if (md is object) { sb.LinkedToModal(md); } })
                                               .OnSearch(sr => sr.SetBeforeTypesFacet(node.Type).WithExcludeUIDs(new[] { node.UID })
                                                                 .WithSimilarityRanking(new SimilarityRanking().WithIndex(new UID64("9RbCqN1agBA")).WithSimilarTo(new[] { node.UID }))).S();
        }

        public string GetColor(Node node) => Color;

        public string GetDisplayName(Node node) => DisplayName;

        public string GetIcon(Node node)
        {
            if (LocalStorage.GetBool($"read-{node.UID}"))
            {
                return "envelope-open";
            }

            return "envelope";
        }

        public string GetLabel(Node node) => node.GetString("Title");

        public async Task<CardContent> PreviewAsync(Node node, Parameters state)
        {
            return CardContent(Header(this, node), CreateView(node, state)).PreviewWidth(80.vw()).PreviewHeight(80.vh())
                .WithExtraCommands(new[]
            {
                new CommandDefinition("Open on HN", "o", UIcons.ArrowUpRightFromSquare, () => OpenOnHackerNews(node)).ShowAlsoOnViewAndPreview(),
            });
        }

        public async Task<IComponent> ViewAsync(Node node, Parameters state)
        {
            Router.Replace(Routes.StoryId(node.GetString("Id")));
            return (await PreviewAsync(node, state)).Merge();
        }

        private IComponent CreateView(Node node, Parameters state)
        {
            LocalStorage.SetBool($"read-{node.UID}", true);
            SearchRenderer.MaybeRedraw(node.UID);

            return Pivot().Pivot("story", PivotTitle("Story"), () => GetStoryView(node, state))
                          .Pivot("similar", PivotTitle("Similar Stories"), () => GetSimilarStories(node))
                          .S();
        }

        private IComponent GetStoryView(Node node, Parameters state)
        {
            var author = Button().Foreground(Theme.Secondary.Foreground).NoPadding().NoMinSize();
            var text = node.GetString(N.Story.Text);
            var url = node.GetString(N.Story.Url);

            RegExp highlighter = null;
            if (state is object && state.TryGetValue(SearchRenderer.HighlighterKey, out var highlighterSource))
            {
                highlighter = new RegExp(highlighterSource, "gmi");
            }

            Mosaik.API.Aggregated.GetNodeNeighbors(node.UID, N.User.Type, E.HasAuthor, (uids) =>
            {
                var uid = uids.First();
                author.OnClick(() => NodePreview.For(uid));
                Mosaik.API.Aggregated.GetNodeLabel(uid, N.User.Name, (n) => author.SetText(n));
            });

            return SearchRenderer.HighlightComponent(VStack().S().Children(
                    VStack().S().ScrollY().Class("no-default-margin").PL(8).Children(
                            If(!string.IsNullOrEmpty(text), TextBlock(text, selectable: true, treatAsHTML: true).WS().BreakSpaces()),
                            HStack().WS().AlignItemsCenter().Children(TextBlock("Posted by"), author.PL(8),
                                                                      TextBlock(node.Timestamp.Humanize()).PL(8),
                                                                      If(!string.IsNullOrEmpty(url), Link(url, Button("Open link").NoPadding().NoMinSize().SetIcon(UIcons.ArrowUpRightFromSquare, afterText: true), noUnderline: true).OpenInNewTab())
                                                                      ),
                            LazyLoadComments(node.UID, highlighter).WS())
                    ), highlighter);
        }

        private IComponent LazyLoadComments(UID128 parent, RegExp highlighter)
        {
            return Defer(async () =>
            {
                var stack = VStack().WS();

                var response = await Mosaik.API.Endpoints.CallAsync<Comment[]>("get-comments", parent);

                if (response.Any())
                {
                    stack.Add(TextBlock("Comments").PT(32).PB(16));
                }

                AddComments(response, stack, highlighter);

                return stack;
            });
        }

        private void AddComments(Comment[] comments, Stack stack, RegExp highlighter, int level = 1)
        {
            foreach (var c in comments)
            {
                stack.Add(SearchRenderer.HighlightComponent(RenderComment(c, level), highlighter));
                AddComments(c.Children, stack, highlighter, level + 1);
            }
        }

        private IComponent RenderComment(Comment c, int level)
        {
            return VStack().WS().PL(42 * level).PB(16).Children(
                HStack().WS().AlignItemsCenter().Children(Button(c.Author.Name).Foreground(Theme.Secondary.Foreground).NoPadding().NoMinSize().OnClick(() => NodePreview.For(c.Author.UID)), TextBlock(c.When.Humanize()).PL(8)),
                TextBlock(c.Text, selectable: true, treatAsHTML: true).BreakSpaces().WS());
        }

        private class Comment
        {
            public UID128 UID { get; set; }
            public DateTimeOffset When { get; set; }
            public string Text { get; set; }
            public Author Author { get; set; }
            public int Score { get; set; }
            public Comment[] Children { get; set; }
        }

        private class Author
        {
            public UID128 UID { get; set; }
            public string Name { get; set; }
        }
    }

}