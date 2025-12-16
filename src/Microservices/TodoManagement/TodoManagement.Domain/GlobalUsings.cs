// This file defines global using directives for the domain layer of the microservice.
// Global usings allow these namespaces to be available throughout the project without needing to specify them in each file.
// This helps reduce code duplication and improves maintainability.
global using Mediator;
global using System.Collections.Concurrent;
global using System.Linq.Expressions;
global using System.Reflection;
global using TodoManagement.Domain.SeedWork.Specifications;
global using TodoManagement.Domain.SeedWork;
global using TodoManagement.Domain.IRepositories.BaseIRepositories;
global using TodoManagement.Domain.AggregatesModel.TodoListAggregate;
