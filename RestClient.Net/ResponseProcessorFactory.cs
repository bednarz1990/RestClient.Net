﻿using RestClientDotNet.Abstractions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RestClientDotNet
{
    public class ResponseProcessorFactory : IResponseProcessorFactory
    {
        public IZip Zip { get; }
        public ISerializationAdapter SerializationAdapter { get; }
        public ITracer Tracer { get; }
        public IHttpClientFactory HttpClientFactory { get; }
        public TimeSpan Timeout { get => HttpClientFactory.Timeout; set => HttpClientFactory.Timeout = value; }
        public Uri BaseAddress => HttpClientFactory.BaseUri;
        public IRestHeadersCollection DefaultRequestHeaders => HttpClientFactory.DefaultRequestHeaders;

        public ResponseProcessorFactory(
            ISerializationAdapter serializationAdapter,
            ITracer tracer,
            IHttpClientFactory httpClientFactory,
            IZip zip
            )
        {
            if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));
            SerializationAdapter = serializationAdapter;
            Zip = zip;
            HttpClientFactory = httpClientFactory;
            Tracer = tracer;
        }

        public async Task<IResponseProcessor> GetResponseProcessor<TBody>(HttpVerb httpVerb, Uri BaseUri, Uri queryString, TBody body, string contentType, CancellationToken cancellationToken)
        {
            var HttpClient = HttpClientFactory.Create();

            HttpResponseMessage httpResponseMessage = null;

            switch (httpVerb)
            {
                case HttpVerb.Get:
                    Tracer?.Trace(httpVerb, BaseUri, queryString, null, TraceType.Request, null, DefaultRequestHeaders);
                    httpResponseMessage = await HttpClient.GetAsync(queryString, cancellationToken);
                    break;
                case HttpVerb.Delete:
                    Tracer?.Trace(httpVerb, BaseUri, queryString, null, TraceType.Request, null, DefaultRequestHeaders);
                    httpResponseMessage = await HttpClient.DeleteAsync(queryString, cancellationToken);
                    break;

                case HttpVerb.Post:
                case HttpVerb.Put:
                case HttpVerb.Patch:

                    var bodyData = await SerializationAdapter.SerializeAsync(body);

                    using (var httpContent = new ByteArrayContent(bodyData))
                    {
                        //Why do we have to set the content type only in cases where there is a request body, and headers?
                        httpContent.Headers.Add("Content-Type", contentType);

                        Tracer?.Trace(httpVerb, BaseUri, queryString, bodyData, TraceType.Request, null, DefaultRequestHeaders);

                        if (httpVerb == HttpVerb.Put)
                        {
                            httpResponseMessage = await HttpClient.PutAsync(queryString, httpContent, cancellationToken);
                        }
                        else if (httpVerb == HttpVerb.Post)
                        {
                            httpResponseMessage = await HttpClient.PostAsync(queryString, httpContent, cancellationToken);
                        }
                        else
                        {
                            var method = new HttpMethod("PATCH");
                            using (var request = new HttpRequestMessage(method, queryString)
                            {
                                Content = httpContent
                            })
                            {
                                httpResponseMessage = await HttpClient.SendAsync(request, cancellationToken);
                            }
                        }
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }

            var responseProcessor = new ResponseProcessor
            (
                Zip,
                SerializationAdapter,
                httpResponseMessage,
                Tracer
            );

            return responseProcessor;
        }

        public void Dispose()
        {
        }
    }

}
