using Microsoft.EntityFrameworkCore;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Traxs.SharedKernel;

namespace Apex.API.Infrastructure.Data;

/// <summary>
/// Generic EF Core repository implementation
/// </summary>
public class EfRepository<T> : RepositoryBase<T>, IReadRepository<T>, IRepository<T> 
    where T : class, IAggregateRoot
{
    public EfRepository(ApexDbContext dbContext) : base(dbContext)
    {
    }
}
