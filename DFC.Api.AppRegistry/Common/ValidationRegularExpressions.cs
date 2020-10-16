using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.Common
{
    [ExcludeFromCodeCoverage]
    public static class ValidationRegularExpressions
    {
        public const string Path = @"^[a-zA-Z0-9](\w|[.,\/\-])*[a-zA-Z0-9]$";
    }
}
