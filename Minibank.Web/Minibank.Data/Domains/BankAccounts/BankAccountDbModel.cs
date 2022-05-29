using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minibank.Core.Domains.Currencies;
using Minibank.Data.Domain.Users;
using System;

namespace Minibank.Data.Domains.BankAccounts
{
    public class BankAccountDbModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public decimal Balance { get; set; }
        public AvaliableCurrencies Currency { get; set; }
        public bool IsActive { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime? ClosingDate { get; set; }
        public virtual UserDbModel User { get; set; }

        internal class Map : IEntityTypeConfiguration<BankAccountDbModel>
        {
            public void Configure(EntityTypeBuilder<BankAccountDbModel> builder)
            {
                builder.ToTable("bank_account");

                builder.HasKey(it => it.Id).HasName("pk_bank_account_id");

                builder.Property(it => it.UserId)
                    .IsRequired();

                builder.HasOne(it => it.User)
                    .WithMany(it => it.BankAccounts)
                    .HasForeignKey(it => it.UserId);
            }
        }
    }
}
