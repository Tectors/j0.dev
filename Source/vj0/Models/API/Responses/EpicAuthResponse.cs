using System;

using Newtonsoft.Json;

namespace vj0.Models.API.Responses;

public class EpicAuthResponse
{
    [JsonProperty("access_token")] public string Token = null!;
    [JsonProperty("expires_at")] public DateTime Expires;
}
