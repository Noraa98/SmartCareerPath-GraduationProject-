using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCareerPath.Infrastructure.Persistence.Migrations
{
    [Migration("20251201173000_FixPaymentTransactionStatusColumn")]
    public partial class FixPaymentTransactionStatusColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add a temporary int column
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.PaymentTransactions', 'StatusInt') IS NULL
                BEGIN
                    ALTER TABLE dbo.PaymentTransactions ADD StatusInt INT NULL;
                END
            ");

            // 2) Populate the int column using numeric conversion when possible,
            //    otherwise map common textual values to the enum integer values.
            migrationBuilder.Sql(@"
                UPDATE dbo.PaymentTransactions
                SET StatusInt = CASE
                    WHEN TRY_CAST(Status AS INT) IS NOT NULL THEN TRY_CAST(Status AS INT)
                    WHEN LOWER(Status) IN ('unknown') THEN 0
                    WHEN LOWER(Status) IN ('pending') THEN 1
                    WHEN LOWER(Status) IN ('processing') THEN 2
                    WHEN LOWER(Status) IN ('completed') THEN 3
                    WHEN LOWER(Status) IN ('failed') THEN 4
                    WHEN LOWER(Status) IN ('refunded') THEN 5
                    WHEN LOWER(Status) IN ('cancelled') THEN 6
                    WHEN LOWER(Status) IN ('verifying') THEN 7
                    WHEN LOWER(Status) IN ('expired') THEN 8
                    ELSE 0
                END
            ");

            // 3) If any rows still have NULL, set to Pending (1)
            migrationBuilder.Sql(@"
                UPDATE dbo.PaymentTransactions SET StatusInt = 1 WHERE StatusInt IS NULL;
            ");

            // 4) Add a new int column 'StatusNew' with NOT NULL and default 1, copy values, drop old string column, rename new
            migrationBuilder.Sql(@"
                ALTER TABLE dbo.PaymentTransactions ADD StatusNew INT NOT NULL CONSTRAINT DF_PaymentTransactions_Status DEFAULT 1;
                UPDATE dbo.PaymentTransactions SET StatusNew = StatusInt;
                ALTER TABLE dbo.PaymentTransactions DROP COLUMN Status;
                EXEC sp_rename 'dbo.PaymentTransactions.StatusNew', 'Status', 'COLUMN';
                ALTER TABLE dbo.PaymentTransactions DROP CONSTRAINT DF_PaymentTransactions_Status;
            ");

            // 5) Cleanup temporary column
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.PaymentTransactions', 'StatusInt') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.PaymentTransactions DROP COLUMN StatusInt;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Try to revert: create StatusStr, map ints back to names, drop int column, rename
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.PaymentTransactions', 'StatusStr') IS NULL
                BEGIN
                    ALTER TABLE dbo.PaymentTransactions ADD StatusStr NVARCHAR(50) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                UPDATE dbo.PaymentTransactions
                SET StatusStr = CASE Status
                    WHEN 0 THEN 'Unknown'
                    WHEN 1 THEN 'Pending'
                    WHEN 2 THEN 'Processing'
                    WHEN 3 THEN 'Completed'
                    WHEN 4 THEN 'Failed'
                    WHEN 5 THEN 'Refunded'
                    WHEN 6 THEN 'Cancelled'
                    WHEN 7 THEN 'Verifying'
                    WHEN 8 THEN 'Expired'
                    ELSE 'Unknown'
                END
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE dbo.PaymentTransactions DROP COLUMN Status;
                EXEC sp_rename 'dbo.PaymentTransactions.StatusStr', 'Status', 'COLUMN';
            ");
        }
    }
}
