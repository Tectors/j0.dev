using System.Collections.Generic;
using Newtonsoft.Json;
using vj0.Core.Framework.CUEParse;

namespace vj0.Core.Models.API.Responses;

public class AesResponse
{
    [JsonProperty("mainKey")] public string MainKey = null!;
    [JsonProperty("dynamicKeys")] public List<EncryptionKey> DynamicKeys = null!;
}
