using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Metrics
{
    public abstract class MetricMiddleware
    {
        private readonly Regex[] ignorePatterns;

        protected MetricMiddleware(Regex[] ignorePatterns)
        {
            this.ignorePatterns = ignorePatterns;
        }

        protected bool PerformMetric(HttpContext context)
        {
            if (ignorePatterns == null)
            {
                return true;
            }

            var requestPath = context.Request.Path.Value;

            if (string.IsNullOrWhiteSpace(requestPath)) return false;

            return !this.ignorePatterns.Any(ignorePattern => ignorePattern.IsMatch(requestPath.TrimStart('/')));
        }
    }
}