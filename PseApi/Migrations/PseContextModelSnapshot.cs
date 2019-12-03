﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PseApi.Data;

namespace PseApi.Migrations
{
    [DbContext(typeof(PseContext))]
    partial class PseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("PseApi.Data.Dataset", b =>
                {
                    b.Property<DateTime>("Day")
                        .HasColumnType("date");

                    b.HasKey("Day");

                    b.ToTable("days");
                });

            modelBuilder.Entity("PseApi.Data.Stock", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("BIC")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ISIN")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("BIC")
                        .IsUnique();

                    b.HasIndex("ISIN")
                        .IsUnique();

                    b.ToTable("Stocks");
                });

            modelBuilder.Entity("PseApi.Data.Trade", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("BIC")
                        .HasColumnType("varchar(255)");

                    b.Property<decimal>("Change")
                        .HasColumnType("decimal(16,4)");

                    b.Property<decimal>("Close")
                        .HasColumnType("decimal(16,4)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<decimal>("DayMax")
                        .HasColumnType("decimal(16,4)");

                    b.Property<decimal>("DayMin")
                        .HasColumnType("decimal(16,4)");

                    b.Property<string>("ISIN")
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("LastTrade")
                        .HasColumnType("date");

                    b.Property<int>("LotSize")
                        .HasColumnType("int");

                    b.Property<string>("MarketCode")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("MarketGroup")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Mode")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<decimal>("Open")
                        .HasColumnType("decimal(16,4)");

                    b.Property<decimal>("Previous")
                        .HasColumnType("decimal(16,4)");

                    b.Property<decimal>("TradedAmount")
                        .HasColumnType("decimal(16,4)");

                    b.Property<long>("Volume")
                        .HasColumnType("bigint");

                    b.Property<decimal>("YearMax")
                        .HasColumnType("decimal(16,4)");

                    b.Property<decimal>("YearMin")
                        .HasColumnType("decimal(16,4)");

                    b.HasKey("Id");

                    b.HasIndex("BIC");

                    b.HasIndex("Date");

                    b.HasIndex("ISIN");

                    b.ToTable("trades");
                });
#pragma warning restore 612, 618
        }
    }
}
