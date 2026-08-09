using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenWorld.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
[Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(GreenWorldDbContext))]
[Microsoft.EntityFrameworkCore.Migrations.Migration("20250101000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "neighbourhoods",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                SimulationStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_neighbourhoods", x => x.Id));

        migrationBuilder.CreateTable(
            name: "sites",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                NeighbourhoodId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                site_type = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_sites", x => x.Id);
                table.ForeignKey("FK_sites_neighbourhoods_NeighbourhoodId", x => x.NeighbourhoodId,
                    "neighbourhoods", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "assets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SiteId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CapacityKwp = table.Column<double>(type: "double precision", nullable: false),
                RatedPowerKw = table.Column<double>(type: "double precision", nullable: false),
                ScaleFactor = table.Column<double>(type: "double precision", nullable: false),
                Seed = table.Column<int>(type: "integer", nullable: false),
                CumulativeConsumedKwh = table.Column<double>(type: "double precision", nullable: false),
                CumulativeGeneratedKwh = table.Column<double>(type: "double precision", nullable: false),
                LastPowerKw = table.Column<double>(type: "double precision", nullable: false),
                LastReadingAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_assets", x => x.Id);
                table.ForeignKey("FK_assets_sites_SiteId", x => x.SiteId,
                    "sites", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "meter_readings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                EnergyKwh = table.Column<double>(type: "double precision", nullable: false),
                PowerKw = table.Column<double>(type: "double precision", nullable: false),
                Direction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_meter_readings", x => x.Id));

        migrationBuilder.CreateTable(
            name: "neighbourhood_aggregate_snapshots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                NeighbourhoodId = table.Column<Guid>(type: "uuid", nullable: false),
                At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Season = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                TemperatureCelsius = table.Column<double>(type: "double precision", nullable: false),
                CloudCover = table.Column<double>(type: "double precision", nullable: false),
                IrradianceFactor = table.Column<double>(type: "double precision", nullable: false),
                TotalConsumptionKw = table.Column<double>(type: "double precision", nullable: false),
                TotalGenerationKw = table.Column<double>(type: "double precision", nullable: false),
                CumulativeConsumedKwh = table.Column<double>(type: "double precision", nullable: false),
                CumulativeGeneratedKwh = table.Column<double>(type: "double precision", nullable: false),
                BatteryPowerKw = table.Column<double>(type: "double precision", nullable: false),
                BatterySocKwh = table.Column<double>(type: "double precision", nullable: false),
                NetLoadWithBatteryKw = table.Column<double>(type: "double precision", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_neighbourhood_aggregate_snapshots", x => x.Id));

        migrationBuilder.CreateIndex("IX_sites_NeighbourhoodId", "sites", "NeighbourhoodId");
        migrationBuilder.CreateIndex("IX_assets_SiteId", "assets", "SiteId");
        migrationBuilder.CreateIndex("IX_meter_readings_AssetId_OccurredAt", "meter_readings",
            new[] { "AssetId", "OccurredAt" });
        migrationBuilder.CreateIndex("IX_neighbourhood_aggregate_snapshots_NeighbourhoodId_At",
            "neighbourhood_aggregate_snapshots", new[] { "NeighbourhoodId", "At" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("assets");
        migrationBuilder.DropTable("meter_readings");
        migrationBuilder.DropTable("neighbourhood_aggregate_snapshots");
        migrationBuilder.DropTable("sites");
        migrationBuilder.DropTable("neighbourhoods");
    }
}
