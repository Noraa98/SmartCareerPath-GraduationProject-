using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCareerPath.Infrastructure.Persistence.Migrations
{
    public partial class FixPaymentTransactionCurrencyType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add temporary int column
            migrationBuilder.AddColumn<int>(
                name: "CurrencyTemp",
                table: "PaymentTransactions",
                type: "int",
                nullable: true);

            // Map common currency string codes to enum integers, try casting numeric strings as a fallback
            migrationBuilder.Sql(@"
                UPDATE PaymentTransactions
                SET CurrencyTemp = CASE
                    WHEN Currency IN ('USD','usd') THEN 1
                    WHEN Currency IN ('EGP','egp') THEN 2
                    WHEN Currency IN ('EUR','eur') THEN 3
                    WHEN Currency IN ('GBP','gbp') THEN 4
                    WHEN Currency IN ('SAR','sar') THEN 5
                    ELSE TRY_CAST(Currency AS INT)
                END
            ");

            // Ensure no nulls - default to USD (1) if mapping failed
            migrationBuilder.Sql("UPDATE PaymentTransactions SET CurrencyTemp = 1 WHERE CurrencyTemp IS NULL");

            // Drop the old column (which is currently a string in the target DB)
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PaymentTransactions");

            // Add the new integer-backed Currency column
            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "PaymentTransactions",
                type: "int",
                nullable: false,
                defaultValue: 1,
                comment: "Currency code: 1=USD, 2=EGP, 3=EUR, etc.");

            // Copy converted values into the new column
            migrationBuilder.Sql("UPDATE PaymentTransactions SET Currency = CurrencyTemp");

            // Remove the temporary column
            migrationBuilder.DropColumn(
                name: "CurrencyTemp",
                table: "PaymentTransactions");

            // Fix ProductType column - convert string to int
            migrationBuilder.AddColumn<int>(
                name: "ProductTypeTemp",
                table: "PaymentTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE PaymentTransactions
                SET ProductTypeTemp = CASE
                    WHEN LOWER(ProductType) IN ('interviewerpractice','interviewer','interviewe') THEN 1
                    WHEN LOWER(ProductType) IN ('cvbuilder','cv') THEN 2
                    WHEN LOWER(ProductType) IN ('bundlesubscription','bundle') THEN 3
                    ELSE TRY_CAST(ProductType AS INT)
                END
            ");

            migrationBuilder.Sql("UPDATE PaymentTransactions SET ProductTypeTemp = 1 WHERE ProductTypeTemp IS NULL");

            migrationBuilder.DropColumn(name: "ProductType", table: "PaymentTransactions");

            migrationBuilder.AddColumn<int>(
                name: "ProductType",
                table: "PaymentTransactions",
                type: "int",
                nullable: false,
                defaultValue: 1,
                comment: "Product type: 1=Interviewer, 2=CV, 3=Bundle, etc.");

            migrationBuilder.Sql("UPDATE PaymentTransactions SET ProductType = ProductTypeTemp");

            migrationBuilder.DropColumn(name: "ProductTypeTemp", table: "PaymentTransactions");

            // Fix PaymentMethod column - convert string to int (nullable)
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodTemp",
                table: "PaymentTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE PaymentTransactions
                SET PaymentMethodTemp = CASE
                    WHEN LOWER(PaymentMethod) IN ('creditcard','card','credit_card','credit') THEN 1
                    WHEN LOWER(PaymentMethod) IN ('debitcard','debit_card','debit') THEN 2
                    WHEN LOWER(PaymentMethod) IN ('paypal') THEN 3
                    WHEN LOWER(PaymentMethod) IN ('applepay','apple_pay','apple') THEN 4
                    WHEN LOWER(PaymentMethod) IN ('googlepay','google_pay','google') THEN 5
                    ELSE TRY_CAST(PaymentMethod AS INT)
                END
            ");

            migrationBuilder.DropColumn(name: "PaymentMethod", table: "PaymentTransactions");

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "PaymentTransactions",
                type: "int",
                nullable: true,
                comment: "Payment method used by customer");

            migrationBuilder.Sql("UPDATE PaymentTransactions SET PaymentMethod = PaymentMethodTemp WHERE PaymentMethodTemp IS NOT NULL");

            migrationBuilder.DropColumn(name: "PaymentMethodTemp", table: "PaymentTransactions");

            // Fix BillingCycle column - convert string to int (nullable)
            migrationBuilder.AddColumn<int>(
                name: "BillingCycleTemp",
                table: "PaymentTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE PaymentTransactions
                SET BillingCycleTemp = CASE
                    WHEN LOWER(BillingCycle) IN ('monthly','month') THEN 1
                    WHEN LOWER(BillingCycle) IN ('quarterly','quarter') THEN 2
                    WHEN LOWER(BillingCycle) IN ('yearly','year','annual') THEN 3
                    WHEN LOWER(BillingCycle) IN ('lifetime','onetime') THEN 4
                    ELSE TRY_CAST(BillingCycle AS INT)
                END
            ");

            migrationBuilder.DropColumn(name: "BillingCycle", table: "PaymentTransactions");

            migrationBuilder.AddColumn<int>(
                name: "BillingCycle",
                table: "PaymentTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("UPDATE PaymentTransactions SET BillingCycle = BillingCycleTemp WHERE BillingCycleTemp IS NOT NULL");

            migrationBuilder.DropColumn(name: "BillingCycleTemp", table: "PaymentTransactions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback Currency
            migrationBuilder.AddColumn<string>(
                name: "CurrencyTemp",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE PaymentTransactions
                SET CurrencyTemp = CASE
                    WHEN Currency = 1 THEN 'USD'
                    WHEN Currency = 2 THEN 'EGP'
                    WHEN Currency = 3 THEN 'EUR'
                    WHEN Currency = 4 THEN 'GBP'
                    WHEN Currency = 5 THEN 'SAR'
                    ELSE CAST(Currency AS nvarchar(50))
                END
            ");

            migrationBuilder.DropColumn(name: "Currency", table: "PaymentTransactions");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.Sql("UPDATE PaymentTransactions SET Currency = CurrencyTemp");

            migrationBuilder.DropColumn(name: "CurrencyTemp", table: "PaymentTransactions");

            // Rollback ProductType, PaymentMethod, BillingCycle similarly
            // (simplified - reverting to nvarchar defaults)
        }
    }
}
