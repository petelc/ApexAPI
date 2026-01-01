namespace Apex.API.UseCases.Departments.GetById;

public record DepartmentDto(
    Guid Id,
    string Name,
    string Description,
    Guid? DepartmentManagerUserId,
    string? DepartmentManagerName,
    bool IsActive,
    int MemberCount,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);