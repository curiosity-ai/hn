namespace HackerNews
{
    // This class is an auto-generated helper for all existing node & edge schema on your graph.
    // You can get an updated version of it by downloading the template project again, and replacing this file
    //
    // You can use as a helper to call methods on the graph, or read contents nodes you retrieve.
    //
    // Examples:
    //
    // - Fetch node from graph
    //   >    var authorNode = await Mosaik.API.Nodes.GetAsync(N.Author.Type, "John Doe");
    //
    // - Read string value from node:
    //   >    var authorName = authorNode .GetString(N.Author.Name);
    //
    // - Run query on graph:
    //   >    var booksFromAuthor = await Mosaik.API.Query.StartAt(N.Author.Type, "John Doe").Out(N.Book.Type, E.AuthorOf).Take(100).GetAsync();
    //     or
    //   >    var booksFromAuthor = await Mosaik.API.Query.StartAt(authorNode.UID).Out(N.Book.Type, E.AuthorOf).Take(100).GetAsync();

    public static class N
    {
        public sealed class Job
        {
            public const string Type = nameof(Job);
            public const string Id = nameof(Id);
            public const string Title = nameof(Title);
            public const string Text = nameof(Text);
            public const string Url = nameof(Url);
            public const string Score = nameof(Score);
        }
        public sealed class Comment
        {
            public const string Type = nameof(Comment);
            public const string Id = nameof(Id);
            public const string Title = nameof(Title);
            public const string Text = nameof(Text);
            public const string Url = nameof(Url);
            public const string Score = nameof(Score);
        }
        public sealed class Story
        {
            public const string Type = nameof(Story);
            public const string Id = nameof(Id);
            public const string Title = nameof(Title);
            public const string Text = nameof(Text);
            public const string Url = nameof(Url);
            public const string Score = nameof(Score);
        }
        public sealed class Pool
        {
            public const string Type = nameof(Pool);
            public const string Id = nameof(Id);
            public const string Title = nameof(Title);
            public const string Text = nameof(Text);
            public const string Url = nameof(Url);
            public const string Score = nameof(Score);
        }
        public sealed class PoolOption
        {
            public const string Type = nameof(PoolOption);
            public const string Id = nameof(Id);
            public const string Title = nameof(Title);
            public const string Text = nameof(Text);
            public const string Url = nameof(Url);
            public const string Score = nameof(Score);
        }
        public sealed class Status
        {
            public const string Type = nameof(Status);
            public const string Name = nameof(Name);
        }
        public sealed class Property
        {
            public const string Type = nameof(Property);
            public const string Name = nameof(Name);
            public const string Value = nameof(Value);
        }
        public sealed class User
        {
            public const string Type = nameof(User);
            public const string Name = nameof(Name);
            public const string Karma = nameof(Karma);
            public const string About = nameof(About);
        }
        public sealed class SubmissionType
        {
            public const string Type = nameof(SubmissionType);
            public const string Name = nameof(Name);
        }
    }


    public static class E
    {
        public const string HasCategory = nameof(HasCategory);
        public const string CategoryOf = nameof(CategoryOf);
        public const string HasPoolOption = nameof(HasPoolOption);
        public const string PoolOptionOf = nameof(PoolOptionOf);
        public const string StatusOf = nameof(StatusOf);
        public const string HasStatus = nameof(HasStatus);
        public const string HasAuthor = nameof(HasAuthor);
        public const string AuthorOf = nameof(AuthorOf);
        public const string HasComment = nameof(HasComment);
        public const string CommentOf = nameof(CommentOf);
        public const string HasFavorite = nameof(HasFavorite);
        public const string FavoriteOf = nameof(FavoriteOf);
    }

}
