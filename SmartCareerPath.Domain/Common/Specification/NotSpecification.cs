using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.Specification
{
    public class NotSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _specification;

        public NotSpecification(Specification<T> specification)
        {
            _specification = specification;
        }

        public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
        {
            var expression = _specification.ToExpression();
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T));
            var negated = System.Linq.Expressions.Expression.Not(
                System.Linq.Expressions.Expression.Invoke(expression, parameter)
            );

            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(negated, parameter);
        }
    }
}
