using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace ChatAdmin.Bot.Data.Migrations
{
    public partial class InitialChatAdminContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ca");

            migrationBuilder.EnsureSchema(
                name: "bot");

            migrationBuilder.CreateTable(
                name: "accountings",
                schema: "ca",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accountings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "auxiliary_words",
                schema: "ca",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auxiliary_words", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chat_types",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "commands",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    script = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    default_args = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    group = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "message_types",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "messengers",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messengers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "ca",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    month = table.Column<int>(type: "integer", nullable: true),
                    day = table.Column<int>(type: "integer", nullable: false),
                    hour = table.Column<int>(type: "integer", nullable: false),
                    minute = table.Column<int>(type: "integer", nullable: false),
                    exec_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permissions = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chats",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    chat_type_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.id);
                    table.ForeignKey(
                        name: "FK_chats_chat_types_chat_type_id",
                        column: x => x.chat_type_id,
                        principalSchema: "bot",
                        principalTable: "chat_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    full_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_role_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_bot = table.Column<bool>(type: "bool", nullable: false),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "bot",
                        principalTable: "user_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    reply_to_message_id = table.Column<int>(type: "integer", nullable: true),
                    messenger_id = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    message_type_id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    chat_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    is_succeed = table.Column<bool>(type: "bool", nullable: false),
                    fails_count = table.Column<int>(type: "integer", nullable: false),
                    fail_description = table.Column<string>(type: "json", nullable: true),
                    is_deleted = table.Column<bool>(type: "bool", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_chats_chat_id",
                        column: x => x.chat_id,
                        principalSchema: "bot",
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_message_types_message_type_id",
                        column: x => x.message_type_id,
                        principalSchema: "bot",
                        principalTable: "message_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_messages_reply_to_message_id",
                        column: x => x.reply_to_message_id,
                        principalSchema: "bot",
                        principalTable: "messages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_messages_messengers_messenger_id",
                        column: x => x.messenger_id,
                        principalSchema: "bot",
                        principalTable: "messengers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "bot",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bans",
                schema: "ca",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    chat_id = table.Column<int>(type: "integer", nullable: false),
                    warning_message_id = table.Column<int>(type: "integer", nullable: true),
                    finish_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bans", x => x.id);
                    table.ForeignKey(
                        name: "FK_bans_chats_chat_id",
                        column: x => x.chat_id,
                        principalSchema: "bot",
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bans_messages_warning_message_id",
                        column: x => x.warning_message_id,
                        principalSchema: "bot",
                        principalTable: "messages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_bans_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "bot",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "chat_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { "CHANNEL", "Channel" },
                    { "GROUP", "Group" },
                    { "PRIVATE", "Private" },
                    { "UNDEFINED", "Undefined" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "commands",
                columns: new[] { "id", "default_args", "description", "group", "script" },
                values: new object[,]
                {
                    { "/getuserstatistics", "15; now()::Date; now()", "Получение статистики по активности участников всех чатов за определённый период", "adminCmdGroup", "SELECT string_agg(function_results.val, ' ') as Result\r\n                               FROM (\r\n                                   SELECT * FROM (\r\n                                       SELECT ca.sf_cmd_get_full_statistics({0}, {1}, {2}) as val, 1 as order\r\n                                       UNION\r\n                                       SELECT chr(10) || chr(10) || 'The most popular words:' || chr(10) || string_agg(w.word || ' (' || w.count || ')', ',  ') as val, 2 as order\r\n                                       FROM (SELECT * FROM ca.sf_get_most_popular_words(1, {1}, {2}, 2) LIMIT 10) w\r\n                                       UNION\r\n                                       SELECT chr(10) || chr(10) || 'Bans and warnings:' || chr(10) || string_agg(user_name || ' - ' || status, chr(10)) as val, 3 as order\r\n                                       FROM (SELECT * FROM ca.sf_get_bans(1, {1}, {2})) b\r\n                                   ) r\r\n                               ORDER BY r.order\r\n                               ) function_results" },
                    { "/help", "<UserRoleId>", "Получение справки по доступным функциям", "userCmdGroup", "SELECT bot.sf_cmd_get_help({0})" },
                    { "/nulltest", null, "Тестовый запрос к боту. Возвращает NULL", "moderatorCmdGroup", "SELECT null" },
                    { "/sqlquery", "select 'Pass your query as a parameter in double quotes'", "SQL-запрос", "adminCmdGroup", "select (with userQuery as ({0}) select json_agg(q) from userQuery q)" },
                    { "/test", null, "Тестовый запрос к боту. Возвращает ''Test''", "moderatorCmdGroup", "SELECT 'Test'" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "message_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { "AUD", "Audio" },
                    { "CNT", "Contact" },
                    { "DOC", "Document" },
                    { "LOC", "Location" },
                    { "OTH", "Other" },
                    { "PHT", "Photo" },
                    { "SRV", "Service message" },
                    { "STK", "Sticker" },
                    { "TXT", "Text" },
                    { "UKN", "Unknown" },
                    { "VID", "Video" },
                    { "VOI", "Voice" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "messengers",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { "DC", "Discord" },
                    { "FB", "Facebook" },
                    { "SK", "Skype" },
                    { "TG", "Telegram" },
                    { "VK", "Вконтакте" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "user_roles",
                columns: new[] { "id", "name", "permissions" },
                values: new object[,]
                {
                    { "ADMIN", "Administrator", "[ \"adminCmdGroup\", \"moderatorCmdGroup\", \"userCmdGroup\" ]" },
                    { "MODERATOR", "Moderator", "[ \"moderatorCmdGroup\", \"userCmdGroup\" ]" },
                    { "OWNER", "Owner", "[ \"All\" ]" },
                    { "USER", "User", "[ \"userCmdGroup\" ]" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "chats",
                columns: new[] { "id", "chat_type_id", "description", "insert_date", "name", "raw_data", "raw_data_hash", "raw_data_history" },
                values: new object[,]
                {
                    { -1, "PRIVATE", "IntegrationTestChat", new DateTime(2021, 10, 24, 7, 18, 6, 583, DateTimeKind.Utc).AddTicks(9791), "IntegrationTestChat", "{ \"test\": \"test\" }", "-1063294487", null },
                    { 1, "PRIVATE", null, new DateTime(2021, 10, 24, 7, 18, 6, 583, DateTimeKind.Utc).AddTicks(9793), "zuev56", "{ \"Id\": 210281448 }", "-1063294487", null }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "users",
                columns: new[] { "id", "full_name", "insert_date", "is_bot", "name", "raw_data", "raw_data_hash", "raw_data_history", "user_role_id" },
                values: new object[,]
                {
                    { -10, "for exported message reading", new DateTime(2021, 10, 24, 7, 18, 6, 583, DateTimeKind.Utc).AddTicks(9817), false, "Unknown", "{ \"test\": \"test\" }", "-1063294487", null, "USER" },
                    { -1, "IntegrationTest", new DateTime(2021, 10, 24, 7, 18, 6, 583, DateTimeKind.Utc).AddTicks(9819), false, "IntegrationTestUser", "{ \"test\": \"test\" }", "-1063294487", null, "USER" },
                    { 1, "Сергей Зуев", new DateTime(2021, 10, 24, 7, 18, 6, 583, DateTimeKind.Utc).AddTicks(9820), false, "zuev56", "{ \"Id\": 210281448 }", "-1063294487", null, "OWNER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_bans_chat_id",
                schema: "ca",
                table: "bans",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_bans_user_id",
                schema: "ca",
                table: "bans",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_bans_warning_message_id",
                schema: "ca",
                table: "bans",
                column: "warning_message_id");

            migrationBuilder.CreateIndex(
                name: "IX_chats_chat_type_id",
                schema: "bot",
                table: "chats",
                column: "chat_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_chat_id",
                schema: "bot",
                table: "messages",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_message_type_id",
                schema: "bot",
                table: "messages",
                column: "message_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_messenger_id",
                schema: "bot",
                table: "messages",
                column: "messenger_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_reply_to_message_id",
                schema: "bot",
                table: "messages",
                column: "reply_to_message_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_user_id",
                schema: "bot",
                table: "messages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_user_role_id",
                schema: "bot",
                table: "users",
                column: "user_role_id");

            migrationBuilder.Sql("CREATE ROLE app WITH NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT LOGIN NOREPLICATION NOBYPASSRLS CONNECTION LIMIT -1;");
            migrationBuilder.Sql(Zs.Bot.Data.PostgreSQL.PostgreSqlBotContext.GetOtherSqlScripts("appsettings.json"));
            migrationBuilder.Sql(ChatAdminContext.GetOtherSqlScripts("appsettings.json"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accountings",
                schema: "ca");

            migrationBuilder.DropTable(
                name: "auxiliary_words",
                schema: "ca");

            migrationBuilder.DropTable(
                name: "bans",
                schema: "ca");

            migrationBuilder.DropTable(
                name: "commands",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "ca");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "chats",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "message_types",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "messengers",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "users",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "chat_types",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "bot");
        }
    }
}
