using Microsoft.AspNetCore.Mvc;

namespace ServiceBase.OData.Web.Configuration
{
    /// <summary>
    /// Helper class that lists the versions of the API
    /// </summary>
    internal static class ApiVersions
    {
        internal static readonly ApiVersion V1 = new ApiVersion(1, 0);
    }
}
