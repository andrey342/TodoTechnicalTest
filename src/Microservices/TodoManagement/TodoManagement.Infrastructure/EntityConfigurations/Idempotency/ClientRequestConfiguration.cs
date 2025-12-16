namespace TodoManagement.Infrastructure.EntityConfigurations.Idempotency;

/// <summary>
/// Entity configuration for the ClientRequest entity, used for idempotency tracking.
/// </summary>
class ClientRequestConfiguration
    : IEntityTypeConfiguration<ClientRequest>
{
    /// <summary>
    /// Configures the ClientRequest entity properties and constraints.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<ClientRequest> builder)
    {
        // Table Configuration
        builder.ToTable("ClientRequest", "idempotency");

        // Configure primary key
        builder.HasKey(cr => cr.Id);

        // The GUID is provided by the client, so value generation is disabled
        builder.Property(cr => cr.Id)
               .ValueGeneratedNever();

        // Name is required and has a reasonable maximum length
        builder.Property(cr => cr.Name)
               .IsRequired()
               .HasMaxLength(200);

        // Unique index on Id for efficient lookups
        builder.HasIndex(cr => cr.Id)
               .IsUnique();
    }
}