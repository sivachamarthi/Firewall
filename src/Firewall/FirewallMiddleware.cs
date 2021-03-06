﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Firewall
{
    /// <summary>
    /// An ASP.NET Core middlware for request filtering.
    /// <para>Firewall can allow/deny access to a connection based on a range of different rules, such as IP address filtering, CIDR notation filtering or geo-location settings.</para>
    /// <para>Use the <see cref="FirewallRulesEngine"/> class to configure the Firewall rules.</para>
    /// </summary>
    public sealed class FirewallMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFirewallRule _ruleEngine;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new object of type <see cref="FirewallMiddleware"/>.
        /// </summary>
        public FirewallMiddleware(
            RequestDelegate next,
            IFirewallRule ruleEngine,
            ILogger<FirewallMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
            _logger = logger;
        }

        /// <summary>
        /// Filters an incoming HTTP request based on the configured rules engine.
        /// </summary>
        public Task Invoke (HttpContext context)
        {
            return
                _ruleEngine.IsAllowed(context)
                ? _next.Invoke(context)
                : DenyAccess(context);
        }

        private Task DenyAccess(HttpContext context)
        {
            if (_logger != null)
                _logger.LogWarning(
                    "Firewall: Unauthorized access from IP Address '{address}' trying to reach '{path}' has been blocked.",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path); //ToDo

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return context.Response.WriteAsync("You're not authorized to access this resource.");
        }
    }
}