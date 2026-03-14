using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CSCMasterAPI.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_District", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aadhaar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginAllowed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Member",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Block = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aadhaar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnboardDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginAllowed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Member", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Member_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Member_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDistrict",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDistrict", x => new { x.UserId, x.DistrictId });
                    table.ForeignKey(
                        name: "FK_UserDistrict_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDistrict_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Entrollment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Block = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EIDDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChildName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uploaded = table.Column<bool>(type: "bit", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatorAadhaar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    GST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CC = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entrollment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entrollment_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entrollment_Member_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entrollment_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "District",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Thiruvarur" },
                    { 2, "Nagapattinam" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Aadhaar", "LoginAllowed", "Name", "Password", "Phone", "Status" },
                values: new object[,]
                {
                    { 1, "123456789012", true, "Retinue", "Red@123", "9943135008", true },
                    { 2, "123456789013", true, "Karthi", "Karthi123", "9444488760", true }
                });

            migrationBuilder.InsertData(
                table: "Member",
                columns: new[] { "Id", "Aadhaar", "Block", "District", "DistrictId", "LoginAllowed", "Mode", "Name", "OnboardDate", "Password", "Phone", "StationId", "Status", "UserId" },
                values: new object[,]
                {
                    { 1, "781191892916", "Mannargudi", "Thiruvarur", 1, true, "Online", "PRAKATHI V", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "40288@", "8428516150", "40288", true, 2 },
                    { 2, "414969174894", "KODAVSAL", "Thiruvarur", 1, true, "Online", "VIVEKA RAJASEKARAN", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "40287@", "9688129202", "40287", true, 2 },
                    { 3, "520549764245", "THIRUTHURAIPOONDI\r\n", "Thiruvarur", 1, true, "Online", "PRAVINSHA ELAVARASAN", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "40286@", "9047595234", "40286", true, 2 },
                    { 4, "741948749615", "Mannargudi", "Thiruvarur", 1, true, "Online", "S SOUNDARAPANDIAN", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "32115@", "7904784390", "32115", true, 2 },
                    { 5, "283319238556", "MUTHUPET\r\n", "Thiruvarur", 1, true, "Online", "Ganesh anbazhagan", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "52142@", "9047430909", "52142", true, 2 },
                    { 6, "677251519950", "Mannargudi", "Thiruvarur", 1, true, "Online", "Mohamed Yunus M", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "8228@", "7200882629", "8228", true, 2 },
                    { 7, "401851195105", "NEEDAMANGALAM", "Thiruvarur", 1, true, "Online", "Ramesh Prabu", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "8224@", "9245329720", "8224", true, 2 },
                    { 8, "516873296730", "NEEDAMANGALAM", "Thiruvarur", 1, true, "Online", "GANESH R", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "8219@", "8760849391", "8219", true, 2 },
                    { 9, "924108073256", "KODAVASAL", "Thiruvarur", 1, true, "Online", "SHANMUGAPRIYA P", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "11274@", "9688925880", "11274", true, 2 },
                    { 10, "568932655898", "MANNARGUDI", "Thiruvarur", 1, true, "Online", "KUMARAN GANAPATHI", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "11272@", "8870207988", "11272", true, 2 },
                    { 11, "828526901867", "Mannargudi", "Thiruvarur", 1, true, "Online", "Sumathi", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "8222@", "9042626755", "8222", true, 2 },
                    { 12, "984494354407", "MUTHUPET", "Thiruvarur", 1, true, "Online", "KaniMozhi", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "8219@", "9003330783", "8219", true, 2 },
                    { 13, "700887772985", "Mannargudi", "Thiruvarur", 1, true, "Online", "ATCHIYA MURUGANANDHAM", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "11179@", "9751861115", "11179", true, 2 },
                    { 14, "885383877726", "Thiruvarur", "Thiruvarur", 1, true, "Online", "SATHEESH THAMBIYAN", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "40362@", "9543642406", "40362", true, 2 },
                    { 15, "283019815916", "THIRUTHURAIPOONDI", "Thiruvarur", 1, true, "Online", "Govindaraj", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "52012@", "9751867179", "52012", true, 2 },
                    { 16, "712083934553", "NEEDAMANGALAM", "Thiruvarur", 1, true, "Online", "SUBISH SUGUMARAN", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "11176@", "8270741713", "11176", true, 2 },
                    { 17, "256646693269", "NAGOOR", "NAGAPATTINAM", 2, true, "Online", "kaviyarasan", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "40361@", "8072537753", "40361", true, 2 },
                    { 18, "544354172245", "KEELAIYUR", "NAGAPATTINAM", 2, true, "Online", "Mathiyarasi Vengadachalam", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "11276@", "9943092275", "11276", true, 2 },
                    { 19, "672146031183", "KEEVALUR", "NAGAPATTINAM", 2, true, "Online", "saranya", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "40360@", "7339328559", "40360", true, 2 },
                    { 20, "245634561191", "NAGAPATTINAM", "NAGAPATTINAM", 2, true, "Online", "yuvashri", new DateTime(2026, 2, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "40359@", "8056064961", "40359", true, 2 }
                });

            migrationBuilder.InsertData(
                table: "UserDistrict",
                columns: new[] { "DistrictId", "UserId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 1, 2 },
                    { 2, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entrollment_DistrictId",
                table: "Entrollment",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrollment_MemberId",
                table: "Entrollment",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrollment_UserId",
                table: "Entrollment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Member_DistrictId",
                table: "Member",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Member_UserId",
                table: "Member",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDistrict_DistrictId",
                table: "UserDistrict",
                column: "DistrictId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entrollment");

            migrationBuilder.DropTable(
                name: "UserDistrict");

            migrationBuilder.DropTable(
                name: "Member");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
