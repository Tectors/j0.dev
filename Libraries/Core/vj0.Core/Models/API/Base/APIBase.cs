using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using Serilog;

namespace vj0.Core.Models.API.Base;

public class APIBase
{
    private string BaseURL = string.Empty;

    private readonly RestClient _client;

    protected APIBase(RestClient client)
    {
        _client = client;
    }
    
    protected APIBase(RestClient client, string baseUrl)
    {
        _client = client;
        BaseURL = baseUrl;
    }

    protected async Task<T?> ExecuteAsync<T>(string url, Method method = Method.Get, bool verbose = true, bool useBaseUrl = true, params Parameter[] parameters)
    {
        try
        {
            var request = CreateRequest(url, method, parameters, useBaseUrl);

            var response = await _client.ExecuteAsync<T>(request).ConfigureAwait(false);
            LogResponse(request, response, verbose);

            return response.StatusCode == HttpStatusCode.OK ? response.Data : default;
        }
        catch (Exception e)
        {
            Log.Error("{Message}\n{StackTrace}", e.Message, e.StackTrace);
            return default;
        }
    }

    protected async Task<RestResponse> ExecuteAsync(string url, Method method = Method.Get, bool verbose = true, params Parameter[] parameters)
    {
        var request = CreateRequest(url, method, parameters);

        var response = await _client.ExecuteAsync(request).ConfigureAwait(false);
        LogResponse(request, response, verbose);

        return response;
    }

    private RestRequest CreateRequest(string url, Method method, Parameter[] parameters, bool useBaseUrl = true)
    {
        var fullUrl = useBaseUrl ? string.IsNullOrEmpty(BaseURL) ? url : $"{BaseURL}/{url}" : url;
        var request = new RestRequest(fullUrl, method);

        foreach (var parameter in parameters)
        {
            request.AddParameter(parameter);
        }

        return request;
    }

    private void LogResponse(RestRequest request, RestResponse response, bool verbose)
    {
        if (!verbose) return;

        Log.Information("[{Method}] {StatusDescription} ({StatusCode}): {Uri}", request.Method, response.StatusDescription, (int)response.StatusCode, request.Resource);

        if (response.ErrorException is not null)
        {
            Log.Error(response.ErrorException.ToString());
        }

        if (response.StatusCode != HttpStatusCode.OK && response.Content is not null)
        {
            Log.Error(response.Content);
        }
    }
}
