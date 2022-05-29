using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Minibank.Data.Domains.BankAccounts;
using System.Collections.Generic;

namespace Minibank.Data.Domain.Users
{
    public class UserDbModel
    {
        public string Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public virtual List<BankAccountDbModel> BankAccounts { get; set; }

        internal class Map : IEntityTypeConfiguration<UserDbModel>
        {
            public void Configure(EntityTypeBuilder<UserDbModel> builder)
            {
                builder.ToTable("user");

                builder.HasKey(it => it.Id).HasName("pk_user_id");
            }
        }
    }
}
