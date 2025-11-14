using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.User
{
    public class UserInterestDto
    {
        public int InterestId { get; set; }
        public string Name { get; set; }
        public int? MatchPercentage { get; set; }
    }
}
