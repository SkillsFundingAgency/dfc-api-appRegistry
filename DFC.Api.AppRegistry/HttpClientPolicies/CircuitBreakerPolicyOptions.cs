using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.HttpClientPolicies
{
    [ExcludeFromCodeCoverage]
    public class CircuitBreakerPolicyOptions
    {
        public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

        public int ExceptionsAllowedBeforeBreaking { get; set; } = 12;
    }
}
