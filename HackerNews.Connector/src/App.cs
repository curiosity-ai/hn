using CommandLine;
using Curiosity.Library;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using UID;
using Mosaik.Core;
using Python.Runtime;
using System.Threading.Channels;
using System.Runtime.CompilerServices;
using static HackerNews.Schema;
using static HackerNews.Schema.Nodes;
using System.Collections.Concurrent;

namespace HackerNews
{
    public static class App
    {
        private static dynamic _hub;
        private static dynamic _embed;

        public static async Task<int> Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("CURIOSITY_API_TOKEN");
            var url   = Environment.GetEnvironmentVariable("CURIOSITY_URL");

            if (string.IsNullOrEmpty(token))
            {
                PrintHelp();
                return 0xDEAD;
            }

            var pythonEnginePath = Environment.GetEnvironmentVariable("PYTHON_ENGINE_PATH");
            var modelPath        = Environment.GetEnvironmentVariable("MODEL_PATH");
            var embeddingsIndex  = Environment.GetEnvironmentVariable("EMBEDDINGS_INDEX");

            var loggerFactory = LoggerFactory.Create(log => log.AddConsole());
            var logger = loggerFactory.CreateLogger("HackerNews");

            bool isEmbeddingsEnabled = false;

            if (File.Exists(pythonEnginePath) && File.Exists(modelPath) && !string.IsNullOrEmpty(embeddingsIndex))
            {
                Runtime.PythonDLL = pythonEnginePath;
                PythonEngine.PythonPath = PythonEngine.PythonPath + ";" + Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                isEmbeddingsEnabled = InitializeModel(logger, modelPath);

                //TODO: Build a PCA model from pre-computed posts title vectors
                //PcaModel.Build(modelPath, precomputed);
            }

            using (var graph = Graph.Connect(url, token, "Hackernews Connector").WithLoggingFactory(loggerFactory))
            {
                try
                {
                    await graph.LogAsync("Starting Hackernews connector");

                    await CreateSchemaAsync(graph);

                    await UploadDataAsync(graph, isEmbeddingsEnabled, embeddingsIndex, logger);

                    await graph.LogAsync("Finished Hackernews connector");
                }
                catch(Exception E)
                {
                    await graph.LogErrorAsync(E.ToString());
                    throw;
                }
            }

            return 0;
        }

        private static async Task CreateSchemaAsync(Graph graph)
        {
            await graph.CreateNodeSchemaAsync<Job>();
            await graph.CreateNodeSchemaAsync<Comment>();
            await graph.CreateNodeSchemaAsync<Story>();
            await graph.CreateNodeSchemaAsync<Pool>();
            await graph.CreateNodeSchemaAsync<PoolOption>();
            await graph.CreateNodeSchemaAsync<Status>();
            await graph.CreateNodeSchemaAsync<Property>();
            await graph.CreateNodeSchemaAsync<User>();
            await graph.CreateNodeSchemaAsync<SubmissionType>();

            await graph.CreateEdgeSchemaAsync(Edges.HasCategory,      Edges.CategoryOf,
                                        Edges.HasPoolOption,    Edges.PoolOptionOf,
                                        Edges.StatusOf,         Edges.HasStatus,
                                        Edges.HasAuthor,        Edges.AuthorOf,
                                        Edges.HasComment,       Edges.CommentOf,
                                        Edges.HasFavorite,      Edges.FavoriteOf);
        }

        private static async Task UploadDataAsync(Graph graph, bool isEmbeddingsEnabled, string embeddingsIndex, ILogger logger)
        {
            int count = 0;
            var pendingEmbeddings = new ConcurrentDictionary<Node, string>();

            await foreach(var post in HackerNewsClient.FetchPostsAsync(1, limit: 100_000))
            {
                IngestPost(graph, post, pendingEmbeddings);
                count++;
                logger.LogInformation("Found {0} with id {1}: '{2}'", post.Type, post.Id, post.Title);

                if (count % 1000 == 0)
                {
                    await graph.CommitPendingAsync(); //Commit transactions every 1000 posts

                    var vectors = new List<NodeAndVector>();
                    foreach(var (postNode, title) in pendingEmbeddings)
                    {
                        vectors.Add(NodeAndVector.Create(postNode, GetVector(title)));
                    }

                    //TODO enable PCA:

                    //if (PcaModel.IsAvailable)
                    //{
                    //    _logger.LogInformation("Applying PCA transform to {0} vectors", vectors.Count);
                    //    PcaModel.Apply(vectors);
                    //    _logger.LogInformation("{0} vectors transformed using PCA", vectors.Count);
                    //}

                    await graph.AddEmbeddingsToIndexAsync(embeddingsIndex, vectors);
                }
            }
        }

