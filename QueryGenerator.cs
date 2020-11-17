using System;
using System.Linq.Expressions;

namespace ODataGenerator
{
    public class QueryGenerator<T>
    {
        public string Generate(Expression<Func<T,object>> expression)
        {
            string result = String.Empty;

            return result;
        }
    }
}
