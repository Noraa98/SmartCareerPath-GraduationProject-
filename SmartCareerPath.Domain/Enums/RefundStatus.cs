using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Enums
{
    public enum RefundStatus
    {
        Unknown = 0,
        Requested = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        Processing = 5,
        Completed = 6,
        Failed = 7
    }
}
