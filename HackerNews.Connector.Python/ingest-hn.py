from curiosity import Graph
from hackernews import HackerNews, Item, User
from datetime import datetime, timezone

def main():
    # Note: 
    #   You'll need to first create your schemas directly in the workspace,
    #   creating schemas using the Python API is not yet supported.

    server = 'http://localhost:8080'
    lib_token = 'eyJhbGc...'
    connector_name = 'HN Python data connector'

    hn = HackerNews()
    count = 100000
    story_count = 0
    with Graph.connect(server, lib_token, connector_name) as graph:
        max_id = hn.max_item()
        
        print(f'Latest story id = {max_id}')
        for x in range(1,max_id):
            post = hn.item(x)
            if post.type != 'story': pass
            inget_story(graph, post)
            story_count = story_count +1
            if story_count == count: break
            if story_count % 10 == 0: graph.commit_pending()

def inget_story(graph : Graph, post):
    title = post.title if hasattr(post, 'title') else ''
    if title:
        text  = post.text if hasattr(post, 'text') else ''
        url   = post.url if hasattr(post, 'url') else ''
        time  = post.time.isoformat() + 'Z'
        author = post.by if hasattr(post, 'by') else ''

        print(f'Found story {post.id}  from {time} -> {title}')

        status = ''
        post_type = ''

        if title.startswith('Show HN:'):
            postType = 'Show';
        elif title.startswith('Ask HN:'):
            if title.startswith('Ask HN: Who is hiring'):
                postType = 'Hiring';
            else:
                postType = 'Ask';

        if hasattr(post, 'deleted') and post.deleted:
            status = 'Deleted'
        elif hasattr(post,'dead') and post.dead:
            status = 'Dead'
        elif title == 'Placeholder':
            status = 'Placeholder'

        post_node = graph.add_or_update_by_key('Story', str(post.id), { 'Timestamp': time, 'Title': title , 'Text': text, 'Url': url, 'Score': post.score})

        if(author):
            author_node = graph.try_add_by_key('User', author, {})
            graph.link_bidirect(post_node, author_node, 'HasAuthor', 'AuthorOf')

        if(status):
            status_node = graph.try_add_by_key('Status', status, {})
            graph.link_bidirect(post_node, status_node, 'HasStatus', 'StatusOf')
        
        if(post_type):
            post_type_node = graph.try_add_by_key('SubmissionType', post_type, {})
            graph.link_bidirect(post_node, post_type_node, 'HasCategory', 'CategoryOf')


if __name__ == '__main__':
    main()