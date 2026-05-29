namespace DeskMatch.Domain.Base;

public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    public int Value { get; }
    public string Name { get; }

    protected Enumeration(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static IEnumerable<TEnum> GetAll()
    {
        var type = typeof(TEnum);
        var fields = type.GetFields(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.DeclaredOnly);

        return fields
            .Select(f => f.GetValue(null))
            .Cast<TEnum>();
    }

    public static TEnum FromValue(int value)
    {
        var item = GetAll().FirstOrDefault(e => e.Value == value);
        return item ?? throw new InvalidOperationException(
            $"No {typeof(TEnum).Name} with value {value} found.");
    }

    public static TEnum FromName(string name)
    {
        var item = GetAll().FirstOrDefault(e =>
            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
        return item ?? throw new InvalidOperationException(
            $"No {typeof(TEnum).Name} with name '{name}' found.");
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj) => obj is Enumeration<TEnum> other && Equals(other);

    public bool Equals(Enumeration<TEnum>? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
        => Equals(left, right);

    public static bool operator !=(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
        => !Equals(left, right);
}