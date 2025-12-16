// This file defines global using directives for commonly used namespaces across the AG layer,
// simplifying code files by eliminating repetitive using statements.
global using Microsoft.AspNetCore.Mvc;
global using System.Diagnostics;
global using System.Net;
global using System.Text.Json;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using ApiGateway.AG.Application.Common.Exceptions;
global using ApiGateway.AG.Extensions;
global using Microsoft.OpenApi.Models;
global using System.Collections.Concurrent;
global using Microsoft.OpenApi.Readers;
global using ApiGateway.AG.Infrastructure.Services;
global using Swashbuckle.AspNetCore.SwaggerGen;
global using ApiGateway.AG.Infrastructure.Swagger;
global using Yarp.ReverseProxy.Configuration;
global using Microsoft.Extensions.Primitives;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.OpenApi.Any;
global using DotNetCore.CAP;
global using EventBus.Abstractions;
global using Contracts.Events;
global using EventBus.Cap;