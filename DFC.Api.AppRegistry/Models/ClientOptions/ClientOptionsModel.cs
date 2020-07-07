using System;

namespace DFC.Api.AppRegistry.Models.ClientOptions
{
    public abstract class ClientOptionsModel
    {
        public Uri? BaseAddress { get; set; }

        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 20);         // default to 20 seconds

        public string? ApiKey { get; set; }
    }
}
