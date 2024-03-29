﻿using Sentry;
using Serilog;
using UncoreMetrics.API.Models.Responses.API;

namespace UncoreMetrics.API.Middleware;

public class JSONErrorMiddleware : IMiddleware
{
    // We want to return as many errors as we can in json ErrorResponse Format so any client can get a helpful error response it can read and not just a boring status code
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
            if (context.Response.StatusCode == 404 && context.Response.HasStarted == false)
            {
                var errorResponse = new ErrorResponse(context.Response.StatusCode,
                    $"Path not found: ({context.Request.Method}: {context.Request.Path})", "path_not_found");

                await context.Response.WriteAsJsonAsync(errorResponse);
            }

            if (context.Response.StatusCode == 405 && context.Response.HasStarted == false)
            {
                var errorResponse = new ErrorResponse(context.Response.StatusCode,
                    $"Method not allowed: ({context.Request.Method}: {context.Request.Path})", "method_not_allowed");

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
        catch (OperationCanceledException) { /* Nom If Client cancels requests... */ }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error");
            SentrySdk.CaptureException(ex);
            var errorResponse =
                new ErrorResponse(context.Response.StatusCode, "Internal Server Error", "internal_error");

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}