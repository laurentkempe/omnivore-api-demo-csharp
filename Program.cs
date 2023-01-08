using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

const string apiUrl = "https://api-prod.omnivore.app/api/graphql";
var settings = new JsonSerializerSettings
{
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    }
};
var serializer = JsonSerializer.Create(settings);

var searchTerm = Environment.GetEnvironmentVariable("SEARCH_TERM") ?? "";
var documentsDir = "documents";
Directory.CreateDirectory(documentsDir);

var downloaded = 0;
await foreach (var item in FetchAllLinksAsync(null, searchTerm))
{
    File.WriteAllText($"{documentsDir}/{item.Slug}.md", item.Content);
    if (++downloaded > 20) break;
}

async IAsyncEnumerable<Node> FetchAllLinksAsync(string? start, string searchQuery)
{
    var cursor = start;
    var hasNextPage = true;
    while (hasNextPage)
    {
        var nextPage = await FetchPageAsync(cursor, 10, searchQuery);
        if (nextPage == null) break;

        if (nextPage.Edges != null)
            foreach (var edge in nextPage.Edges)
                yield return edge.Node;
        cursor = nextPage.PageInfo?.EndCursor;
        hasNextPage = nextPage.PageInfo?.HasNextPage ?? false;
    }
}

async Task<Search?> FetchPageAsync(string? cursor, int limit, string searchQuery)
{
    var data = JsonConvert.SerializeObject(new
    {
        variables = new
        {
            after = cursor,
            first = limit,
            format = "markdown",
            includeContent = true,
            query = searchQuery
        },
        query = """
                query Search(
                  $after: String
                  $first: Int
                  $query: String
                  $includeContent: Boolean
                  $format: String
                ) {
                  search(
                    after: $after
                    first: $first
                    query: $query
                    includeContent: $includeContent
                    format: $format
                  ) {
                    ... on SearchSuccess {
                      edges {
                        node {
                          slug
                          content
                        }
                      }
                      pageInfo {
                        hasNextPage
                        endCursor
                        totalCount
                      }
                    }
                    ... on SearchError {
                      errorCodes
                    }
                  }
                }
                """
    });

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Cookie",
        $"auth={Environment.GetEnvironmentVariable("OMNIVORE_AUTH_TOKEN")};");
    var stringContent = new StringContent(data);
    stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
    {
        Content = stringContent
    };

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();
    await using var responseStream = await response.Content.ReadAsStreamAsync();
    await using var reader = new JsonTextReader(new StreamReader(responseStream));

    //get content as string from responseStream
    // var responseString = await response.Content.ReadAsStringAsync();

    var fetchPageAsync = serializer.Deserialize<RootObject>(reader);

    return fetchPageAsync?.Data.Search;
}

public record RootObject(Data Data);

public record Data(Search Search);

public record Search(Edges[]? Edges, PageInfo? PageInfo);

public record Edges(Node Node);

public record Node(string Slug, string Content);

public record PageInfo(bool HasNextPage, string? EndCursor, int TotalCount);