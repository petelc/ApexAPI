using MediatR;
using Ardalis.Result;
using Apex.API.Core.ValueObjects;

namespace Apex.API.UseCases.Tenants.Create;

/// <summary>
/// Command to create a new tenant (using MediatR)
/// </summary>
public record CreateTenantCommand(
    string CompanyName,
    string Subdomain,
    string AdminEmail,
    string AdminFirstName,
    string AdminLastName,
    string? Region = null
) : IRequest<Result<TenantId>>;
