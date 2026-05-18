using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MathTestSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorExamTaskSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamTasks_Exams_ExamId",
                table: "ExamTasks");

            migrationBuilder.DropIndex(
                name: "IX_ExamTasks_ExamId",
                table: "ExamTasks");

            migrationBuilder.DropColumn(
                name: "ExamId",
                table: "ExamTasks");

            migrationBuilder.DropColumn(
                name: "HasError",
                table: "ExamTasks");

            migrationBuilder.AddColumn<Guid>(
                name: "ExamUid",
                table: "ExamTasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Exams_Uid",
                table: "Exams",
                column: "Uid");

            migrationBuilder.CreateIndex(
                name: "IX_ExamTasks_ExamUid",
                table: "ExamTasks",
                column: "ExamUid");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTasks_Exams_ExamUid",
                table: "ExamTasks",
                column: "ExamUid",
                principalTable: "Exams",
                principalColumn: "Uid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamTasks_Exams_ExamUid",
                table: "ExamTasks");

            migrationBuilder.DropIndex(
                name: "IX_ExamTasks_ExamUid",
                table: "ExamTasks");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Exams_Uid",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ExamUid",
                table: "ExamTasks");

            migrationBuilder.AddColumn<int>(
                name: "ExamId",
                table: "ExamTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasError",
                table: "ExamTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ExamTasks_ExamId",
                table: "ExamTasks",
                column: "ExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamTasks_Exams_ExamId",
                table: "ExamTasks",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
