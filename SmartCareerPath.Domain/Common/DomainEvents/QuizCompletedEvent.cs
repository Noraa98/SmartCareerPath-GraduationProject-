using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.DomainEvents
{
    public class QuizCompletedEvent : DomainEvent
    {
        public int UserId { get; }
        public int QuizId { get; }
        public int Score { get; }

        public QuizCompletedEvent(int userId, int quizId, int score)
        {
            UserId = userId;
            QuizId = quizId;
            Score = score;
        }
    }
}
