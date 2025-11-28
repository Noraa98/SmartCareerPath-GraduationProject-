using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.Payment
{
    public class DailyRevenueItem
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int TransactionCount { get; set; }
    }
}
