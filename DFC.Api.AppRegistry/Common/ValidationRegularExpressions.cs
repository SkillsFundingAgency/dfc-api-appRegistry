using System.Diagnostics.CodeAnalysis;

namespace DFC.Api.AppRegistry.Common
{
    [ExcludeFromCodeCoverage]
    public static class ValidationRegularExpressions
    {
        public const string Path = "^[A-Za-z0-9.,-_]*$";
    }
}
