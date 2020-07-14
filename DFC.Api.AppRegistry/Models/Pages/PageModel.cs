using System;
using System.Collections.Generic;
using System.Text;

namespace DFC.Api.AppRegistry.Models.Pages
{
    public class PageModel
    {
        public string? CanonicalName { get; set; }

        public Uri? Url { get; set; }

        public IList<string>? RedirectLocations { get; set; }
    }
}
