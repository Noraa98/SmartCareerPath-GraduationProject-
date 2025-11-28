using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class PaymentStatisticsResponse
    {
        public int TotalTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public required string Currency { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int FailedCount { get; set; }
        public int RefundedCount { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public Dictionary<string, int> TransactionsByProvider { get; set; } = new();
        public Dictionary<string, int> TransactionsByProduct { get; set; } = new();
        public List<DailyRevenueItem> DailyRevenue { get; set; } = new();
    }
}
