using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasPay.Finance.Infrastructure.Persistence.FunctionMigrations;

[DbContext(typeof(FinanceDbContext))]
[Migration("20261121000000_AddWithdrawalRequestIdUniqueIndex")]
public sealed class AddWithdrawalRequestIdUniqueIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_withdrawals_RequestId",
            table: "withdrawals",
            column: "RequestId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_withdrawals_RequestId",
            table: "withdrawals");
    }
}
