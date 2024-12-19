// This file is a modified version of a file that is part of Hangfire.
// Copyright © 2016 Hangfire OÜ.

// This modified file is subject to the terms of the GNU Lesser General Public License,
// version 3 or later. See the LICENSE file in the root of this project or visit
// <https://www.gnu.org/licenses/lgpl-3.0.html> for the full license text.

// Modifications:
// - Acquiring a `JobStorage` by `httpContext.RequestServices.GetRequiredService<JobStorage>();`
//   instead of taking `JobStorage` as ctor param.

using System.Net;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Mashboard;

/// <remarks>Modified version of https://github.com/HangfireIO/Hangfire/blob/v1.8.17/src/Hangfire.AspNetCore/Dashboard/AspNetCoreDashboardMiddleware.cs</remarks>
public class JobStorageEveryInvokeAspNetCoreDashboardMiddleware(
    RequestDelegate next,
    DashboardOptions options,
    RouteCollection routes)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var findResult = routes.FindDispatcher(httpContext.Request.Path.Value);

        if (findResult == null)
        {
            await next.Invoke(httpContext);
            return;
        }

        var storage = httpContext.RequestServices.GetRequiredService<JobStorage>();
        var context = new AspNetCoreDashboardContext(storage, options, httpContext);

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var filter in options.Authorization)
        {
            if (!filter.Authorize(context))
            {
                SetResponseStatusCode(httpContext, GetUnauthorizedStatusCode(httpContext));
                return;
            }
        }

        foreach (var filter in options.AsyncAuthorization)
        {
            if (!await filter.AuthorizeAsync(context))
            {
                SetResponseStatusCode(httpContext, GetUnauthorizedStatusCode(httpContext));
                return;
            }
        }

        if (!options.IgnoreAntiforgeryToken)
        {
            var antiforgery = httpContext.RequestServices.GetService<IAntiforgery>();

            if (antiforgery != null)
            {
                var requestValid = await antiforgery.IsRequestValidAsync(httpContext);

                if (!requestValid)
                {
                    // Invalid or missing CSRF token
                    SetResponseStatusCode(httpContext, (int)HttpStatusCode.Forbidden);
                    return;
                }
            }
        }

        context.UriMatch = findResult.Item2;

        await findResult.Item1.Dispatch(context);
    }

    private static void SetResponseStatusCode(HttpContext httpContext, int statusCode)
    {
        if (!httpContext.Response.HasStarted)
        {
            httpContext.Response.StatusCode = statusCode;
        }
    }

    private static int GetUnauthorizedStatusCode(HttpContext httpContext)
    {
        // ReSharper disable once ConstantConditionalAccessQualifier
        return httpContext.User?.Identity?.IsAuthenticated == true
            ? (int)HttpStatusCode.Forbidden
            : (int)HttpStatusCode.Unauthorized;
    }
}
