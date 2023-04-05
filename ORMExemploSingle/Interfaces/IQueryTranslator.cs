using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle.Interfaces
{
    internal interface IQueryTranslator
    {
        QueryInfo Translate(Expression expression);
    }
}
