using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.Extensions
{
    [ExcludeFromCodeCoverage]
    public class HttpResponseBody<T>
        where T : class
    {
        public T? Value { get; set; }

        public bool IsValid { get; set; }

        public IEnumerable<ValidationResult>? ValidationResults { get; set; }
    }
}
