using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasPay.Finance.Infrastructure.Persistence.FunctionMigrations
{
    /// <inheritdoc />
    public partial class AddLedgerIntegrityFunction : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        CREATE OR REPLACE FUNCTION finance.check_unbalanced_transactions()
        RETURNS TABLE
        (
            "LedgerTransactionId" uuid,
            "Currency" text,
            "TotalDebit" numeric,
            "TotalCredit" numeric,
            "Difference" numeric
        )
        LANGUAGE sql
        STABLE
        AS $function$
            SELECT
                t."Id",
                p.currency::text,
                COALESCE(SUM(p.amount) FILTER (
                    WHERE p."Direction" = 'Debit'
                ), 0),
                COALESCE(SUM(p.amount) FILTER (
                    WHERE p."Direction" = 'Credit'
                ), 0),
                COALESCE(SUM(p.amount) FILTER (
                    WHERE p."Direction" = 'Debit'
                ), 0)
                -
                COALESCE(SUM(p.amount) FILTER (
                    WHERE p."Direction" = 'Credit'
                ), 0)
            FROM ledger_transactions t
            INNER JOIN ledger_postings p
                ON p."LedgerTransactionId" = t."Id"
            WHERE t."IsPosted" = TRUE
            GROUP BY t."Id", p.currency
            HAVING
                COALESCE(SUM(p.amount) FILTER (
                    WHERE p."Direction" = 'Debit'
                ), 0)
                <>
                COALESCE(SUM(p.amount) FILTER (
                    WHERE p."Direction" = 'Credit'
                ), 0);
        $function$;
        """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
        DROP FUNCTION IF EXISTS finance.check_unbalanced_transactions();
        """);
        }

    }
}
