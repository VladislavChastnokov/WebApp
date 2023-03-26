using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data;

public partial class DContext : DbContext
{
    public DContext(DbContextOptions<DContext> options)
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
            entity.Property(e => e.DisciplineName).HasMaxLength(500);

            entity.HasOne(d => d.Module).WithMany(p => p.Disciplines)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK_Disciplines_DisciplineModules");

            entity.HasOne(d => d.PracticeType).WithMany(p => p.Disciplines)
                .HasForeignKey(d => d.PracticeTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Disciplines_PracticeTypes");

            entity.HasOne(d => d.SemesterNavigation).WithMany(p => p.Disciplines)
                .HasForeignKey(d => d.Semester)
                .HasConstraintName("FK_Disciplines_Semesters");
        });

        modelBuilder.Entity<DisciplineModule>(entity =>
        {
            entity.Property(e => e.ModuleCode).HasMaxLength(10);
            entity.Property(e => e.ModuleName).HasMaxLength(150);

            entity.HasOne(d => d.Speciality).WithMany(p => p.DisciplineModules)
                .HasForeignKey(d => d.SpecialityId)
                .HasConstraintName("FK_DisciplineModules_Specialities");
        });

        modelBuilder.Entity<Examination>(entity =>
        {
            entity.ToTable("Examination");

            entity.HasOne(d => d.Discipline).WithMany(p => p.Examinations)
                .HasForeignKey(d => d.DisciplineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Examination_Disciplines");

            entity.HasOne(d => d.MarkNavigation).WithMany(p => p.Examinations)
                .HasForeignKey(d => d.Mark)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Examination_Marks");

            entity.HasOne(d => d.Student).WithMany(p => p.Examinations)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_Examination_Students");
        });

        modelBuilder.Entity<Institution>(entity =>
        {
            entity.Property(e => e.InstitutionName).HasMaxLength(1000);
        });

        modelBuilder.Entity<InstitutionAssignment>(entity =>
        {
            entity.HasOne(d => d.Institution).WithMany(p => p.InstitutionAssignments)
                .HasForeignKey(d => d.InstitutionId)
                .HasConstraintName("FK_InstitutionAssignments_Institutions");

            entity.HasOne(d => d.User).WithMany(p => p.InstitutionAssignments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_InstitutionAssignments_Users");
        });

        modelBuilder.Entity<Mark>(entity =>
        {
            entity.HasKey(e => e.Mark1);

            entity.Property(e => e.Mark1)
                .ValueGeneratedNever()
                .HasColumnName("Mark");
        });

        modelBuilder.Entity<PracticeType>(entity =>
        {
            entity.Property(e => e.PracticeTypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Caption).HasMaxLength(50);
        });

        modelBuilder.Entity<Speciality>(entity =>
        {
            entity.Property(e => e.SpecialityCode).HasMaxLength(50);
            entity.Property(e => e.SpecialityName).HasMaxLength(150);

            entity.HasOne(d => d.Institution).WithMany(p => p.Specialities)
                .HasForeignKey(d => d.InstitutionId)
                .HasConstraintName("FK_Specialities_Institutions");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);

            entity.HasOne(d => d.Speciality).WithMany(p => p.Students)
                .HasForeignKey(d => d.SpecialityId)
                .HasConstraintName("FK_Students_Specialities");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);
            entity.Property(e => e.Password)
                .HasMaxLength(1024)
                .IsFixedLength();

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
