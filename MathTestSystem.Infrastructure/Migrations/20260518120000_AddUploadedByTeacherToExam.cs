using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MathTestSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedByTeacherToExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UploadedByTeacherId",
                table: "Exams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Exams_UploadedByTeacherId",
                table: "Exams",
                column: "UploadedByTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_Teachers_UploadedByTeacherId",
                table: "Exams",
                column: "UploadedByTeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_Teachers_UploadedByTeacherId",
                table: "Exams");

            migrationBuilder.DropIndex(
                name: "IX_Exams_UploadedByTeacherId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "UploadedByTeacherId",
                table: "Exams");
        }
    }
}
