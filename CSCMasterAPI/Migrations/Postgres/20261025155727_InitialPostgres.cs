using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CSCMasterAPI.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:case_insensitive", "und-u-ks-level2,und-u-ks-level2,icu,False");

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyBranch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBranch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_District", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    MRP = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Address = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    ContactInfoJSon = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_CompanyBranch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "CompanyBranch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customer_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Phone = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Aadhaar = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    LoginAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_CompanyBranch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "CompanyBranch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Member",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    District = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Block = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Name = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Phone = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    StationId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Aadhaar = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Mode = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    OnboardDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    LoginAllowed = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "PettyCashAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    TotalLineAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalLineDiscount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    BillDiscount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    BillCharges = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    PaymentJSon = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    BillAdustmentsJSon = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CashAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UPIAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PettyCashAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PettyCashAccount_CompanyBranch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "CompanyBranch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PettyCashAccount_Company_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Company",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PettyCashAccount_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDistrict",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UploadedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    StationId = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    District = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Block = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    EID = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    EIDDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ChildName = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    Type = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive"),
                    Uploaded = table.Column<bool>(type: "boolean", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    OperatorAadhaar = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    Amount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    GST = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CC = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ProcessingStateDescription = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    RejectReasonDescription = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    PacketSkipped = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive"),
                    ForeignResident = table.Column<string>(type: "text", nullable: true, collation: "case_insensitive")
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

            migrationBuilder.CreateTable(
                name: "PettyCashAccountItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    MRP = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PettyCashAccountItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PettyCashAccountItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PettyCashAccountItem_PettyCashAccount_AccountId",
                        column: x => x.AccountId,
                        principalTable: "PettyCashAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Company",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Retinue" },
                    { 2, "FriendsCSC" }
                });

            migrationBuilder.InsertData(
                table: "CompanyBranch",
                columns: new[] { "Id", "CompanyId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Main" },
                    { 2, 2, "FriendsCSC Mannai" }
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
                table: "Customer",
                columns: new[] { "Id", "Address", "BranchId", "CompanyId", "ContactInfoJSon", "Name" },
                values: new object[] { 1, "UNKNOWN", 2, 2, "[{\"Type\":1,\"Number\":\"9999999999\"}]", "UNKNOWN" });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Aadhaar", "BranchId", "CompanyId", "LoginAllowed", "Name", "Password", "Phone", "Status" },
                values: new object[,]
                {
                    { 1, "123456789012", 1, 1, true, "Retinue", "Red@123", "9943135008", true },
                    { 2, "123456789013", 2, 2, true, "Karthi", "Karthi123", "9444488760", true },
                    { 3, "123456789013", 2, 2, true, "Frds MAU1", "0228@", "9444490228", true }
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
                name: "IX_Company_Name",
                table: "Company",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranch_Name",
                table: "CompanyBranch",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_BranchId",
                table: "Customer",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CompanyId",
                table: "Customer",
                column: "CompanyId");

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
                name: "IX_Item_Name",
                table: "Item",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Member_DistrictId",
                table: "Member",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Member_UserId",
                table: "Member",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAccount_BranchId",
                table: "PettyCashAccount",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAccount_CompanyId",
                table: "PettyCashAccount",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAccount_UserId",
                table: "PettyCashAccount",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAccountItem_AccountId",
                table: "PettyCashAccountItem",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashAccountItem_ItemId",
                table: "PettyCashAccountItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_User_BranchId",
                table: "User",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_User_CompanyId",
                table: "User",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDistrict_DistrictId",
                table: "UserDistrict",
                column: "DistrictId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "Entrollment");

            migrationBuilder.DropTable(
                name: "PettyCashAccountItem");

            migrationBuilder.DropTable(
                name: "UserDistrict");

            migrationBuilder.DropTable(
                name: "Member");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "PettyCashAccount");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "CompanyBranch");

            migrationBuilder.DropTable(
                name: "Company");
        }
    }
}
