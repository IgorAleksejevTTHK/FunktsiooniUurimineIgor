using System;
using FunktsiooniUurimineWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FunktsiooniUurimineWeb.Migrations
{
    [DbContext(typeof(RakenduseKontekst))]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("FunktsiooniUurimineWeb.Models.FunktsiooniUurimine", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Ekstreemumid")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("KriitilisedPunktid")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<DateTime>("LuodudAeg")
                    .HasColumnType("TEXT");

                b.Property<string>("Nullkohad")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Maaramispiirkond")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Tuletis")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Valem")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("TEXT");

                b.HasKey("Id");
                b.ToTable("FunktsiooniUurimised");
            });
#pragma warning restore 612, 618
        }
    }
}
