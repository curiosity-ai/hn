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
            var card = CardContent(Header(this, node), TextBlock(node.GetString(N.Story.Text), selectable: true, treatAsHTML: true, textSize: TextSize.Tiny).WS().PL(56).BreakSpaces());
            
            var url = node.GetString(N.Story.Url);
            
            if (!string.IsNullOrEmpty(url))
            {
                card.WithExtraCommands(new[] { new CommandDefinition("Open Link", "o+l", LineAwesome.ExternalLinkAlt, () => ElectronBridge.OpenNewWindow(url, true)) });
            }

            return card;
        }

        public string GetColor(Node node) => Color;

        public string GetDisplayName(Node node) => DisplayName;

        public string GetIcon(Node node)
        {
            var url = node.GetString(N.Story.Url);

            if(!string.IsNullOrEmpty(url))
            {
                return "link";
            }
            else
            {
                return "notes";
            }
        }

        public string GetLabel(Node node) => node.GetString("Title");

        public async Task<CardContent> PreviewAsync(Node node, Parameters state)
        {
            return CardContent(Header(this, node), CreateView(node, state));
        }

        public async Task<IComponent> ViewAsync(Node node, Parameters state)
        {
            return (await PreviewAsync(node, state)).Merge();
        }

        private IComponent CreateView(Node node, Parameters state)
        {
            var author = Button().Foreground(Theme.Secondary.Foreground).NoPadding().NoMinSize();
            var text   = node.GetString(N.Story.Text);
            var url    = node.GetString(N.Story.Url);

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
                                                                      If(!string.IsNullOrEmpty(url), Link(url, Button("Open link").NoPadding().NoMinSize().SetIcon(LineAwesome.ExternalLinkAlt, afterText: true), noUnderline: true).OpenInNewTab())
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