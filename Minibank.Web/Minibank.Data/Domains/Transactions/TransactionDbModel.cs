using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minibank.Core.Domains.Currencies;
using System;

namespace Minibank.Data.Domain.Transactions
{
    public class TransactionDbModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public AvaliableCurrencies Currency { get; set; }
        public string FromAccountId { get; set; }
        public string ToAccountId { get; set; }
        public DateTime CreatedAt { get; set; }

        internal class Map : IEntityTypeConfiguration<TransactionDbModel>
        {
            public void Configure(EntityTypeBuilder<TransactionDbModel> builder)
            {
                builder.ToTable("transaction");

                builder.HasKey(it => it.Id).HasName("pk_transaction_id");
            }
        }
    }
}
