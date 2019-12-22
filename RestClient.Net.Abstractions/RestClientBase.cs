﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestClientDotNet.Abstractions
{
    public class RestClientBase : IDisposable, IRestClient
    {
        #region Fields
        private bool disposed;
        #endregion

        #region Public Properties
        public IRestHeadersCollection DefaultRequestHeaders => ResponseProcessorFactory.DefaultRequestHeaders;
        public IResponseProcessorFactory ResponseProcessorFactory { get; }
        public bool ThrowExceptionOnFailure { get; set; } = true;
        public string DefaultContentType { get; set; } = "application/json";
        public Uri BaseUri => ResponseProcessorFactory.BaseAddress;
        public ISerializationAdapter SerializationAdapter { get; }
        public TimeSpan Timeout
        {
            get => ResponseProcessorFactory.Timeout;
            set => ResponseProcessorFactory.Timeout = value;
        }
        public ITracer Tracer { get; }
        #endregion

        #region Constructor
        public RestClientBase(ISerializationAdapter serializationAdapter, IResponseProcessorFactory responseProcessorFactory, ITracer tracer)
        {
            ResponseProcessorFactory = responseProcessorFactory;
            SerializationAdapter = serializationAdapter;
            Tracer = tracer;
        }
        #endregion

        #region Private Methods
        Task<RestResponse<TReturn>> IRestClient.SendAsync<TReturn, TBody>(Uri queryString, HttpVerb httpVerb, string contentType, TBody body, CancellationToken cancellationToken)
        {
            return Call<TReturn, TBody>(queryString, httpVerb, contentType, body, cancellationToken);
        }

        private async Task<RestResponse<TReturn>> Call<TReturn, TBody>(Uri queryString, HttpVerb httpVerb, string contentType, TBody body, CancellationToken cancellationToken)
        {
            var responseProcessor = await ResponseProcessorFactory.CreateResponseProcessorAsync
                (
                httpVerb,
                BaseUri,
                queryString,
                body,
                contentType,
                cancellationToken);

            if (responseProcessor.IsSuccess)
            {
                return await responseProcessor.ProcessRestResponseAsync<TReturn>(BaseUri, queryString, httpVerb);
            }

            var errorResponse = new RestResponse<TReturn>(
                default,
                responseProcessor.Headers,
                responseProcessor.StatusCode,
                responseProcessor,
                BaseUri,
                queryString,
                httpVerb
                );

            if (ThrowExceptionOnFailure)
            {
                throw new HttpStatusException(
                    $"{responseProcessor.StatusCode}.\r\nBase Uri: {BaseUri}. Querystring: {queryString}", errorResponse);
            }

            return errorResponse;
        }
        #endregion

        #region Public Methods


        #region Post
        public Task<RestResponse<TReturn>> PostAsync<TReturn, TBody>(TBody body)
        {
            return PostAsync<TReturn, TBody>(body, default(Uri));
        }

        public Task<RestResponse<TReturn>> PostAsync<TReturn, TBody>(TBody body, string queryString)
        {
            return PostAsync<TReturn, TBody>(body, new Uri(queryString, UriKind.Relative));
        }

        public Task<RestResponse<TReturn>> PostAsync<TReturn, TBody>(TBody body, Uri queryString)
        {
            return PostAsync<TReturn, TBody>(body, queryString, default);
        }

        public Task<RestResponse<TReturn>> PostAsync<TReturn, TBody>(TBody body, Uri queryString, CancellationToken cancellationToken)
        {
            return PostAsync<TReturn, TBody>(body, queryString, DefaultContentType, cancellationToken);
        }

        public Task<RestResponse<TReturn>> PostAsync<TReturn, TBody>(TBody body, Uri queryString, string contentType, CancellationToken cancellationToken)
        {
            return Call<TReturn, TBody>(queryString, HttpVerb.Post, contentType, body, cancellationToken);
        }
        #endregion

        #region Put
        public Task<RestResponse<TReturn>> PutAsync<TReturn, TBody>(TBody body, string queryString)
        {
            return PutAsync<TReturn, TBody>(body, new Uri(queryString, UriKind.Relative));
        }

        public Task<RestResponse<TReturn>> PutAsync<TReturn, TBody>(TBody body, Uri queryString)
        {
            return PutAsync<TReturn, TBody>(body, queryString, DefaultContentType, default);
        }

        public Task<RestResponse<TReturn>> PutAsync<TReturn, TBody>(TBody body, Uri queryString, CancellationToken cancellationToken)
        {
            return PutAsync<TReturn, TBody>(body, queryString, DefaultContentType, cancellationToken);
        }

        public Task<RestResponse<TReturn>> PutAsync<TReturn, TBody>(TBody body, Uri queryString, string contentType, CancellationToken cancellationToken)
        {
            return Call<TReturn, TBody>(queryString, HttpVerb.Put, contentType, body, cancellationToken);
        }
        #endregion

        #region Patch
        public Task<RestResponse<TReturn>> PatchAsync<TReturn, TBody>(TBody body, string queryString)
        {
            return PatchAsync<TReturn, TBody>(body, new Uri(queryString, UriKind.Relative));
        }

        public Task<RestResponse<TReturn>> PatchAsync<TReturn, TBody>(TBody body, Uri queryString)
        {
            return PatchAsync<TReturn, TBody>(body, queryString, DefaultContentType, default);
        }

        public Task<RestResponse<TReturn>> PatchAsync<TReturn, TBody>(TBody body, Uri queryString, CancellationToken cancellationToken)
        {
            return PatchAsync<TReturn, TBody>(body, queryString, DefaultContentType, cancellationToken);
        }

        public Task<RestResponse<TReturn>> PatchAsync<TReturn, TBody>(TBody body, Uri queryString, string contentType, CancellationToken cancellationToken)
        {
            return Call<TReturn, object>(queryString, HttpVerb.Patch, contentType, body, cancellationToken);
        }
        #endregion

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            GC.SuppressFinalize(this);

            ResponseProcessorFactory.Dispose();
        }

        #endregion
    }
}
