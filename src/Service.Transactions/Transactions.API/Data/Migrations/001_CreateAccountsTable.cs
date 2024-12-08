using FluentMigrator;

namespace Transactions.API.Data.Migrations;

[Migration(001)]
public class CreateAccountsTable : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("accounts")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("space_id").AsGuid().NotNullable()
            .WithColumn("type").AsString().NotNullable();
    }
}