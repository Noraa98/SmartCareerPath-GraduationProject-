using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCareerPath.Infrastructure.Persistence.Migrations
{
    public partial class FixPaymentTransactionProviderColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProviderTemp",
                table: "PaymentTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE PaymentTransactions
                SET ProviderTemp = CASE
                    WHEN LOWER(Provider) IN ('stripe') THEN 1
                    WHEN LOWER(Provider) IN ('paypal') THEN 2
                    WHEN LOWER(Provider) IN ('paymob') THEN 3
                    ELSE TRY_CAST(Provider AS INT)
                END
            ");

            migrationBuilder.Sql("UPDATE PaymentTransactions SET ProviderTemp = 1 WHERE ProviderTemp IS NULL");

            migrationBuilder.DropColumn(name: "Provider", table: "PaymentTransactions");

            migrationBuilder.AddColumn<int>(
                name: "Provider",
                table: "PaymentTransactions",
                type: "int",
                nullable: false,
                defaultValue: 1,
                comment: "Payment provider: 1=Stripe, 2=PayPal, 3=Paymob");

            migrationBuilder.Sql("UPDATE PaymentTransactions SET Provider = ProviderTemp");

            migrationBuilder.DropColumn(name: "ProviderTemp", table: "PaymentTransactions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderTemp",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE PaymentTransactions
                SET ProviderTemp = CASE
                    WHEN Provider = 1 THEN 'Stripe'
                    WHEN Provider = 2 THEN 'PayPal'
                    WHEN Provider = 3 THEN 'Paymob'
                    ELSE CAST(Provider AS nvarchar(50))
                END
            ");

            migrationBuilder.DropColumn(name: "Provider", table: "PaymentTransactions");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Stripe");

            migrationBuilder.Sql("UPDATE PaymentTransactions SET Provider = ProviderTemp");

            migrationBuilder.DropColumn(name: "ProviderTemp", table: "PaymentTransactions");
        }
    }
}
