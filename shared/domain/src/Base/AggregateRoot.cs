namespace DeskMatch.Domain.Base;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
}