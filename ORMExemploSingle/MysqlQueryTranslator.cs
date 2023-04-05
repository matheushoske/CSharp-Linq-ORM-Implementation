using ORMExemploSingle.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploSingle
{
    internal class MysqlQueryTranslator : ExpressionVisitor, IQueryTranslator
    {
        private readonly MetaModelBD _model;
        private ProjectionTranslator _selectTranslator;
        public MysqlQueryTranslator(MetaModelBD model)
        {
            _model = model;
        }
        protected override Expression VisitMethodCall(MethodCallExpression mc)
        {
            Type declaringType = mc.Method.DeclaringType;
            if (declaringType != typeof(Queryable))
                throw new NotSupportedException(
                  "The type for the query operator is not Queryable!");
            switch (mc.Method.Name)
            {
                case "Where":
                    // valida se realmente é uma expressão where
                    var whereLambda = GetLambdaWithParamCheck(mc);
                    if (whereLambda == null)
                        break;
                    VisitWhere(mc.Arguments[0], whereLambda);
                    break;
                default:
                    return base.VisitMethodCall(mc);
            }
            Visit(mc.Arguments[0]);
            return mc;
        }

        protected virtual LambdaExpression GetLambdaWithParamCheck(MethodCallExpression mc)
        {
            if (mc == null) throw new ArgumentNullException(nameof(mc));
            var unary = mc.Arguments.LastOrDefault() as UnaryExpression;
            var lambda = unary.Operand as LambdaExpression;
            if (lambda == null) throw new ArgumentException("The method call expression does not have a lambda expression as its last argument.", nameof(mc));
            return lambda;
        }
        private WhereTranslator _whereTranslator;
        private void VisitWhere(Expression queryable, LambdaExpression predicate)
        {
            // esse exemplo de provider não aceita mais de um
            // operador Where na Linq expression
            if (_whereTranslator != null)
                throw new NotSupportedException(
                   "Não é possível ter mais que um operador Where nessa expression");
            _whereTranslator = new WhereTranslator(_model);
            _whereTranslator.Translate(predicate);
        }
        





        private QueryInfo ConvertToExecutableQuery(Expression query)
        {
            // Encontra a tabela usada na expression
            MetaTableBD source;
            if (!GetSourceTable(query, out source))
                throw new NotSupportedException("Esta expression de consulta não é suportada!");
            this.Visit(query);
            StringBuilder sb = new StringBuilder();
            bool useDefault = false;
            // SELECT
            sb.Append("SELECT ");
            
            // PROJECTION
            if (_selectTranslator == null || !_selectTranslator.DataMembers.Any())
            {
                // projeta as colunas mapeadas 
                _selectTranslator = new ProjectionTranslator(_model, source.PersistentDataMembers);
            }
            if (!_selectTranslator.DataMembers.Any())
                throw new Exception("Não existem itens para projeção nesta consulta!");
            sb.Append(_selectTranslator.ProjectionClause);
            // FROM
            sb.Append(" FROM ");
            sb.AppendLine(source.TableName);
            // WHERE
            if (_whereTranslator != null)
            {
                string where = _whereTranslator.WhereClause;
                if (!string.IsNullOrEmpty(where))
                {
                    sb.AppendLine(where);
                }
            }
            return new QueryInfo
            {
                QueryText = sb.ToString(), // Query completa montada
                QueryParameters = _whereTranslator.WhereParameters,// Parametros da query mapeados para evitar sql injection
                SourceMetadata = source,
                LambdaExpression = _selectTranslator.ProjectionLambda,
                ResultShape = GetResultShape(query),
                UseDefault = useDefault // Utilizaremos quando os operadores contendo OrDefault forem aplicados - ex: FirstOrDefault
            };
        }

        private bool GetSourceTable(Expression query, out MetaTableBD source)
        {
            MethodCallExpression method = query as MethodCallExpression;
            var a = method.Arguments.First() as ConstantExpression;
            Type type = a.Value.GetType().GetGenericArguments()[0];
            source = _model.GetTable(type);
            return source != null;

        }

        private ResultShape GetResultShape(Expression query)
        {
            LambdaExpression lambda = query as LambdaExpression;
            if (lambda != null)
                query = lambda.Body;
            if (query.Type == typeof(void))
                return ResultShape.None;
            if (query.Type == typeof(IMultipleResults))
                throw new NotSupportedException("Multiple result shape is not supported");
            MethodCallExpression methodExp = query as MethodCallExpression;
            if (methodExp != null && ((methodExp.Method.DeclaringType == typeof(Queryable)) ||
                (methodExp.Method.DeclaringType == typeof(Enumerable))))
            {
                string str = methodExp.Method.Name;
                if (str != null && (str == "First" || str == "FirstOrDefault" || str == "Single" || str == "SingleOrDefault"))
                    return ResultShape.Singleton;
            }
            return ResultShape.Sequence;
        }

        public QueryInfo Translate(Expression query)
        {
            return ConvertToExecutableQuery(query);
        }
        class ProjectionTranslator
        {
            public List<MetaDataMemberBD> DataMembers { get; set; }
            private MetaModelBD _model { get; set; }
            private readonly StringBuilder _sb;
            public ProjectionTranslator(MetaModelBD model, List<MetaDataMemberBD> persistentDataMembers)
            {
                DataMembers = persistentDataMembers;
                _model = model;
                _sb = new StringBuilder();
                this.Translate();
            }
            private void Translate() 
            {
                foreach (var item in DataMembers)
                {
                    _sb.Append(item.MappedName);
                    if(item.Name != DataMembers.Last().Name)
                        _sb.Append(',');
                }
                ProjectionClause = _sb.ToString();
            }
            public LambdaExpression ProjectionLambda { get; internal set; }
            public string ProjectionClause { get; internal set; }
        }
        class WhereTranslator : ExpressionVisitor
        {
            private readonly StringBuilder _sb;
            private readonly MetaModelBD _model;

            public WhereTranslator(MetaModelBD model)
            {
                _sb = new StringBuilder();
                WhereParameters = new List<KeyValuePair<string, object>>();
                _model = model;
            }

            public string WhereClause
            {
                get { return _sb.ToString(); }
            }

            // Utilizei como tipo o List<KeyValuePair<string,object>> ao invés de um Dictionary para
            // facilitar as inserções em keys e utilização por index
            public List<KeyValuePair<string,object>> WhereParameters { get; private set; }
            public void Translate(LambdaExpression predicate)
            {
                _sb.Append("WHERE ");
                Visit(predicate.Body);//chama o visitBinary
            }

            protected override Expression VisitBinary(BinaryExpression b)
            {
                _sb.Append("(");
                Visit(b.Left);

                switch (b.NodeType)
                {
                    case ExpressionType.Equal:
                        if (IsComparingWithNull(b))
                            _sb.Append(" IS ");
                        else
                            _sb.Append(" = ");
                        break;
                    case ExpressionType.NotEqual:
                        if (IsComparingWithNull(b))
                            _sb.Append(" IS NOT ");
                        else
                            _sb.Append(" <> ");
                        break;
                    case ExpressionType.GreaterThan:
                        _sb.Append(" > ");
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        _sb.Append(" >= ");
                        break;
                    case ExpressionType.LessThan:
                        _sb.Append(" < ");
                        break;
                    case ExpressionType.LessThanOrEqual:
                        _sb.Append(" <= ");
                        break;
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        _sb.Append(" AND ");
                        break;
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        _sb.Append(" OR ");
                        break;
                    default:
                        throw new NotSupportedException($"O operador binário '{b.NodeType}' tem suporte para uma cláusula Where.");
                }

                Visit(b.Right);
                _sb.Append(")");

                return b;
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {
                AppendValue(c.Value);
                return c;
            }
            private void AppendValue(object value) 
            {
                if (value == null)
                    _sb.Append("NULL");
                else 
                { 
                    // aqui poderiamos basicamente utilizar o
                    // _sb.Append($"'{c.Value}'")
                    // porém por questões de segurança,
                    // principalmente para evitar possiveis SQLInjections,
                    // vincularemos o valor encontrado à ultíma key inserida
                    // da nossa lista de parâmetros, que 
                    // posteriormente serão aplicados ao nosso comando
                    AddValueToLastParameter(value);
                }
            }
            protected override Expression VisitMember(MemberExpression m)
            {
                var nodeType = m.Expression.NodeType; //Parameter //Constant 
                //Existe aqui a possibilidade de recebermos uma cosntante em formato 
                //de MemberExpression
                if (nodeType == ExpressionType.Parameter)
                {
                    var column = _model.GetColumnName(m.Member);
                    _sb.Append(column);
                    //criando uma nova key com o formato de '@coluna' para ser utilizada como parâmetro
                    CreateParameter("@" + column);
                }
                else 
                {
                    var value = GetMemberValue(m);
                    AppendValue(value);
                }
                return m;
            }
            void AddValueToLastParameter(object value) 
            {
                var index = WhereParameters.Count - 1;
                var parameter = WhereParameters[index].Key;
                WhereParameters[index] = new KeyValuePair<string, object>(parameter, value);
                _sb.Append(parameter);
            }
            void CreateParameter(string parameterName) 
            {
                var repeated = WhereParameters.Where(x => x.Key == parameterName);
                if (repeated.Any()) parameterName = parameterName + repeated.Count();
                WhereParameters.Add(new KeyValuePair<string, object>(parameterName, null));
            }
            private object GetMemberValue(MemberExpression member)
            {
                var objectMember = Expression.Convert(member, typeof(object));

                var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                var getter = getterLambda.Compile();

                return getter();
            }
            private bool IsComparingWithNull(BinaryExpression b)
            {
                return (IsNullConstant(b.Left) && IsNullConstant(b.Right));
            }

            private bool IsNullConstant(Expression exp)
            {
                return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
            }
        }
    }
}
