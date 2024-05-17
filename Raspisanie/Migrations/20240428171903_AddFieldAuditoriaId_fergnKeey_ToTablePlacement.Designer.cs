﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Raspisanie.Data;

#nullable disable

namespace Raspisanie.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240428171903_AddFieldAuditoriaId_fergnKeey_ToTablePlacement")]
    partial class AddFieldAuditoriaId_fergnKeey_ToTablePlacement
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.24")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Raspisanie.Models.Auditoria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("AuditoryName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LekciaOrPractica")
                        .HasColumnType("bit");

                    b.Property<bool>("Proektor")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Auditoria");
                });

            modelBuilder.Entity("Raspisanie.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("AuditoriaId")
                        .HasColumnType("int");

                    b.Property<int>("Course")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Specialnost")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AuditoriaId");

                    b.ToTable("Group");
                });

            modelBuilder.Entity("Raspisanie.Models.Placement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("AuditoriaId")
                        .HasColumnType("int");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<int>("PredmetId")
                        .HasColumnType("int");

                    b.Property<int>("index")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AuditoriaId");

                    b.HasIndex("GroupId");

                    b.HasIndex("PredmetId");

                    b.ToTable("Placement");
                });

            modelBuilder.Entity("Raspisanie.Models.Predmet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("Hours")
                        .HasColumnType("int");

                    b.Property<string>("PredmetName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TeacherId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TeacherId");

                    b.ToTable("Predmet");
                });

            modelBuilder.Entity("Raspisanie.Models.Teacher", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("AuditoryId")
                        .HasColumnType("int");

                    b.Property<string>("TeacherName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AuditoryId");

                    b.ToTable("Teacher");
                });

            modelBuilder.Entity("Raspisanie.Models.Group", b =>
                {
                    b.HasOne("Raspisanie.Models.Auditoria", "Auditoria")
                        .WithMany()
                        .HasForeignKey("AuditoriaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Auditoria");
                });

            modelBuilder.Entity("Raspisanie.Models.Placement", b =>
                {
                    b.HasOne("Raspisanie.Models.Auditoria", "Auditoria")
                        .WithMany()
                        .HasForeignKey("AuditoriaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Raspisanie.Models.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Raspisanie.Models.Predmet", "Predmet")
                        .WithMany()
                        .HasForeignKey("PredmetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Auditoria");

                    b.Navigation("Group");

                    b.Navigation("Predmet");
                });

            modelBuilder.Entity("Raspisanie.Models.Predmet", b =>
                {
                    b.HasOne("Raspisanie.Models.Teacher", "Teacher")
                        .WithMany()
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("Raspisanie.Models.Teacher", b =>
                {
                    b.HasOne("Raspisanie.Models.Auditoria", "Auditoria")
                        .WithMany()
                        .HasForeignKey("AuditoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Auditoria");
                });
#pragma warning restore 612, 618
        }
    }
}
