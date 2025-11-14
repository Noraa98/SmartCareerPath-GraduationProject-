using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.DomainEvents
{
    public class UserRegisteredEvent : DomainEvent
    {
        public int UserId { get; }
        public string Email { get; }

        public UserRegisteredEvent(int userId, string email)
        {
            UserId = userId;
            Email = email;
        }
    }

}
