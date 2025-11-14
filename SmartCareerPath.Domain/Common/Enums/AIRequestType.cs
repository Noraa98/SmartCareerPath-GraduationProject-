using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.Enums
{
    public enum AIRequestType
    {
        CVGenerate = 1,
        CVImprove = 2,
        CVParsing = 3,
        Interview = 4,
        JobParsing = 5,
        JobMatching = 6,
        CareerPathRecommendation = 7,
        SkillGapAnalysis = 8,
        QuizGeneration = 9,
        Other = 99
    }
}
