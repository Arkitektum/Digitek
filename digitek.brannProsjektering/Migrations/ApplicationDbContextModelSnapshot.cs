﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using digitek.brannProsjektering.Persistence;

namespace digitek.brannProsjektering.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("digitek.brannProsjektering.Models.UseRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateTime");

                    b.Property<string>("Email");

                    b.Property<string>("ExecutionNr");

                    b.Property<string>("InputJson");

                    b.Property<string>("Kapitel");

                    b.Property<string>("Model");

                    b.Property<string>("Navn");

                    b.Property<string>("OrganisasjonsNavn");

                    b.Property<string>("Organisasjonsnummer");

                    b.Property<int>("ResponseCode");

                    b.Property<string>("ResponseText");

                    b.HasKey("Id");

                    b.ToTable("UseRecords");
                });
#pragma warning restore 612, 618
        }
    }
}
