using System;
using System.Collections.Generic;
using System.Linq;

namespace DFC.Api.AppRegistry.Models.Pages
{
    public class PageModel
    {
        public Guid? Id { get; set; }

        public string? Location { get; set; }

        public Uri? Url { get; set; }

        public IList<string>? RedirectLocations { get; set; }

        public List<string>? AllLocations
        {
            get
            {
                var result = new List<string>();

                if (!string.IsNullOrWhiteSpace(Location))
                {
                    result.Add(Location);
                }

                if (RedirectLocations != null && RedirectLocations.Any())
                {
                    result.AddRange(RedirectLocations);
                }

                return result;
            }
        }
    }
}
