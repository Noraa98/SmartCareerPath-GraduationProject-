using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.Specification
{
    public class OrSpecification<T> : Specification<T>
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public OrSpecification(Specification<T> left, Specification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = _left.ToExpression();
            var rightExpression = _right.ToExpression();

            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T));
            var combined = System.Linq.Expressions.Expression.OrElse(
                System.Linq.Expressions.Expression.Invoke(leftExpression, parameter),
                System.Linq.Expressions.Expression.Invoke(rightExpression, parameter)
            );

            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(combined, parameter);
        }
    }

}
