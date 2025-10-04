using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExamenParcial.Models;
namespace ExamenParcial.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
    base.OnModelCreating(builder);

    builder.Entity<Curso>()
        .HasIndex(c => c.Codigo)
        .IsUnique();

    builder.Entity<Matricula>()
        .HasIndex(m => new { m.CursoId, m.UsuarioId })
        .IsUnique();

    builder.Entity<Matricula>()
        .HasOne(m => m.Curso)
        .WithMany(c => c.Matriculas)
        .HasForeignKey(m => m.CursoId);
}
}
