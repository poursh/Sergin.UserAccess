using Microsoft.EntityFrameworkCore;
using Sergin.UserAccess.Application;
using Sergin.SharedKernel.Infrastructure.Data.EFCore;
using Sergin.SharedKernel.Infrastructure.Data.EFCore.Outbox;

namespace Sergin.UserAccess.Infrastructure.Data;

public interface IUserAccessDbContext : IDbContext;

/// <summary>
/// Opts into the outbox (<see cref="IOutboxDbContext"/>): the <c>outbox_messages</c> and <c>inbox_messages</c>
/// tables live in this module's schema, added by the <c>AddOutbox</c> migration, so an integration event this
/// module raises rides on the same save as the aggregate that raised it, and a consumer here dedups through
/// its own inbox. No translator, event or handler is declared yet — the tables are ready for the first one.
/// </summary>
internal sealed class UserAccessDbContext(DbContextOptions<UserAccessDbContext> options)
    : SerginDbContext(options), IUserAccessDbContext, IUserAccessUnitOfWork, IOutboxDbContext
{
    public const string Schema = "ua";

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyOutbox();

        base.OnModelCreating(modelBuilder);
    }
}
