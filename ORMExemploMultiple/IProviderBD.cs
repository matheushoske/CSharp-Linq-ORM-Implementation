using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    internal interface IProviderBD : IDisposable
    {
        string GetQueryText();
        
    }
}
