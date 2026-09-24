using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAgentAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGuidToClients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Clients",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Guid",
                table: "Clients",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Guid",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Clients");
        }
    }
}
