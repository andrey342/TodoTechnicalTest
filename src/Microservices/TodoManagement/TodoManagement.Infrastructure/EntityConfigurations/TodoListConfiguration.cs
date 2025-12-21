namespace TodoManagement.Infrastructure.EntityConfigurations;

public class TodoListConfiguration : IEntityTypeConfiguration<TodoList>
{
    public void Configure(EntityTypeBuilder<TodoList> builder)
    {
        // Table Configuration

        builder.ToTable("TodoLists", "todo");

        // Primary Key Configuration

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // Property Configuration

        builder.Property(e => e.Name).HasMaxLength(200).IsUnicode(true).IsRequired();

        builder.Property(e => e.LastIssuedPublicId).IsRequired().HasDefaultValue(0);

        builder.Property(e => e.RowVersion).IsRequired().IsRowVersion().ValueGeneratedOnAddOrUpdate();

        // Index Configuration

        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_TodoList_Name")
            .IsUnique();

        // Owned Value Objects Configuration


        // Aggregate References Configuration (FK to other Aggregate Roots - no navigation)


        // Relationship Configuration

        builder.HasMany(e => e.Items)
            .WithOne()
            .HasForeignKey(e => e.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
