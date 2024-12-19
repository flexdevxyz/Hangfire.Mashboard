// This file is a modified version of a file that is part of Hangfire.
// Copyright © 2016 Hangfire OÜ.

// This modified file is subject to the terms of the GNU Lesser General Public License,
// version 3 or later. See the LICENSE file in the root of this project or visit
// <https://www.gnu.org/licenses/lgpl-3.0.html> for the full license text.

// Modifications:
// - Use middleware `JobStorageEveryInvokeAspNetCoreDashboardMiddleware` instead of `AspNetCoreDashboardMiddleware`.
// - It is up to said middleware to acquire a `JobStorage`.

using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Mashboard;

public static class HangfireMashboardExtensions
{
    /// <remarks>Modified version of https://github.com/HangfireIO/Hangfire/blob/v1.8.17/src/Hangfire.AspNetCore/HangfireApplicationBuilderExtensions.cs</remarks>
    public static IApplicationBuilder UseHangfireMashboard(
        this IApplicationBuilder app,
        string pathMatch,
        DashboardOptions options)
    {
        var services = app.ApplicationServices;
        options.TimeZoneResolver ??= services.GetService<ITimeZoneResolver>();
        var routes = app.ApplicationServices.GetRequiredService<RouteCollection>();

        app.Map(new PathString(pathMatch), x => x.UseMiddleware<JobStorageEveryInvokeAspNetCoreDashboardMiddleware>(options, routes));

        return app;
    }
}
