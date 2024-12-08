using FluentMigrator;

namespace Wallets.API.Data.Migrations;

[Migration(001)]
public class CreateWalletsTable : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("wallets")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(64).NotNullable()
            .WithColumn("currency").AsString(6).NotNullable()
            .WithColumn("value").AsDecimal().NotNullable()
            .WithColumn("cumulative").AsBoolean().NotNullable()
            .WithColumn("space_id").AsGuid().NotNullable()
            .WithColumn("account_id").AsGuid().NotNullable()
            .WithColumn("archived").AsBoolean().NotNullable();
    }
}