        private static Node IngestPost(Graph graph, Post post, ConcurrentDictionary<Node, string> pendingEmbeddings, bool fetchKids = true)
        {
            Node postNode;

            switch (post.Type)
            {
                case PostType.job:       
                {
                    postNode = graph.TryAdd(new Job()
                    {
                        Id = post.Id.ToString(),
                        Text = post.Text,
                        Score = post.Score,
                        Title = post.Title,
                        Url = post.Url,
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds(post.Time)
                    });
                    break; 
                }
                case PostType.story:     
                {
                    postNode = graph.TryAdd(new Story()
                    {
                        Id = post.Id.ToString(),
                        Text = post.Text,
                        Score = post.Score,
                        Title = post.Title,
                        Url = post.Url,
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds(post.Time)
                    }); break; 
                }
                case PostType.comment:   
                {
                    postNode = graph.TryAdd(new Comment()
                    {
                        Id = post.Id.ToString(),
                        Text = post.Text,
                        Score = post.Score,
                        Title = post.Title,
                        Url = post.Url,
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds(post.Time)
                    });
                    break; 
                }
                case PostType.pool:      
                {
                    postNode = graph.TryAdd(new Pool()
                    {
                        Id = post.Id.ToString(),
                        Text = post.Text,
                        Score = post.Score,
                        Title = post.Title,
                        Url = post.Url,
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds(post.Time)
                    });
                    break; 
                }
                case PostType.poolopt:   
                {
                    postNode = graph.TryAdd(new PoolOption()
                    {
                        Id = post.Id.ToString(),
                        Text = post.Text,
                        Score = post.Score,
                        Title = post.Title,
                        Url = post.Url,
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds(post.Time)
                    });
                    break; 
                }
                default: throw new NotSupportedException($"Unknown post type: {post.Type}");
            }

            if (post.HasTitle && post.Title.Length > 5)
            {
                pendingEmbeddings[postNode] = post.Title;
            }


            string postType = null, status = null;

            if (!string.IsNullOrEmpty(post.Title))
            {
                if (post.Title.StartsWith("Show HN:"))
                {
                    postType = "Show";
                }
                else if (post.Title.StartsWith("Ask HN:"))
                {
                    if (post.IsHiring)
                    {
                        postType = "Hiring";
                    }
                    else
                    {
                        postType = "Ask";
                    }
                }
            }

            if (post.Deleted)
            {
                status = "Deleted";
            }
            else if (post.Dead)
            {
                status = "Dead";
            }
            else if (post.IsPlaceholder)
            {
                status = "Placeholder";
            }

            if (!string.IsNullOrEmpty(post.By))
            {
                var authorNode = graph.TryAdd(new User() { Name = post.By });
                graph.Link(postNode, authorNode, Edges.HasAuthor, Edges.AuthorOf);
            }

            if (!string.IsNullOrEmpty(postType))
            {
                var authorNode = graph.TryAdd(new SubmissionType() { Name = postType });
                graph.Link(postNode, authorNode, Edges.HasCategory, Edges.CategoryOf);
            }

            if (!string.IsNullOrEmpty(status))
            {
                var statusNode = graph.TryAdd(new Status() { Name = status });
                graph.Link(postNode, statusNode, Edges.HasStatus, Edges.StatusOf);
            }


            if (post.HasKids && fetchKids)
            {
                foreach (var kid in post.Children)
                {
                    var kidNode = IngestPost(graph, kid, pendingEmbeddings);

                    if (kid.Type == PostType.comment)
                    {
                        graph.Link(postNode, kidNode, Edges.HasComment, Edges.CommentOf);
                    }
                    else
                    {
                        graph.Link(postNode, kidNode, Edges.HasPoolOption, Edges.PoolOptionOf);
                    }
                }
            }

            return postNode;
        }

        private static bool InitializeModel(ILogger logger, string modelPath)
        {
            using (Py.GIL())
            {
                logger.LogInformation("Importing tensorflow_hub");
                _hub = Py.Import("tensorflow_hub");
                logger.LogInformation("Loading model");
                logger = _hub.load(modelPath);
                return true;
            }
        }

        private static float[] GetVector(string text)
        {
            using (Py.GIL())
            {
                var input = new List<string>(1) { text };
                var vector = _embed(input).numpy().flatten().tolist();
                return vector.As<float[]>();
            }
        }

        private static void PrintHelp() => Console.WriteLine("Missing API token, you can set it using the CURIOSITY_API_TOKEN environment variable.");
    }
}