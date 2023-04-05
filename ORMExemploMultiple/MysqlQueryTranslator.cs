using ORMExemploMultiple.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    internal class MysqlQueryTranslator : ExpressionVisitor, IQueryTranslator
    {
        private readonly MetaModelBD _model;
        private TakeTranslator _takeTranslator;
        private OrderByTranslator _orderByTranslator;
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
                    // is this really a proper Where?
                    var whereLambda = GetLambdaWithParamCheck(mc);
                    if (whereLambda == null)
                        break;
                    VisitWhere(mc.Arguments[0], whereLambda);
                    break;
                case "OrderBy"://alterado//criado
                    // is this really a proper OrderBy?
                    var orderByLambda = GetLambdaWithParamCheck(mc);
                    if (orderByLambda == null)
                        break;
                    VisitOrderBy(mc.Arguments[0], orderByLambda, OrderDirection.Ascending);
                    break;
                case "OrderByDescending"://alterado//criado
                    // is this really a proper OrderByDescending?
                    var orderByDescLambda = GetLambdaWithParamCheck(mc);
                    if (orderByDescLambda == null)
                        break;
                    VisitOrderBy(mc.Arguments[0], orderByDescLambda, OrderDirection.Descending);
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
            // this custom provider cannot support more
            // than one Where query operator in a LINQ query
            if (_whereTranslator != null)
                throw new NotSupportedException(
                   "You cannot have more than one Where operator in this expression");
            _whereTranslator = new WhereTranslator(_model);
            _whereTranslator.Translate(predicate);
        }
        private void VisitOrderBy(Expression queryable, LambdaExpression predicate, OrderDirection orderDirection)//criado
        {
            // this custom provider cannot support more
            // than one Where query operator in a LINQ query
            if (_orderByTranslator != null)
                throw new NotSupportedException(
                   "You cannot have more than one OrderBy operator in this expression");
            _orderByTranslator = new OrderByTranslator(_model);
            _orderByTranslator.Translate(predicate, orderDirection);
        }





        private QueryInfo ConvertToExecutableQuery(Expression[] expressions)//alterado
        {
            // Find the query source
            MetaTableBD source;
            if (!GetSourceTable(expressions.First(), out source))//alterado
                throw new NotSupportedException("This query expression is not supported!");
            //this.VisitMethodCall(query as MethodCallExpression);
            foreach (var exp in expressions)
            {
                this.Visit(exp);
            }
            
            StringBuilder sb = new StringBuilder();
            bool useDefault = false;
            // SELECT
            sb.Append("SELECT ");
            
            // PROJECTION
            if (_selectTranslator == null || !_selectTranslator.DataMembers.Any())
            {
                // project on all the mapped columns
                _selectTranslator = new ProjectionTranslator(_model, source.PersistentDataMembers);
            }
            if (!_selectTranslator.DataMembers.Any())
                throw new Exception("There are no items for projection in this query!");
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
            // ORDER BY
            if (_orderByTranslator != null)
            {
                string orderby = _orderByTranslator.OrderByClause;
                if (!string.IsNullOrEmpty(orderby))
                {
                    sb.AppendLine(orderby);
                }
            }
            //// LIMIT
            //if (_takeTranslator != null && _takeTranslator.Count.HasValue)
            //{
            //    useDefault = _takeTranslator.UseDefault;
            //    sb.Append("LIMIT ");
            //    sb.Append(_takeTranslator.Count);
            //    sb.Append(" ");
            //}
            return new QueryInfo
            {
                QueryText = sb.ToString(), // The actual SQL query
                QueryParameters = _whereTranslator.WhereParameters,
                SourceMetadata = source,
                LambdaExpression = _selectTranslator.ProjectionLambda,
                ResultShape = GetResultShape(expressions),
                UseDefault = useDefault
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
        private ResultShape GetResultShape(Expression[] expressions)//criado
        {
            ResultShape[] resultShapes= new ResultShape[expressions.Length];
            for (int i = 0; i < expressions.Length; i++)
                resultShapes[i] = GetResultShape(expressions[i]);

            if(resultShapes.Contains(ResultShape.Sequence)) return ResultShape.Sequence;
            else if (resultShapes.Contains(ResultShape.Singleton)) return ResultShape.Singleton;
            else return ResultShape.None;
        }

        public QueryInfo Translate(Expression[] expressions)//alterado
        {
            return ConvertToExecutableQuery(expressions);
        }

        enum OrderDirection
        {
            Ascending,
            Descending
        }
        class OrderByTranslator : ExpressionVisitor//criado
        {
            StringBuilder _sb;
            MetaModelBD _model;
            public string OrderByClause
            {
                get { return _sb.ToString(); }
            }
            public OrderByTranslator(MetaModelBD model)
            {
                _sb = new StringBuilder();
                _model = model;
            }

            public void Translate(LambdaExpression predicate, OrderDirection orderDirection) 
            {
                _sb.Append("ORDER BY ");
                Visit(predicate.Body);//chama o visitBinary
                switch (orderDirection)
                {
                    case OrderDirection.Ascending:
                        _sb.Append(" ASC");
                        break;
                    case OrderDirection.Descending:
                        _sb.Append(" DESC");
                        break;
                    default:
                        break;
                }
            }
            protected override Expression VisitBinary(BinaryExpression b)
            {
                //_sb.Append("(");
                Visit(b.Left);

                switch (b.NodeType)
                {
                    
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
                    case ExpressionType.AndAlso:
                        _sb.Append(" AND ");
                        break;
                    case ExpressionType.OrElse:
                        _sb.Append(" OR ");
                        break;
                    default:
                        throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported in a Where clause.");
                }

                Visit(b.Right);
                //_sb.Append(")");

                return b;
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {

                if (c.Value != null)
                    _sb.Append($"{c.Value}");
                return c;
            }

            protected override Expression VisitMember(MemberExpression m)
            {
                var nodeType = m.Expression.NodeType; //Parameter //Constant 
                if (nodeType == ExpressionType.Parameter)
                {
                    var column = _model.GetColumnName(m.Member);
                    _sb.Append(column);
                }
                else
                {
                    var value = GetMemberValue(m);
                    _sb.Append(value);
                }
                return m;
            }
            private object GetMemberValue(MemberExpression member)
            {
                var objectMember = Expression.Convert(member, typeof(object));

                var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                var getter = getterLambda.Compile();

                return getter();
            }
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
        class TakeTranslator
        {

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
                    case ExpressionType.AndAlso:
                        _sb.Append(" AND ");
                        break;
                    case ExpressionType.OrElse:
                        _sb.Append(" OR ");
                        break;
                    default:
                        throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported in a Where clause.");
                }

                Visit(b.Right);
                _sb.Append(")");

                return b;
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {

                if (c.Value == null)
                    _sb.Append("NULL");
                else
                    //_sb.Append($"'{c.Value}'");
                    AddValueToLastParameter(c.Value);

                return c;
            }
           
            protected override Expression VisitMember(MemberExpression m)
            {
                var nodeType = m.Expression.NodeType; //Parameter //Constant 
                if (nodeType == ExpressionType.Parameter)
                {
                    var column = _model.GetColumnName(m.Member);
                    _sb.Append(column);
                    CreateParameter("@" + column);
                    
                }
                else 
                {
                    var value = GetMemberValue(m);
                    //_sb.Append(value);
                    AddValueToLastParameter(value);
                }
                return m;
            }
            void AddValueToLastParameter(object value) 
            {
                var parameter = WhereParameters[WhereParameters.Count - 1].Key;
                WhereParameters[WhereParameters.Count - 1] = new KeyValuePair<string, object>(parameter, value);
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
            //StringBuilder _sb;

            //public WhereTranslator(MetaModelBD model)
            //{
            //}

            //internal string WhereClause => _sb.ToString();
            //protected override Expression VisitBinary(BinaryExpression b)
            //{
            //    _sb.Append("(");
            //    Visit(b.Left);
            //    switch (b.NodeType)
            //    {
            //        case ExpressionType.And:
            //        case ExpressionType.AndAlso:
            //            _sb.Append(" AND ");
            //            break;
            //        case ExpressionType.Or:
            //        case ExpressionType.OrElse:
            //            _sb.Append(" OR ");
            //            break;
            //        case ExpressionType.Equal:
            //            if (IsComparingWithNull(b))
            //                _sb.Append(" IS ");
            //            else
            //                _sb.Append(" = ");
            //            break;
            //        case ExpressionType.GreaterThan:
            //            _sb.Append(" > ");
            //            break;
            //        case ExpressionType.NotEqual:
            //            _sb.Append(" <> ");
            //            break;
            //        case ExpressionType.LessThan:
            //            _sb.Append(" < ");
            //            break;
            //        case ExpressionType.LessThanOrEqual:
            //            _sb.Append(" <= ");
            //            break;
            //        case ExpressionType.GreaterThanOrEqual:
            //            _sb.Append(" >= ");
            //            break;
            //        default:
            //            throw new NotImplementedException();


            //    }
            //    Visit(b.Right);
            //    _sb.Append(")");
            //    return b;
            //}
            //internal void Translate(LambdaExpression predicate)
            //{
            //    throw new NotImplementedException();
            //}
        }
    }
}
