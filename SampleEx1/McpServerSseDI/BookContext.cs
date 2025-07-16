using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace McpServerSseEF;

public class BookContext : DbContext
{
    public BookContext(DbContextOptions options) : base(options)
    {
    }

    protected BookContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookData>(e =>
        {
            e.ToTable(ShadowContent.TitleTable);
            e.Property<long>(ShadowContent.IdColumn);
            e.HasKey(ShadowContent.IdColumn);
        });
    }

    public DbSet<BookData> Books { get; set; } = null!;
}

public class BookData
{
    [Column(ShadowContent.IsbnColumn)]
    [Description("ISBNコード")]
    public string ISBN { get; set; } = string.Empty;

    [Column(ShadowContent.TitleColumn)]
    [Description("タイトル")]
    public string Title { get; set; } = string.Empty;

    [Column(ShadowContent.VolumneColumn)]
    [Description("巻数")]
    public int Volume { get; set; }
}
