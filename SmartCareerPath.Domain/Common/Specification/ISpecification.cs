using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.Specification
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T entity);
        System.Linq.Expressions.Expression<Func<T, bool>> ToExpression();
    }
}
