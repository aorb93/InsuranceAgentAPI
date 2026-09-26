using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAgentAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangePolicyCatalogColumnsToIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Agregar las nuevas columnas como opcionales (NULL)
            migrationBuilder.AddColumn<int>(
                name: "PolicyTypeId",
                table: "Policies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentFrequencyId",
                table: "Policies",
                type: "int",
                nullable: true);

            // 2. Copiar los IDs desde las tablas en singular (PolicyType y PaymentFrequency)
            migrationBuilder.Sql(@"
        UPDATE p
        SET p.PolicyTypeId = pt.Id
        FROM Policies p
        INNER JOIN PolicyType pt ON LOWER(TRIM(p.PolicyType)) = LOWER(TRIM(pt.Name));

        UPDATE p
        SET p.PaymentFrequencyId = pf.Id
        FROM Policies p
        INNER JOIN PaymentFrequency pf ON LOWER(TRIM(p.PaymentFrequency)) = LOWER(TRIM(pf.Name));

        -- Si alguna póliza tenía un texto que no hizo match, asigna un ID por defecto (1)
        UPDATE Policies SET PolicyTypeId = 1 WHERE PolicyTypeId IS NULL;
        UPDATE Policies SET PaymentFrequencyId = 1 WHERE PaymentFrequencyId IS NULL;
    ");

            // 3. Cambiar las columnas a NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "PolicyTypeId",
                table: "Policies",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentFrequencyId",
                table: "Policies",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // 4. Eliminar las columnas viejas de texto
            migrationBuilder.DropColumn(
                name: "PolicyType",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "PaymentFrequency",
                table: "Policies");

            // 5. Crear los índices y llaves foráneas apuntando a las tablas en singular
            migrationBuilder.CreateIndex(
                name: "IX_Policies_PolicyTypeId",
                table: "Policies",
                column: "PolicyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PaymentFrequencyId",
                table: "Policies",
                column: "PaymentFrequencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_PolicyType_PolicyTypeId",
                table: "Policies",
                column: "PolicyTypeId",
                principalTable: "PolicyType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_PaymentFrequency_PaymentFrequencyId",
                table: "Policies",
                column: "PaymentFrequencyId",
                principalTable: "PaymentFrequency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lógica para revertir en caso de rollback
            migrationBuilder.DropForeignKey(name: "FK_Policies_PolicyType_PolicyTypeId", table: "Policies");
            migrationBuilder.DropForeignKey(name: "FK_Policies_PaymentFrequency_PaymentFrequencyId", table: "Policies");

            migrationBuilder.DropIndex(name: "IX_Policies_PolicyTypeId", table: "Policies");
            migrationBuilder.DropIndex(name: "IX_Policies_PaymentFrequencyId", table: "Policies");

            migrationBuilder.AddColumn<string>(name: "PolicyType", table: "Policies", type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "PaymentFrequency", table: "Policies", type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "");

            migrationBuilder.DropColumn(name: "PolicyTypeId", table: "Policies");
            migrationBuilder.DropColumn(name: "PaymentFrequencyId", table: "Policies");
        }
    }
}
