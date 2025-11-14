using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Enums
{
    public enum JobApplicationStatus
    {
        Applied = 1,
        UnderReview = 2,
        Shortlisted = 3,
        InterviewScheduled = 4,
        Interviewed = 5,
        Offered = 6,
        Accepted = 7,
        Rejected = 8,
        Withdrawn = 9
    }
}
