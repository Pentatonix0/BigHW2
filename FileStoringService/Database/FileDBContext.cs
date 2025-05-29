using Microsoft.EntityFrameworkCore;
using FileStoringService.Models;

namespace FileStoringService.Database
{
	public class FileDBContext : DbContext
	{
		public FileDBContext(DbContextOptions<FileDBContext> options)
		   : base(options)
		{ }

		public DbSet<FileModel> Files { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<FileModel>()
				.HasIndex(sf => sf.Hash)
				.IsUnique();
		}
	}
}