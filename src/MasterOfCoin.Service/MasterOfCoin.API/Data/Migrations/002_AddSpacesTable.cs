using FluentMigrator;

namespace MasterOfCoin.API.Data.Migrations;

[Migration(002)]
public class AddSpacesTable : ForwardOnlyMigration 
{
    public override void Up()
    {
        Create.Table("spaces")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("user_id").AsGuid().NotNullable();
    }
}