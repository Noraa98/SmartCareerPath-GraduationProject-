using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.User
{
    public class UserSkillDto
    {
        public int SkillId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int ProficiencyLevel { get; set; }
        public string Source { get; set; }
        public bool IsVerified { get; set; }
    }
}
