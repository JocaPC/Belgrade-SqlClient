using Belgrade.SqlClient;
using Belgrade.SqlClient.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Belgrade.SqlClient
{
    public static class ErrorHandlerExtension
    {
        public static IQueryPipe OnError(this IQueryPipe pipe, Action<Exception> handler)
        {
            var stmt = pipe as BaseStatement;
            stmt.AddErrorHandler(new ActionErrorHandlerBuilder(handler));
            return pipe;
        }


        public static IQueryMapper OnError(this IQueryMapper mapper, Action<Exception> handler)
        {
            var stmt = mapper as BaseStatement;
            stmt.AddErrorHandler(new ActionErrorHandlerBuilder(handler));
            return mapper;
        }


        public static ICommand OnError(this ICommand cmd, Action<Exception> handler)
        {
            var stmt = cmd as BaseStatement;
            stmt.AddErrorHandler(new ActionErrorHandlerBuilder(handler));
            return cmd;
        }
    }
}
