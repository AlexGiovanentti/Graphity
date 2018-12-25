﻿using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Graphity.Tests.Fixtures;
using Graphity.Tests.Fixtures.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Graphity.Tests.Middleware
{
    [Collection("Middleware Tests")]
    public class BasicMiddlewareTests : IClassFixture<CustomWebApplicationFactory<BasicStartup>>
    {
        private readonly WebApplicationFactory<BasicStartup> _factory;

        public BasicMiddlewareTests(CustomWebApplicationFactory<BasicStartup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder => builder.UseStartup<BasicStartup>());
        }

        [Fact]
        public async Task Can_retrieve_top_level_entities()
        {
            var client = _factory.CreateClient();

            var query = new GraphQLQuery
            {
                Query = @"{animals {id name}}"
            };

            var response = await client.PostAsJsonAsync("/api/graph", query);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<GraphExecutionResult<Animal>>();

            Assert.Equal(4, result.Data["animals"].Count());

            var item = result.Data["animals"].First();
            Assert.Equal(1, item.Id);
            Assert.Equal("Dog", item.Name);
            Assert.Null(item.LivesIn);
        }

        [Fact]
        public async Task Can_retrieve_second_level_entities()
        {
            var client = _factory.CreateClient();

            var query = new GraphQLQuery
            {
                Query = @"{animals {id name livesIn { name }}}"
            };

            var response = await client.PostAsJsonAsync("/api/graph", query);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<GraphExecutionResult<Animal>>();

            Assert.Equal(4, result.Data["animals"].Count());

            var item = result.Data["animals"].First();
            Assert.Equal(1, item.Id);
            Assert.Equal("Dog", item.Name);
            Assert.Equal("England", item.LivesIn.Name);
        }

        [Fact]
        public async Task Can_use_where_expression()
        {
            var client = _factory.CreateClient();

            var query = new GraphQLQuery
            {
                Query = @"{animals(where: [{path: ""name"", comparison: equal, value: ""Snake""}]) {id name}}"
            };

            var response = await client.PostAsJsonAsync("/api/graph", query);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<GraphExecutionResult<Animal>>();

            Assert.Single(result.Data["animals"]);

            var item = result.Data["animals"].First();
            Assert.Equal(4, item.Id);
            Assert.Equal("Snake", item.Name);
            Assert.Null(item.LivesIn);
        }

        [Fact]
        public async Task Can_use_multiple_where_expressions()
        {
            var client = _factory.CreateClient();

            var query = new GraphQLQuery
            {
                Query = @"{animals(where: [{path: ""id"", comparison: greaterThan, value: ""2""}, {path: ""id"", comparison: lessThan, value: ""4""}]) {id name}}"
            };

            var response = await client.PostAsJsonAsync("/api/graph", query);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<GraphExecutionResult<Animal>>();

            Assert.Single(result.Data["animals"]);

            var item = result.Data["animals"].First();
            Assert.Equal(3, item.Id);
            Assert.Equal("Sloth", item.Name);
            Assert.Null(item.LivesIn);
        }
    }
}