using FluentMigrator.Builders.Create.Table;

namespace Lib.Db.Extensions;

public static class MigrationsExtensions
{
    public static ICreateTableColumnOptionOrWithColumnSyntax AsJsonB(this ICreateTableColumnAsTypeSyntax x) 
        => x.AsCustom("jsonb");

    public static ICreateTableColumnOptionOrWithColumnSyntax AsJson(this ICreateTableColumnAsTypeSyntax x) 
        => x.AsCustom("json");

    public static ICreateTableColumnOptionOrWithColumnSyntax AsHstore(this ICreateTableColumnAsTypeSyntax x) 
        => x.AsCustom("hstore");
}
