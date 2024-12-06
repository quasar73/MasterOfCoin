using FluentMigrator;

namespace MasterOfCoin.API.Data.Migrations;

[Migration(001)]
public class AddUserTable : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("username").AsString(128).NotNullable().Unique()
            .WithColumn("password_hash").AsString().NotNullable()
            .WithColumn("password_salt").AsBinary(16).NotNullable()
            .WithColumn("email").AsString().NotNullable()
            .WithColumn("avatar").AsString().Nullable()
            .WithColumn("displayed_name").AsString().NotNullable();
    }
}