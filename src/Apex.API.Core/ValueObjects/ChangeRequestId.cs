using Vogen;

namespace Apex.API.Core.ValueObjects;

/// <summary>
/// Strongly-typed identifier for ChangeRequest aggregate
/// </summary>
[ValueObject<Guid>]
public readonly partial struct ChangeRequestId
{
    private static Validation Validate(Guid value)
    {
        if (value == Guid.Empty)
            return Validation.Invalid("ChangeRequestId cannot be empty.");

        return Validation.Ok;
    }
}
