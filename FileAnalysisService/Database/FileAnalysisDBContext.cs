using FileAnalysisService.Models;
using Microsoft.EntityFrameworkCore;

namespace FileAnalysisService.Database;

public class FileAnalysisDBContext : DbContext
{
	public FileAnalysisDBContext(DbContextOptions<FileAnalysisDBContext> options)
		: base(options)
	{
	}

	public DbSet<FileAnalysisResultModel> AnalysisResults { get; set; }
	public DbSet<PlagiarismModel> PlagiarismResults { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		modelBuilder.Entity<FileAnalysisResultModel>()
			.HasIndex(ar => ar.FileId);
	}
}