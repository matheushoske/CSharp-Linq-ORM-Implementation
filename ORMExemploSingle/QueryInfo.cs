using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    internal class QueryInfo
    {
        public string QueryText { get; internal set; }
        public List<KeyValuePair<string, object>> QueryParameters { get; internal set; }
        public MetaTableBD SourceMetadata { get; internal set; }
        public bool UseDefault { get; internal set; }
        public LambdaExpression LambdaExpression { get; internal set; }
        public ResultShape ResultShape { get; internal set; }
    }
}
