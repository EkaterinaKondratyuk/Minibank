// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Minibank.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Minibank.Data.Migrations
{
    [DbContext(typeof(MinibankContext))]
    [Migration("20220410114837_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.15");

            modelBuilder.Entity("Minibank.Data.Domain.Transactions.TransactionDbModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric")
                        .HasColumnName("amount");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<int>("Currency")
                        .HasColumnType("integer")
                        .HasColumnName("currency");

                    b.Property<string>("FromAccountId")
                        .HasColumnType("text")
                        .HasColumnName("from_account_id");

                    b.Property<string>("ToAccountId")
                        .HasColumnType("text")
                        .HasColumnName("to_account_id");

                    b.HasKey("Id")
                        .HasName("pk_transaction_id");

                    b.ToTable("transaction");
                });

            modelBuilder.Entity("Minibank.Data.Domain.Users.UserDbModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<string>("Email")
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("Login")
                        .HasColumnType("text")
                        .HasColumnName("login");

                    b.HasKey("Id")
                        .HasName("pk_user_id");

                    b.ToTable("user");
                });

            modelBuilder.Entity("Minibank.Data.Domains.BankAccounts.BankAccountDbModel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric")
                        .HasColumnName("balance");

                    b.Property<DateTime?>("ClosingDate")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("closing_date");

                    b.Property<int>("Currency")
                        .HasColumnType("integer")
                        .HasColumnName("currency");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");

                    b.Property<DateTime>("OpeningDate")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("opening_date");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_bank_account_id");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_bank_accounts_user_id");

                    b.ToTable("bank_account");
                });

            modelBuilder.Entity("Minibank.Data.Domains.BankAccounts.BankAccountDbModel", b =>
                {
                    b.HasOne("Minibank.Data.Domain.Users.UserDbModel", "User")
                        .WithMany("BankAccounts")
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_bank_accounts_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Minibank.Data.Domain.Users.UserDbModel", b =>
                {
                    b.Navigation("BankAccounts");
                });
#pragma warning restore 612, 618
        }
    }
}
