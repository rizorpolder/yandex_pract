using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BackfillUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO users (id, login, password_hash, role)
                VALUES ('11111111-1111-1111-1111-111111111111', 'system', '', 'User')
                ON CONFLICT DO NOTHING;
            ");

            migrationBuilder.Sql(@"
                UPDATE bookings
                SET ""UserId"" = '11111111-1111-1111-1111-111111111111'
                WHERE ""UserId"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM users
                WHERE id = '11111111-1111-1111-1111-111111111111';
            ");
        }
    }
}
