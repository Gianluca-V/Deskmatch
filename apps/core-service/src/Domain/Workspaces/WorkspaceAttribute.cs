using DeskMatch.Domain.Base;

namespace DeskMatch.CoreService.Domain.Workspaces;

public sealed class WorkspaceAttribute : ValueObject
{
    public string Key { get; }
    public string? Value { get; }

    private WorkspaceAttribute(string key, string? value)
    {
        Key = key;
        Value = value;
    }

    public static WorkspaceAttribute Create(string rawKey, string? value)
    {
        var key = AttributeKeyNormalizer.Normalize(rawKey);
        return new WorkspaceAttribute(key, value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Key;
        yield return Value;
    }
}
