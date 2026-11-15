
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasPay.Finance.Infrastructure.Persistence.FunctionMigrations;

public partial class AddReconcileLedgerFunction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE SCHEMA IF NOT EXISTS finance;

            CREATE OR REPLACE FUNCTION finance.reconcile_ledger()
            RETURNS TABLE
            (
                "LedgerAccountId" uuid,
                "Code" varchar(64),
                "AccountType" text,
                "Currency" text,
                "ExpectedBalance" numeric,
                "StoredBalance" numeric,
                "Difference" numeric,
                "IsMatch" boolean
            )
            LANGUAGE sql
            STABLE
            AS $function$
                WITH posting_totals AS
                (
                    SELECT
                        p."LedgerAccountId",
                        SUM(
                            CASE
                                WHEN p."Direction" = 'Debit'
                                    THEN p.amount
                                WHEN p."Direction" = 'Credit'
                                    THEN -p.amount
                                ELSE 0
                            END
                        ) AS net_debit
                    FROM ledger_postings p
                    INNER JOIN ledger_transactions t
                        ON t."Id" = p."LedgerTransactionId"
                    WHERE t."IsPosted" = TRUE
                    GROUP BY p."LedgerAccountId"
                ),
                expected_balances AS
                (
                    SELECT
                        la."Id" AS ledger_account_id,
                        la."Code" AS code,
                        la."Type"::text AS account_type,
                        la."Currency"::text AS currency,
                        CASE
                            WHEN la."Type" IN ('Asset', 'Expense')
                                THEN COALESCE(pt.net_debit, 0)
                            ELSE -COALESCE(pt.net_debit, 0)
                        END AS expected_balance
                    FROM ledger_accounts la
                    LEFT JOIN posting_totals pt
                        ON pt."LedgerAccountId" = la."Id"
                )
                SELECT
                    e.ledger_account_id,
                    e.code,
                    e.account_type,
                    e.currency,
                    e.expected_balance,
                    ab.balance,
                    ab.balance - e.expected_balance,
                    (
                        ab."LedgerAccountId" IS NOT NULL
                        AND ab.currency = e.currency
                        AND ab.balance = e.expected_balance
                    )
                FROM expected_balances e
                LEFT JOIN account_balances ab
                    ON ab."LedgerAccountId" = e.ledger_account_id
                ORDER BY e.ledger_account_id;
            $function$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP FUNCTION IF EXISTS finance.reconcile_ledger();
            """);
    }
}
