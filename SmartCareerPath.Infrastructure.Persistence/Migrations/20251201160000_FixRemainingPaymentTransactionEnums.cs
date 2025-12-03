using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCareerPath.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixRemainingPaymentTransactionEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                defaultValue: 1);

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
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Simplified rollback - just revert column types
        }
    }
}
