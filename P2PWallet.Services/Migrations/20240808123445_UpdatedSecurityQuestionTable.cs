using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSecurityQuestionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "SecurityQuestions");

            migrationBuilder.DropColumn(
                name: "Question",
                table: "SecurityQuestions");

            migrationBuilder.AddColumn<byte[]>(
                name: "AnswerHash",
                table: "SecurityQuestions",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "AnswerSalt",
                table: "SecurityQuestions",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "SeedSecurityQuestionId",
                table: "SecurityQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityQuestions_SeedSecurityQuestionId",
                table: "SecurityQuestions",
                column: "SeedSecurityQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityQuestions_SeededSecurityQuestions_SeedSecurityQuestionId",
                table: "SecurityQuestions",
                column: "SeedSecurityQuestionId",
                principalTable: "SeededSecurityQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityQuestions_SeededSecurityQuestions_SeedSecurityQuestionId",
                table: "SecurityQuestions");

            migrationBuilder.DropIndex(
                name: "IX_SecurityQuestions_SeedSecurityQuestionId",
                table: "SecurityQuestions");

            migrationBuilder.DropColumn(
                name: "AnswerHash",
                table: "SecurityQuestions");

            migrationBuilder.DropColumn(
                name: "AnswerSalt",
                table: "SecurityQuestions");

            migrationBuilder.DropColumn(
                name: "SeedSecurityQuestionId",
                table: "SecurityQuestions");

            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "SecurityQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Question",
                table: "SecurityQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
