using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data;

public partial class SqliteContext : DbContext
{
    public SqliteContext(DbContextOptions<SqliteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Discipline> Disciplines { get; set; }

    public virtual DbSet<DisciplineModule> DisciplineModules { get; set; }

    public virtual DbSet<Examination> Examinations { get; set; }

    public virtual DbSet<Institution> Institutions { get; set; }

    public virtual DbSet<InstitutionAssignment> InstitutionAssignments { get; set; }

    public virtual DbSet<Mark> Marks { get; set; }

    public virtual DbSet<PracticeType> PracticeTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Speciality> Specialities { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Discipline>(entity =>
        {
            entity.HasOne(d => d.Module).WithMany(p => p.Disciplines).HasForeignKey(d => d.ModuleId);

            entity.HasOne(d => d.PracticeType).WithMany(p => p.Disciplines)
                .HasForeignKey(d => d.PracticeTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SemesterNavigation).WithMany(p => p.Disciplines)
                .HasForeignKey(d => d.Semester)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DisciplineModule>(entity =>
        {
            entity.HasOne(d => d.Speciality).WithMany(p => p.DisciplineModules).HasForeignKey(d => d.SpecialityId);
        });

        modelBuilder.Entity<Examination>(entity =>
        {
            entity.ToTable("Examination");

            entity.HasOne(d => d.Discipline).WithMany(p => p.Examinations).HasForeignKey(d => d.DisciplineId);

            entity.HasOne(d => d.MarkNavigation).WithMany(p => p.Examinations).HasForeignKey(d => d.Mark);

            entity.HasOne(d => d.Student).WithMany(p => p.Examinations).HasForeignKey(d => d.StudentId);
        });

        modelBuilder.Entity<InstitutionAssignment>(entity =>
        {
            entity.ToTable("InstitutionAssignment");

            entity.HasOne(d => d.Institution).WithMany(p => p.InstitutionAssignments).HasForeignKey(d => d.InstitutionId);

            entity.HasOne(d => d.User).WithMany(p => p.InstitutionAssignments).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Mark>(entity =>
        {
            entity.HasKey(e => e.Mark1);

            entity.HasIndex(e => e.Mark1, "IX_Marks_Mark").IsUnique();

            entity.Property(e => e.Mark1)
                .ValueGeneratedNever()
                .HasColumnName("Mark");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Speciality>(entity =>
        {
            entity.HasOne(d => d.Institution).WithMany(p => p.Specialities).HasForeignKey(d => d.InstitutionId);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasOne(d => d.Speciality).WithMany(p => p.Students).HasForeignKey(d => d.SpecialityId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
