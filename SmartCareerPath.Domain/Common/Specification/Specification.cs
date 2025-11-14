using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.Specification
{
    public abstract class Specification<T> : ISpecification<T>
    {
        public abstract System.Linq.Expressions.Expression<Func<T, bool>> ToExpression();

        public bool IsSatisfiedBy(T entity)
        {
            var predicate = ToExpression().Compile();
            return predicate(entity);
        }

        public Specification<T> And(Specification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        public Specification<T> Or(Specification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }

        public Specification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }

    
}
