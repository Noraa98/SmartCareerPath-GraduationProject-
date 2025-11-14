using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.DomainEvents
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }

   
}
