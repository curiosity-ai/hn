using Curiosity.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static HackerNews.Schema;
using static HackerNews.Schema.Nodes;

namespace HackerNews
{
    public static class App
    {
        public static async Task<int> Main(string[] args)
        {
            string token = Environment.GetEnvironmentVariable("CURIOSITY_API_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                PrintHelp();
                return 0xDEAD;
            }

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
    }
}