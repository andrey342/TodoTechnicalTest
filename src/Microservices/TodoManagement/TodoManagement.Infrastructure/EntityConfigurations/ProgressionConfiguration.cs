namespace TodoManagement.Infrastructure.EntityConfigurations;

public class ProgressionConfiguration : IEntityTypeConfiguration<Progression>
{
    public void Configure(EntityTypeBuilder<Progression> builder)
    {
        // Table Configuration

        builder.ToTable("TodoItemProgressions", "todo");

        // Primary Key Configuration

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // Property Configuration

        builder.Property(e => e.TodoItemId).IsRequired();

        builder.Property(e => e.ActionDate).IsRequired();

        builder.Property(e => e.Percent).HasPrecision(5, 2).IsRequired();

        // Index Configuration

        builder.HasIndex(e => new { e.TodoItemId, e.ActionDate })
            .HasDatabaseName("IX_Progression_Item_Date").IsUnique();

        // Owned Value Objects Configuration


        // Aggregate References Configuration (FK to other Aggregate Roots - no navigation)
        

        // Relationship Configuration
    }
}
