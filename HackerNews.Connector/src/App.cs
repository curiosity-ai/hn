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

namespace HackerNews
{
    public static class App
    {
        private static dynamic _hub;
        private static dynamic _embed;

        public static async Task<int> Main(string[] args)
        {
            string token = Environment.GetEnvironmentVariable("CURIOSITY_API_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                PrintHelp();
                return 0xDEAD;
            }


            //var loggerFactory = LoggerFactory.Create(log => ConfigureFilters(log));

            //_logger = loggerFactory.CreateLogger("Program");

            //if (!File.Exists(options.PythonPath)) throw new Exception($"Python DLL not found at {options.PythonPath}");

            //Runtime.PythonDLL = options.PythonPath;

            //PythonEngine.PythonPath = PythonEngine.PythonPath + ";" + Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);

            //PythonEngine.Initialize();

            //PythonEngine.BeginAllowThreads();

            //InitializeModel(options);

            //PcaModel.Build(modelPath, precomputed);
            //vector = GetVector(item.Title);

            //if (PcaModel.IsAvailable)
            //{
            //    _logger.LogInformation("Applying PCA transform to {0} vectors", vectors.Count);
            //    PcaModel.Apply(vectors);
            //    _logger.LogInformation("{0} vectors transformed using PCA", vectors.Count);
            //}

            //await graph.AddEmbeddingsToIndexAsync(options.Index, vectors);


            using (var graph = Graph.Connect("https://hn.curiosity.ai/", token, "Hackernews Connector"))
            {
                try
                {
                    await graph.LogAsync("Starting Hackernews connector");
                    
                    await UploadDataAsync(graph);

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

        private static void PrintHelp()
        {
            Console.WriteLine("Missing API token, you can set it using the CURIOSITY_API_TOKEN environment variable.");
        }

        private static async Task UploadDataAsync(Graph graph)
        {
            // Implement your data logic here.
        }


        private static void InitializeModel(ILogger logger, string modelPath)
        {
            using (Py.GIL())
            {
                logger.LogInformation("Importing tensorflow_hub");
                _hub = Py.Import("tensorflow_hub");
                logger.LogInformation("Loading model");
                logger = _hub.load(modelPath);
            }
        }

        private static float[] GetVector(string text)
        {
            using (Py.GIL())
            {
                var input = new List<string>() { text };
                var vector = _embed(input).numpy().flatten().tolist();
                return vector.As<float[]>();
            }
        }
    }
}