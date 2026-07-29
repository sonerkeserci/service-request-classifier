using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RequestClassifier.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionScoreMargin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "PredictionScore",
                table: "ServiceRequests",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "PredictionScoreMargin",
                table: "ServiceRequests",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PredictionScoreMargin",
                table: "ServiceRequests");

            migrationBuilder.AlterColumn<float>(
                name: "PredictionScore",
                table: "ServiceRequests",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
