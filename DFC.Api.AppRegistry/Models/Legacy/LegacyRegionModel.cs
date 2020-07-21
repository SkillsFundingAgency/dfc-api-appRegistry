using DFC.Api.AppRegistry.Enums;
using Microsoft.Azure.Documents;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.Models.Legacy
{
    [ExcludeFromCodeCoverage]
    public class LegacyRegionModel : Document
    {
        public string? Path { get; set; }

        public PageRegion PageRegion { get; set; }

        public bool IsHealthy { get; set; }

        public string? RegionEndpoint { get; set; }

        public bool HealthCheckRequired { get; set; }

        public string? OfflineHTML { get; set; }

        public DateTime? DateOfRegistration { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}