using DFC.Api.AppRegistry.Enums;
using System;

namespace DFC.Api.AppRegistry.Models.Legacy
{
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

        public string? SitemapURL { get; set; }

        public string? ExternalURL { get; set; }

        public string? RobotsURL { get; set; }

        public DateTime? DateOfRegistration { get; set; }

        public DateTime? LastModifiedDate { get; set; }
    }
}
