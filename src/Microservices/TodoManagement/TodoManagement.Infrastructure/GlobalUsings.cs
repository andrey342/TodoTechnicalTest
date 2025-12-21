// This file defines global using directives for commonly used namespaces across the infrastructure layer,
// simplifying code files by eliminating repetitive using statements.
global using Mediator;
global using TodoManagement.Domain.SeedWork;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;
global using System.ComponentModel.DataAnnotations;
global using System.Linq.Expressions;
global using TodoManagement.Domain.SeedWork.Specifications;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using TodoManagement.Infrastructure.Idempotency;
global using TodoManagement.Domain.IRepositories.BaseIRepositories;
global using TodoManagement.Infrastructure.Repositories.BaseRepositories;
global using TodoManagement.Domain.IRepositories;
global using TodoManagement.Domain.AggregatesModel.TodoListAggregate;
global using TodoManagement.Domain.AggregatesModel.TodoListAggregate.Masters;
