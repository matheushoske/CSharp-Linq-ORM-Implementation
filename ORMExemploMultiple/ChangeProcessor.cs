using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORMExemploMultiple
{
    internal class ChangeProcessor
    {
        private readonly ChangeTracker _tracker;
        private readonly MapeadorBD _mapeador;
        public ChangeProcessor(ChangeTracker tracker, MapeadorBD mapeador)
        {
            _tracker = tracker;
            _mapeador = mapeador;
        }
        DbCommand BuildDeleteCommand(object obj) 
        {
            throw new NotImplementedException();
        }
        private DbCommand BuildInsertCommand(TrackedObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            var cn = _mapeador.Configuracao.Connection;
            DbCommand command = cn.CreateCommand();
            StringBuilder sb = new StringBuilder("INSERT INTO ");
            sb.Append(obj.MetaTable.TableName);
            sb.Append(" (");
            // Find all the insertable columns (excluding
            // all the auto generated data members)
            var columns =
              obj.MetaTable.PersistentDataMembers
                .Where(c => !c.IsDbGenerated);
            StringBuilder columnValuesSb = new StringBuilder();
            bool isFirst = true;
            // Include all the columns in the INSERT statement
            foreach (var col in columns)
            {
                if (!isFirst)
                {
                    sb.Append(", ");
                    columnValuesSb.Append(", ");
                }
                else
                    isFirst = false;
                // What if the name of the column has a space character in it?
                sb.Append(FormatHelper.WrapInBrackets(col.MappedName));
                // Get the value from the entity for this column
                MemberInfo memberInfo = col.StorageMember ?? col.Member;
                // Format the value of the column to
                // its acceptable SQL representation

                
                var parametro = command.CreateParameter();
                parametro.ParameterName = new StringBuilder('@').Append(FormatHelper.WrapInBrackets(col.MappedName)).ToString();
                parametro.Value = TypeHelper.GetMemberValue(obj.Entity, memberInfo);
                command.Parameters.Add(parametro);//não sei se vai funcionar, por receber um tipo object
                columnValuesSb.Append(parametro.ParameterName);
            }
            sb.Append(") VALUES (");
            sb.Append(columnValuesSb.ToString());
            sb.Append(")");
            command.CommandText = sb.ToString();
            return command;
        }
        DbCommand BuildUpdateCommand(object obj)
        {
            throw new NotImplementedException();
        }
        DbCommand ExecuteCommand(DbCommand command)
        {
            throw new NotImplementedException();
        }
        internal void SubmitChanges()
        {
            foreach (var obj in _tracker.GetInterestingObjects())
            {
                DbCommand command = null;
                switch (obj.TrackingState)
                {
                    case TrackingState.ToBeInserted:
                        command = BuildInsertCommand(obj);
                        break;
                    case TrackingState.ToBeUpdated:
                        command = BuildUpdateCommand(obj);
                        break;
                    case TrackingState.ToBeDeleted:
                        command = BuildDeleteCommand(obj);
                        break;
                }
                if (command != null)
                    ExecuteCommand(command);
            }
        }
    }
}
