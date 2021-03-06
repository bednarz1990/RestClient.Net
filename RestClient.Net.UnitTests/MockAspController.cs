﻿using RestClient.Net.Abstractions;

namespace RestClient.Net.UnitTests
{
    public class MockAspController
    {
        public IClient Client { get; }

        public MockAspController(IClientFactory clientFactory)
        {
            Client = clientFactory.CreateClient("test");
        }
    }
}