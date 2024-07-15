using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class EditDeposit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "userId",
                table: "Deposits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_userId",
                table: "Deposits",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposits_Users_userId",
                table: "Deposits",
                column: "userId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deposits_Users_userId",
                table: "Deposits");

            migrationBuilder.DropIndex(
                name: "IX_Deposits_userId",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "Deposits");
        }
    }
}
