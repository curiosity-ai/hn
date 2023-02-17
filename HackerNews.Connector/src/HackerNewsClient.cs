using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HackerNews
{
    public class HackerNewsClient
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Converters = { new JsonStringEnumConverter() },
        };

        private static readonly HttpClient _client = new HttpClient();

        public static async IAsyncEnumerable<Post> FetchPostsAsync(int startingId, int limit = 0, int concurrency = -1, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (concurrency <= 0) concurrency = Environment.ProcessorCount;
            var tasks = new List<Task>();
            var posts = new ConcurrentQueue<Post>();

            var lastPost = await GetLatestPostIdAsync();
            
            long count = 0;

            for (int id = startingId; id < lastPost; id++)
            {
                if (Interlocked.Read(ref count) > limit) break; //Stop after we fetched enough posts

                tasks.Add(Task.Run(async () =>
                {
                    var post = await GetByIdAsync(id, cancellationToken);

                    //Only enqueue parent posts, comments and pool options should be only fetched as children of other posts
                    if(post.Type != PostType.comment && post.Type != PostType.poolopt)
                    {
                        posts.Enqueue(post);
                        Interlocked.Increment(ref count);
                    }
                }));

                if (tasks.Count > concurrency)
                {
                    await Task.WhenAny(tasks);
                    tasks.RemoveAll(t => t.IsCompleted);
                    while (posts.TryDequeue(out var post)) yield return post;
                }
            }
            
            await Task.WhenAll(tasks);
         
            while (posts.TryDequeue(out var post)) yield return post;
        }

        private static async Task<Post> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            string link = $"https://hacker-news.firebaseio.com/v0/item/{id}.json";
            
            var response = await _client.GetAsync(link, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();

            try
            {
                var post = JsonSerializer.Deserialize<Post>(json, _jsonOptions);

                if (post is null)
                {
                    throw new Exception($"Failed to parse: {json}");
                }

                if (string.IsNullOrWhiteSpace(post.Url) && post.Url.StartsWith("https://news.ycombinator.com/item?id="))
                {
                    post.Url = ""; //remove any HackerNews urls like: "https://news.ycombinator.com/item?id={article.Id}";
                }

                if (post.HasKids)
                {
                    post.Children = new();

                    foreach(var  kid in post.Kids)
                    {
                        post.Children.Add(await GetByIdAsync(kid, cancellationToken));
                    }
                }

                return post;
            }
            catch (Exception E)
            {
                throw new Exception($"Failed to parse: {json}", E);
            }
        }

        public static async Task<int> GetLatestPostIdAsync(CancellationToken cancellationToken = default)
        {
            var link = "https://hacker-news.firebaseio.com/v0/topstories.json";
            var response = await _client.GetAsync(link, cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<int[]>(json, _jsonOptions);
            return ids.Max();
        }
    }

    public class Post
    {
        public int Id { get; set; }
        public bool Deleted { get; set; }
        public PostType Type { get; set; }
        public string By { get; set; }
        public long Time { get; set; }
        public bool Dead { get; set; }
        public int[] Kids { get; set; }
        public int Parent { get; set; }
        public int Descendants { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }

        
        public List<Post> Children { get; set; }
        public bool HasKids => Kids is not null && Kids.Length > 0;
        public bool HasTitle => !string.IsNullOrEmpty(Title);
        public bool IsHiring => HasTitle  && Title.StartsWith("Ask HN: Who is hiring", StringComparison.InvariantCultureIgnoreCase);
        public bool IsShow => HasTitle && Title.StartsWith("Show HN:", StringComparison.InvariantCultureIgnoreCase);
        public bool IsAsk  => HasTitle && Title.StartsWith("Ask HN:", StringComparison.InvariantCultureIgnoreCase);
        public bool IsPlaceholder => HasTitle && Title.Equals("Placeholder", StringComparison.InvariantCultureIgnoreCase);
    }

    public enum PostType
    {
        job,
        story,
        comment,
        pool,
        poolopt
    }
}
