using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModelSQLLiteFarm.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    MaterialID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MaterialName = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 10, nullable: true),
                    MaterialPrice = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.MaterialID);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerName = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 50, nullable: false),
                    OrderName = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 50, nullable: false),
                    OrderPrice = table.Column<double>(type: "REAL", nullable: false),
                    PrintJobsCost = table.Column<double>(type: "REAL", nullable: true),
                    PrintJobsCount = table.Column<int>(type: "INTEGER", nullable: true),
                    DateIn = table.Column<DateTime>(type: "DateTime", nullable: true),
                    DateDue = table.Column<DateTime>(type: "DateTime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
                });

            migrationBuilder.CreateTable(
                name: "Printers",
                columns: table => new
                {
                    PrinterID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrinterType = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 20, nullable: false),
                    PrinterPurchDate = table.Column<DateTime>(type: "DateTime", nullable: true),
                    PrinterPurchPrice = table.Column<double>(type: "REAL", nullable: false),
                    MRTimeEst = table.Column<double>(type: "REAL", nullable: true),
                    ServiceTimeEst = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Printers", x => x.PrinterID);
                });

            migrationBuilder.CreateTable(
                name: "ServiceLogEvents",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventName = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 50, nullable: false),
                    EventDate = table.Column<DateTime>(type: "DateTime", nullable: true),
                    EventLength_hrs = table.Column<int>(type: "INTEGER", nullable: true),
                    EventCategory = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 10, nullable: true),
                    EventCost_euro = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceLogEvents", x => x.EventID);
                });

            migrationBuilder.CreateTable(
                name: "PrintJobs",
                columns: table => new
                {
                    JobID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobName = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 50, nullable: false),
                    OrderID = table.Column<int>(type: "INTEGER", nullable: true),
                    Material = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 3, nullable: true),
                    WeightMaterial = table.Column<double>(type: "REAL", nullable: true),
                    NozzleDiam_mm = table.Column<double>(type: "REAL", nullable: true),
                    LayerHeight = table.Column<int>(type: "INTEGER", nullable: true),
                    VolX = table.Column<int>(type: "INTEGER", nullable: true),
                    VolY = table.Column<int>(type: "INTEGER", nullable: true),
                    VolZ = table.Column<int>(type: "INTEGER", nullable: true),
                    PrinterType = table.Column<int>(type: "INTEGER", nullable: false),
                    GcodeAdress = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 200, nullable: true),
                    PrintTime = table.Column<double>(type: "REAL", nullable: true),
                    Costs = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintJobs", x => x.JobID);
                    table.ForeignKey(
                        name: "FK_PrintJobs_Orders",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                });

            migrationBuilder.CreateTable(
                name: "PrintersMaterials",
                columns: table => new
                {
                    PMPrintersFK = table.Column<int>(type: "INTEGER", nullable: true),
                    PMMaterialsFK = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PrintersMaterials_Materials",
                        column: x => x.PMMaterialsFK,
                        principalTable: "Materials",
                        principalColumn: "MaterialID");
                    table.ForeignKey(
                        name: "FK_PrintersMaterials_Printers",
                        column: x => x.PMPrintersFK,
                        principalTable: "Printers",
                        principalColumn: "PrinterID");
                });

            migrationBuilder.CreateTable(
                name: "PrintersServices",
                columns: table => new
                {
                    PSPrintersFK = table.Column<int>(type: "INTEGER", nullable: false),
                    PSServicesFK = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_PrintersServices_Printers1",
                        column: x => x.PSPrintersFK,
                        principalTable: "Printers",
                        principalColumn: "PrinterID");
                    table.ForeignKey(
                        name: "FK_PrintersServices_ServiceLogEvents",
                        column: x => x.PSServicesFK,
                        principalTable: "ServiceLogEvents",
                        principalColumn: "EventID");
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    JobScheduleID = table.Column<int>(type: "INTEGER", nullable: false),
                    PrinterID = table.Column<int>(type: "INTEGER", nullable: false),
                    JobID = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStart = table.Column<DateTime>(type: "DateTime", nullable: true),
                    TimeEnd = table.Column<DateTime>(type: "DateTime", nullable: true),
                    MR_Time = table.Column<double>(type: "REAL", nullable: true),
                    RO_Time = table.Column<double>(type: "REAL", nullable: true),
                    ScheduleWeek = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.JobScheduleID);
                    table.ForeignKey(
                        name: "FK_Schedule_PrintJobs",
                        column: x => x.JobID,
                        principalTable: "PrintJobs",
                        principalColumn: "JobID");
                    table.ForeignKey(
                        name: "FK_Schedule_Printers",
                        column: x => x.PrinterID,
                        principalTable: "Printers",
                        principalColumn: "PrinterID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrintersMaterials_PMMaterialsFK",
                table: "PrintersMaterials",
                column: "PMMaterialsFK");

            migrationBuilder.CreateIndex(
                name: "IX_PrintersMaterials_PMPrintersFK",
                table: "PrintersMaterials",
                column: "PMPrintersFK");

            migrationBuilder.CreateIndex(
                name: "IX_PrintersServices_PSPrintersFK",
                table: "PrintersServices",
                column: "PSPrintersFK");

            migrationBuilder.CreateIndex(
                name: "IX_PrintersServices_PSServicesFK",
                table: "PrintersServices",
                column: "PSServicesFK");

            migrationBuilder.CreateIndex(
                name: "IX_PrintJobs_OrderID",
                table: "PrintJobs",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_PrinterID",
                table: "Schedule",
                column: "PrinterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrintersMaterials");

            migrationBuilder.DropTable(
                name: "PrintersServices");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "ServiceLogEvents");

            migrationBuilder.DropTable(
                name: "PrintJobs");

            migrationBuilder.DropTable(
                name: "Printers");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
