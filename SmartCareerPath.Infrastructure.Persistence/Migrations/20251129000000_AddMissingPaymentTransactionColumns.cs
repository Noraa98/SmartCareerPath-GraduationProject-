using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCareerPath.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingPaymentTransactionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to PaymentTransactions table
            migrationBuilder.AddColumn<int>(
                name: "BillingCycle",
                table: "PaymentTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "PaymentTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountCode",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "PaymentTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureCode",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastVerifiedAt",
                table: "PaymentTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalAmount",
                table: "PaymentTransactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderMetadata",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptUrl",
                table: "PaymentTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundReason",
                table: "PaymentTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundReference",
                table: "PaymentTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "PaymentTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "PaymentTransactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebhookPayload",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingCycle",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "DiscountCode",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "FailureCode",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "LastVerifiedAt",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "OriginalAmount",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ProviderMetadata",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ReceiptUrl",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RefundReason",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RefundReference",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "WebhookPayload",
                table: "PaymentTransactions");
        }
    }
}
