using FluentMigrator;

namespace Categories.API.Data.Migrations;

[Migration(001)]
public class CreateCategoriesTable : ForwardOnlyMigration
{
    public override void Up()
    {
        Create.Table("categories")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("space_id").AsGuid().NotNullable()
            .WithColumn("account_id").AsGuid().NotNullable()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("parent_id").AsGuid().Nullable()
            .WithColumn("icon").AsString().Nullable()
            .WithColumn("color").AsString().Nullable();
    }
}