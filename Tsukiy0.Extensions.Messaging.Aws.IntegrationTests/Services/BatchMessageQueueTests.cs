using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tsukiy0.Extensions.Example.Core.Handlers;
using Tsukiy0.Extensions.Example.Core.Models;
using Tsukiy0.Extensions.Example.Infrastructure.Services;
using Tsukiy0.Extensions.Messaging.Aws.IntegrationTests.Helpers;
using Xunit;

namespace Tsukiy0.Extensions.Messaging.Aws.IntegrationTests.Services
{
    public class BatchMessageQueueTests
    {
        private readonly BatchSaveTestModelQueue _sut;
        private readonly DynamoTestModelRepository _repo;
        private readonly IHost _host;

        public BatchMessageQueueTests()
        {
            _host = TestHostBuilder.Build();
            _repo = _host.Services.GetRequiredService<DynamoTestModelRepository>();
            _sut = _host.Services.GetRequiredService<BatchSaveTestModelQueue>();
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        [Fact(Timeout = 90000)]
        public async void Send()
        {
            // Arrange
            var model = new TestModel(Guid.NewGuid(), Guid.NewGuid());

            // Act
            await _sut.Send(new List<SaveTestModelRequest>{
                new SaveTestModelRequest(model)
            });
            await Task.Delay(60000);
            var actual = await _repo.QueryByNamespace(model.Namespace);

            // Assert
            actual.Single().Should().BeEquivalentTo(model);
        }

    }

}
