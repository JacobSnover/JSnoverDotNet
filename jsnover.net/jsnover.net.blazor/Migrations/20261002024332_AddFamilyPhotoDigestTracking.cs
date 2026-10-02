using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace jsnover.net.blazor.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyPhotoDigestTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FamilyPhotoDigestDelivery",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "int", nullable: false),
                    DeliveredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WasBaseline = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPhotoDigestDelivery", x => x.PhotoId);
                });

            migrationBuilder.CreateTable(
                name: "FamilyPhotoDigestState",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaselineEstablished = table.Column<bool>(type: "bit", nullable: false),
                    BaselineEstablishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LeaseExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPhotoDigestState", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "FamilyPhotoDigestState",
                columns: new[] { "Id", "BaselineEstablished", "BaselineEstablishedAtUtc", "LeaseExpiresAtUtc", "LeaseId" },
                values: new object[] { 1, false, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyPhotoDigestDelivery");

            migrationBuilder.DropTable(
                name: "FamilyPhotoDigestState");

        }
    }
}
