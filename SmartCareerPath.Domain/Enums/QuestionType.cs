using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Enums
{
    public enum QuestionType
    {
        MCQ = 1,              // Multiple Choice - Single Answer
        MultipleAnswer = 2,   // Multiple Choice - Multiple Answers
        TrueFalse = 3,        // True/False
        CodeOutput = 4,       // What's the output of this code?
        FillInTheBlank = 5,   // Text input
        Essay = 6             // Long text answer
    }
}
