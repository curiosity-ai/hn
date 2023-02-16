var client = Microsoft.Extensions.Http.DefaultHttpClientFactory.Instance.CreateClient("HN");
var token = Graph.Unsafe.ShutdownToken;
var jsonOptions = new System.Text.Json.JsonSerializerOptions
{
    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
    NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
};

var latestID = await GetLatestId();
var fromId = 1;

if(Graph.TryGet(N.Property.Type, "LatestIngestedId", out var previousLatestNode))
{
    fromId = previousLatestNode.GetInt(N.Property.Value);
}

var tasks = new List<Task>();

for(int id = 1; id < latestID; id++)
{
    tasks.Add(ProcessId(id));

    if (tasks.Count > 64)
    {
        await Task.WhenAny(tasks);
        tasks.RemoveAll(t => t.IsCompleted);
    }
}

await Task.WhenAll(tasks);

async Task ProcessId(int id)
{
    await FetchPost(id);
    
    if (id % 100 == 0)
    {
        var latestNode = await Graph.GetOrAddLockedAsync(N.Property.Type, "LatestIngestedId");
        if (latestNode.GetInt(N.Property.Value) < id)
        {
            latestNode.SetInt(N.Property.Value, id);
        }
        await Graph.CommitAsync(latestNode);
    }
}

async Task<UID128> FetchPost(int id, bool readingComments = false)
{
    var sid = id.ToString();

    var postUID    = Node.GetUID(N.Story.Type, sid);
    if (Graph.HasNode(postUID)) return postUID;

    var commentUID = Node.GetUID(N.Comment.Type, sid);
    if (Graph.HasNode(commentUID)) return commentUID;

    var jobUID     = Node.GetUID(N.Job.Type, sid);
    if (Graph.HasNode(jobUID)) return jobUID;

    var poolUID    = Node.GetUID(N.Pool.Type, sid);
    if (Graph.HasNode(poolUID)) return poolUID;

    var poolOptUID = Node.GetUID(N.PoolOption.Type, sid);
    if (Graph.HasNode(poolOptUID)) return poolOptUID;

    var post = await GetById(id);

    string type;

    switch(post.Type)
    {
        case PostType.job:      type = N.Job.Type; break;
        case PostType.story:    type = N.Story.Type; break;
        case PostType.comment:  type = N.Comment.Type; break;
        case PostType.pool:     type = N.Pool.Type; break;
        case PostType.poolopt:  type = N.PoolOption.Type; break;
        default: return default(UID128);
    }

    if ((post.Type == PostType.comment || post.Type == PostType.poolopt) && !readingComments)
    {
        return default(UID128);
    }

    string postType = null, status = null;

    if(!string.IsNullOrEmpty(post.Title))
    {
        if (post.Title.StartsWith("Show HN:"))
        {
            postType = "Show";
        }
        else if (post.Title.StartsWith("Ask HN:"))
        {
            if(post.IsHiring)
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

    if (post.Dead)
    {
        status = "Dead";
    }

    var postNode = await Graph.GetOrAddLockedAsync(type, sid);
    postNode.SetString(N.Story.Title, post.Title);
    postNode.SetString(N.Story.Text, post.Text);
    postNode.SetString(N.Story.Url, post.Url);
    postNode.SetInt(N.Story.Score, post.Score);
    postNode.Timestamp = new Time(DateTimeOffset.FromUnixTimeSeconds(post.Time));

    if(!string.IsNullOrEmpty(post.By))
    {
        var authorNode = await Graph.GetOrAddLockedAsync(N.User.Type, post.By);
        postNode.AddUniqueEdge(E.HasAuthor, authorNode);
        authorNode.AddUniqueEdge(E.AuthorOf, postNode);
        await Graph.CommitAsync(authorNode);
    }

    if(!string.IsNullOrEmpty(postType))
    {
        var typeNode = await Graph.GetOrAddLockedAsync(N.SubmissionType.Type, postType);
        typeNode.AddUniqueEdge(E.CategoryOf, postNode);
        postNode.AddUniqueEdge(E.HasCategory, typeNode);
        await Graph.CommitAsync(typeNode);
    }

    if(!string.IsNullOrEmpty(status))
    {
        var statusNode = await Graph.GetOrAddLockedAsync(N.Status.Type, status);
        statusNode.AddUniqueEdge(E.StatusOf, postNode);
        postNode.AddUniqueEdge(E.HasStatus, statusNode);
        await Graph.CommitAsync(statusNode);
    }

    if (post.HasKids)
    {
        foreach(var kid in post.Kids)
        {
            var kidUID = await FetchPost(kid, readingComments: true);

            if (kidUID.IsNull()) continue;

            var kidNode = await Graph.TryGetLockedAsync(kidUID);
            if (kidNode is null) continue;

            if(kidNode.TypeName == N.PoolOption.Type)
            {
                kidNode.AddUniqueEdge(E.PoolOptionOf, postNode);
                postNode.AddUniqueEdge(E.HasPoolOption, kidNode);
            }
            else
            {
                kidNode.AddUniqueEdge(E.CommentOf, postNode);
                postNode.AddUniqueEdge(E.HasComment, kidNode);
            }

            await Graph.CommitAsync(kidNode);
        }
    }

    await Graph.CommitAsync(postNode);

    if (post.Type == PostType.story)
    {
        Logger.LogInformation("Fetched story {0}", post.Id);
    }

    return postNode.UID;
}


async Task<int> GetLatestId()
{
    var link = "https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty";
    var response = await client.GetAsync(link, token);
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    var ids = System.Text.Json.JsonSerializer.Deserialize<int[]>(json, jsonOptions);
    return ids.Max();
}

async Task<Post> GetById(int id)
{
    string link = $"https://hacker-news.firebaseio.com/v0/item/{id}.json?print=pretty";
    var response = await client.GetAsync(link, token);
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();

    try
    {
        var article = System.Text.Json.JsonSerializer.Deserialize<Post>(json, jsonOptions);

        if(article is null)
        {
            throw new Exception($"Failed to parse: {json}");
        }

        if (string.IsNullOrWhiteSpace(article.Url) && article.Url.StartsWith("https://news.ycombinator.com/item?id="))
        {
            article.Url = ""; //remove any HackerNews urls like: "https://news.ycombinator.com/item?id={article.Id}";
        }

        return article;
    }
    catch (Exception E)
    {
        throw new Exception($"Failed to parse: {json}", E);
    }
}
// { "by" : "pg", "descendants" : 15, "id" : 1, "kids" : [ 15, 234509, 487171, 82729 ], "score" : 57, "time" : 1160418111, "title" : "Y Combinator", "type" : "story", "url" : "http://ycombinator.com" }

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
    public bool HasKids => Kids != null;
    public bool IsHiring => Title.Contains("Ask HN: Who is hiring");
}

public enum PostType
{
    job,
    story,
    comment,
    pool,
    poolopt
}