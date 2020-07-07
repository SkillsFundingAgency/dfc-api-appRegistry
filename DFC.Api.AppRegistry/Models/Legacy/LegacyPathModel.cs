using DFC.Api.AppRegistry.Enums;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.Models.Legacy
{
    [ExcludeFromCodeCoverage]
    public class LegacyPathModel
    {
        public Guid DocumentId { get; set; }

        public string? Path { get; set; }

        public string? TopNavigationText { get; set; }

        public int TopNavigationOrder { get; set; }

        public Layout Layout { get; set; }

        public bool IsOnline { get; set; }

        public string? OfflineHtml { get; set; }

        public string? PhaseBannerHtml { get; set; }

        public Uri? SitemapURL { get; set; }

        public Uri? ExternalURL { get; set; }

        public Uri? RobotsURL { get; set; }

        public DateTime? DateOfRegistration { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}
