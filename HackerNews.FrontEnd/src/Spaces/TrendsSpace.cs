using H5.Core;
using Tesserae;
using static Tesserae.UI;
using static Mosaik.UI;
using Mosaik;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Mosaik.Helpers;
using PlotlyH5;
using System.Linq;
using Mosaik.Components;
using System.Text.RegularExpressions;
using static H5.Core.dom;

namespace HackerNews
{
    public class TrendsSpace : IComponent
    {
        private IComponent _content;

        public TrendsSpace(Parameters state)
        {
            _content = HubStack("Trends", Routes.Trends, DefaultRoutes.Home)
                            .Section(CreateTrendsArea(state), grow: true, customPadding: "0 8px 0 0");
        }

        private IComponent CreateTrendsArea(Parameters state)
        {
            var obsDict = new ObservableDictionary<int, string[]>
            {
                { 1, new[] { "Google" } },
                { 2, new[] { "Microsoft" } },
                { 3, new[] { "Apple" } }
            };

            if(state.TryGetValue("terms", out var terms))
            {
                obsDict.Clear();
                var parts = terms.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    obsDict[i + 1] = parts[i].Split(new[] { '+'}, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            var obsStrict   = new SettableObservable<bool>(true);
            var obsComments = new SettableObservable<bool>(false);

            return VStack().S().PL(14).PR(14).PB(24).ScrollY().Children(
                RenderWordsSelector(obsDict, obsStrict, obsComments).WS(),
                Defer(obsDict, obsStrict, obsComments, async (d,s,c) => (await RenderTrends(d,s,c)).S()).WS().H(10).Grow().MinHeight(80.vh()).NoShrink());
        }
        private IComponent RenderWordsSelector(ObservableDictionary<int, string[]> obsDict, SettableObservable<bool> obsStrict, SettableObservable<bool> obsComments)
        {
            if (obsDict.Count == 0)
            {
                obsDict.Add(1, new[] { "" });
                return Empty();
            }

            var toggleComments = Toggle("Comments");
            var toggleStrict   = Toggle("Exact Match");

            toggleComments.IsChecked = obsComments.Value;
            toggleStrict.IsChecked = obsStrict.Value;

            toggleComments.OnChange((_, __) => obsComments.Value = toggleComments.IsChecked);
            toggleStrict.OnChange((_, __) => obsStrict.Value = toggleStrict.IsChecked);

            return HStack().WS().Children(
                    Defer(obsDict, wordGroups => RenderWordGroups(obsDict)).W(10).Grow(),
                    VStack().PL(16).PR(8).Children(toggleComments, toggleStrict));
        }

        private async Task<IComponent> RenderWordGroups(ObservableDictionary<int, string[]> obsDict)
        {
            var stack = VStack().S().ScrollY().Class("trends-word-groups");

            foreach(var group in obsDict.OrderBy(kv => kv.Key))
            {
                var groupStack = HStack().WS().ScrollX().NoWrap().AlignItemsCenter().Class("trends-word-group").MB(4);

                var wordBoxes = new List<TextBox>();
                var groupId = group.Key;

                Action refreshGroup = () =>
                {
                    if(wordBoxes.Count == 0)
                    {
                        obsDict.Remove(groupId);
                    }
                    else
                    {
                        obsDict[groupId] = wordBoxes.Select(w => w.Text).ToArray();
                    }
                };

                groupStack.Add(Icon("fa fa-square", size: TextSize.Medium, color: GetColor(group.Key)).PL(8).PR(6));

                foreach (var word in group.Value)
                {
                    var tb = TextBox(word).UnlockHeight().H(32).Class("trends-word-box");
                    var btnRemove = Button().PL(4).PR(8).SetIcon(UIcons.Cross, color: Theme.Danger.Background).LessPadding().Tooltip("Remove word")
                                            .OnClick(() => { wordBoxes.Remove(tb); refreshGroup(); })
                                            .Class("trends-word-button");
                    wordBoxes.Add(tb);
                    tb.OnChange((_, __) => refreshGroup());
                    groupStack.Add(tb.W(100));
                    groupStack.Add(btnRemove);
                }

                var btnAdd = Button().PL(4).PR(8).SetIcon(UIcons.Plus, color: Theme.Primary.Background).LessPadding().Tooltip("Add a word")
                                        .OnClick(() => { obsDict[groupId] = obsDict[groupId].Append("").ToArray();  })
                                        .Class("trends-word-button");

                groupStack.Add(btnAdd);
             
                stack.Add(groupStack.WS());
            }

            var btnAddGroup = Button().SetIcon(UIcons.Plus).Primary().LessPadding().PT(8).OnClick(() => obsDict.Add(obsDict.Count > 0 ? (obsDict.Keys.Max() + 1) : 1, new[] { "" })).Class("trends-word-button");

            stack.Add(btnAddGroup);

            return stack;
        }

        private async Task<IComponent> RenderTrends(IReadOnlyDictionary<int, string[]> wordGroups, bool strict, bool comments)
        {
            await ExternalLibraries.LoadPlotlyAsync();

            Router.Replace(Routes.TrendsFor(wordGroups.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToArray()));

            var stack = VStack().S().JustifyContent(ItemJustify.Center).AlignItemsCenter().Class("trends-plot-area").P(16);

            var pm = ProgressModal().ProgressIndeterminated().Title("Calculating trends, please wait...");

            stack.Children(pm.ShowEmbedded());

            stack.WhenMounted(() =>
            {
                Task.Run(async () =>
                {

                    var response = await Mosaik.API.Endpoints.CallAsync<TrendsResponse>("trends", new TrendsRequest()
                    {
                        Words = wordGroups,
                        Strict = strict,
                        IncludeComments = comments
                    }, statusUpdatusReceived: s => pm.Message(s));

                    pm.Hide();

                    var ordered = response.Counts.ToDictionary(kv => kv.Key, kv => kv.Value.OrderBy(kv2 => kv2.Key).ToArray());

                    var categories = ordered.Values.SelectMany(kv => kv.Select(v => v.Key))
                                                    .Distinct()
                                                    .OrderBy(v => v)
                                                    .ToArray();

                    var size = stack.Render().getBoundingClientRect().As<DOMRect>();

                    stack.Children(Plotly(Plot.traces(ordered.Select((kv) => Traces.scatter(Scatter.x(kv.Value.Select(kv2 => kv2.Key)),
                                                                                Scatter.y(FilterIfNeeded(kv.Value.Select(kv2 => (float)kv2.Value).ToArray())),
                                                                                Scatter.mode(Scatter.Mode.lines()),
                                                                                //Scatter.fillcolor(GetColor(kv.Key)),
                                                                                Scatter.marker(Marker.color(GetColor(kv.Key))),
                                                                                Scatter.line(Line.color(GetColor(kv.Key)), Line.Shape.spline(), Line.smoothing(2f)),

                                                                                //Scatter.Fill.tozeroy(),
                                                                                //kv.Key == 1 ? Scatter.Fill.tozeroy() : Scatter.Fill.tonexty(),
                                                                                Scatter.name(string.Join(", ", wordGroups[kv.Key])))).ToArray()),
                                        Plot.layout(Layout.autosize(true),
                                            Layout.height((int)size.height),
                                            Layout.width((int)size.width),
                                            Layout.margin(Margin.l(0), Margin.t(0), Margin.r(0), Margin.b(64)),
                                            Layout.yaxis(Yaxis.automargin(true), Yaxis.Ticks.none()),
                                            Layout.showlegend(true),
                                            Layout.xaxis(
                                                Xaxis._type.category(),
                                                Xaxis.categoryarray(categories),
                                                Xaxis.showgrid(false),
                                                Xaxis.automargin(true),
                                                Xaxis.Autorange._true()),
                                            PlotlyConfig.Background(),
                                            PlotlyConfig.PaperBackground(),
                                            PlotlyConfig.Font()),
                                        PlotlyConfig.Default2D()));
                });
            });

            return stack;
        }

        private string GetColor(int i)
        {
            var colors = new[] { "#332288", "#88CCEE", "#44AA99", "#117733", "#999933", "#DDCC77", "#CC6677", "#882255", "#AA4499" };
            return colors[i % colors.Length];
        }

        private float[] FilterIfNeeded(float[] inputData)
        {
            if (inputData.Length < 10) return inputData;

            int n = 5;
            var movingAverages = new float[inputData.Length];
            var runningTotal = 0f;

            for (int i = 0; i < inputData.Length; ++i)
            {
                runningTotal += inputData[i];
                if (i - n >= 0)
                {
                    var lost = inputData[i - n];
                    runningTotal -= lost;
                    movingAverages[i] = runningTotal / n;
                }
            }

            return movingAverages;
        }

        public dom.HTMLElement Render() => _content.Render();

        class TrendsRequest
        {
            public IReadOnlyDictionary<int, string[]> Words { get; set; }
            public bool Strict { get; set; }
            public bool IncludeComments { get; set; }
        }
        class TrendsResponse
        {
            public Dictionary<int, Dictionary<string, int>> Counts { get; set; }
        }
    }

}