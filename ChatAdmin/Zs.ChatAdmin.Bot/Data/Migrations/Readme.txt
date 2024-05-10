1 PS> dotnet ef migrations add InitialChatAdminContext --context ChatAdminContext --output-dir "Data\Migrations"

2. Add to MigrationBuilder.Up(...) method:
            migrationBuilder.Sql(Zs.Bot.Data.PostgreSQL.PostgreSqlBotContext.GetOtherSqlScripts("appsettings.json"));
            migrationBuilder.Sql(ChatAdminContext.GetOtherSqlScripts("appsettings.json"));

3. PS> dotnet ef database update --context ChatAdminContext


// ADD Microsoft.EntityFrameworkCore.Design
// THEN dotnet tool update --global dotnet-ef
//   OR dotnet tool install --global dotnet-ef  (dotnet tool update --global dotnet-ef)
