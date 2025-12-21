namespace TodoManagement.Infrastructure.EntityConfigurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // Table Configuration

        builder.ToTable("TodoItems", "todo");

        // Primary Key Configuration

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // Property Configuration

        builder.Property(e => e.TodoListId).IsRequired();

        builder.Property(e => e.ItemId).IsRequired();

        builder.Property(e => e.Title).HasMaxLength(200).IsUnicode(true).IsRequired();

        builder.Property(e => e.Description).HasMaxLength(1000).IsUnicode(true).IsRequired();

        builder.Property(e => e.Category).HasMaxLength(100).IsUnicode(true).IsRequired();

        // Index Configuration

        builder.HasIndex(e => new { e.TodoListId, e.ItemId })
            .HasDatabaseName("IX_TodoItem_TodoList_ItemId")
            .IsUnique();
        builder.HasIndex(e => e.Category)
            .HasDatabaseName("IX_TodoItem_Category");

        // Owned Value Objects Configuration


        // Aggregate References Configuration (FK to other Aggregate Roots - no navigation)
        

        // Relationship Configuration

        builder.HasMany(e => e.Progressions)
            .WithOne()
            .HasForeignKey(e => e.TodoItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
