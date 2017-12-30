using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Common;
using static System.Console;

namespace TimekeeperDAL.Interception
{
    public class Interceptor : IDbCommandInterceptor
    {
        
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            WriteLine($"NonQueryExecuted IsAsync: {interceptionContext.IsAsync}, Command Text:\n{command.CommandText}");
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            //WriteLine($"NonQueryExecuting IsAsync: {interceptionContext.IsAsync}, Command Text:\n{command.CommandText}");
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            WriteLine($"ReaderExecuted IsAsync: {interceptionContext.IsAsync}, Command Text:\n{command.CommandText}");
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            //WriteLine($"ReaderExecuting IsAsync: {interceptionContext.IsAsync}, Command Text:\n{command.CommandText}");
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            WriteLine($"ScalarExecuted IsAsync: {interceptionContext.IsAsync}, Command Text:\n{command.CommandText}");
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            //WriteLine($"ScalarExecuting IsAsync: {interceptionContext.IsAsync}, Command Text:\n{command.CommandText}");
        }
    }
}
