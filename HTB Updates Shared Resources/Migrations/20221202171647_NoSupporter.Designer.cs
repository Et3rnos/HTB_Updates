﻿// <auto-generated />
using System;
using HTB_Updates_Shared_Resources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace HTB_Updates_Shared_Resources.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20221202171647_NoSupporter")]
    partial class NoSupporter
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Database.DiscordGuild", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("MessageNewMembers")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("OptionalAnnouncements")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("HTBUpdates_DiscordGuilds");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Database.DiscordUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<ulong>("DiscordId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.ToTable("HTBUpdates_DiscordUsers");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Database.GuildUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("DiscordUserId")
                        .HasColumnType("int");

                    b.Property<int>("GuildId")
                        .HasColumnType("int");

                    b.Property<int>("HTBUserId")
                        .HasColumnType("int");

                    b.Property<bool>("Verified")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("DiscordUserId");

                    b.HasIndex("GuildId");

                    b.HasIndex("HTBUserId");

                    b.ToTable("HTBUpdates_GuildUsers");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Database.HTBUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("HtbId")
                        .HasColumnType("int");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Score")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("HTBUpdates_HTBUsers");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Shared.Solve", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ChallengeCategory")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("DateDiff")
                        .HasColumnType("longtext");

                    b.Property<bool>("FirstBlood")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("HTBUserId")
                        .HasColumnType("int");

                    b.Property<string>("MachineAvatar")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("ObjectType")
                        .HasColumnType("longtext");

                    b.Property<int>("Points")
                        .HasColumnType("int");

                    b.Property<int>("SolveId")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("HTBUserId");

                    b.ToTable("HTBUpdates_Solves");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Database.GuildUser", b =>
                {
                    b.HasOne("HTB_Updates_Shared_Resources.Models.Database.DiscordUser", "DiscordUser")
                        .WithMany("GuildUsers")
                        .HasForeignKey("DiscordUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HTB_Updates_Shared_Resources.Models.Database.DiscordGuild", "Guild")
                        .WithMany("GuildUsers")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("HTB_Updates_Shared_Resources.Models.Database.HTBUser", "HTBUser")
                        .WithMany("GuildUsers")
                        .HasForeignKey("HTBUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DiscordUser");

                    b.Navigation("Guild");

                    b.Navigation("HTBUser");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Shared.Solve", b =>
                {
                    b.HasOne("HTB_Updates_Shared_Resources.Models.Database.HTBUser", "HTBUser")
                        .WithMany("Solves")
                        .HasForeignKey("HTBUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HTBUser");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Database.DiscordGuild", b =>
                {
                    b.Navigation("GuildUsers");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Database.DiscordUser", b =>
                {
                    b.Navigation("GuildUsers");
                });

            modelBuilder.Entity("HTB_Updates_Shared_Resources.Models.Database.HTBUser", b =>
                {
                    b.Navigation("GuildUsers");

                    b.Navigation("Solves");
                });
#pragma warning restore 612, 618
        }
    }
}
