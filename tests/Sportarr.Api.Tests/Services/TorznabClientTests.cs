using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Sportarr.Api.Models;
using Sportarr.Api.Services;

namespace Sportarr.Api.Tests.Services;

public class TorznabClientTests
{
    [Fact]
    public async Task SearchAsync_ShouldOmitCategoryFilter_WhenNoCategoriesConfigured()
    {
        // Arrange
        var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var client = new TorznabClient(httpClient, Mock.Of<ILogger<TorznabClient>>());

        var indexer = new Indexer
        {
            Name = "torr9",
            Type = IndexerType.Torznab,
            Url = "https://example.test",
            ApiPath = "api",
            Categories = new List<string>()
        };

        // Act
        _ = await client.SearchAsync(indexer, "NBA 2026 05", 50);

        // Assert
        handler.LastRequestUri.Should().NotBeNull();
        handler.LastRequestUri!.Query.Should().NotContain("cat=");
    }

    [Fact]
    public async Task FetchRssFeedAsync_ShouldKeepDefaultCategoryFilter_WhenNoCategoriesConfigured()
    {
        // Arrange
        var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var client = new TorznabClient(httpClient, Mock.Of<ILogger<TorznabClient>>());

        var indexer = new Indexer
        {
            Name = "torr9",
            Type = IndexerType.Torznab,
            Url = "https://example.test",
            ApiPath = "api",
            Categories = new List<string>()
        };

        // Act
        _ = await client.FetchRssFeedAsync(indexer, 50);

        // Assert
        handler.LastRequestUri.Should().NotBeNull();
        var decodedQuery = Uri.UnescapeDataString(handler.LastRequestUri!.Query);
        decodedQuery.Should().Contain("cat=5000,5040,5045,5060");
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;

            const string xml = """
                <rss xmlns:torznab="http://torznab.com/schemas/2015/feed">
                  <channel>
                    <item>
                      <title>NBA.Test.Release</title>
                      <guid>test-guid</guid>
                      <link>https://example.test/download</link>
                    </item>
                  </channel>
                </rss>
                """;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(xml, Encoding.UTF8, "application/xml")
            });
        }
    }
}
