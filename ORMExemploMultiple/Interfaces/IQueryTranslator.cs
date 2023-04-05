using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple.Interfaces
{
    internal interface IQueryTranslator
    {
        QueryInfo Translate(Expression[] expressions);//alterado
    }
}
