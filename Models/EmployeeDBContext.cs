using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace CoreBotDBConnetion.Models
{
    public partial class EmployeeDBContext : DbContext
    {
        public EmployeeDBContext()
        {
        }

        public EmployeeDBContext(DbContextOptions<EmployeeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Employee> Employees { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json");
            var config = builder.Build();
            var connectionString = config.GetConnectionString("DBConnectionString");

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmpId)
                    .HasName("pk_emp_id");

                entity.ToTable("EMPLOYEE");

                entity.Property(e => e.EmpId)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("EMP_ID");

                entity.Property(e => e.EmpName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("EMP_NAME");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
