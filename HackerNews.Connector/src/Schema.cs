using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.Library;

namespace HackerNews
{
    // This class is an auto-generated helper for all existing node & edge schema names on your graph.
    // You can get an updated version of it by downloading the template project again.

    public static class Schema
    {
        public static class Nodes
        {

            [Node]
            public class Job
            {
                [Key] public string Id { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
                [Property] public string Title { get; set; }
                [Property] public string Text { get; set; }
                [Property] public string Url { get; set; }
                [Property] public int Score { get; set; }
            }

            [Node]
            public class Comment
            {
                [Key] public string Id { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
                [Property] public string Title { get; set; }
                [Property] public string Text { get; set; }
                [Property] public string Url { get; set; }
                [Property] public int Score { get; set; }
            }

            [Node]
            public class Story
            {
                [Key] public string Id { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
                [Property] public string Title { get; set; }
                [Property] public string Text { get; set; }
                [Property] public string Url { get; set; }
                [Property] public int Score { get; set; }
            }

            [Node]
            public class Pool
            {
                [Key] public string Id { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
                [Property] public string Title { get; set; }
                [Property] public string Text { get; set; }
                [Property] public string Url { get; set; }
                [Property] public int Score { get; set; }
            }

            [Node]
            public class PoolOption
            {
                [Key] public string Id { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
                [Property] public string Title { get; set; }
                [Property] public string Text { get; set; }
                [Property] public string Url { get; set; }
                [Property] public int Score { get; set; }
            }

            [Node]
            public class Status
            {
                [Key] public string Name { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
            }

            [Node]
            public class Property
            {
                [Key] public string Name { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
                [Property] public int Value { get; set; }
            }

            [Node]
            public class User
            {
                [Key] public string Name { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
                [Property] public int Karma { get; set; }
                [Property] public string About { get; set; }
            }

            [Node]
            public class SubmissionType
            {
                [Key] public string Name { get; set; }
                [Timestamp] public DateTimeOffset Timestamp { get; set; }
            }
        }


        public static class Edges
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
}
