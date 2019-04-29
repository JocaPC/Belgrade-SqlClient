using Belgrade.SqlClient.Common;
using System;

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

        public static IQuery OnError(this IQuery query, Action<Exception> handler)
        {
            var stmt = query as BaseStatement;
            stmt.AddErrorHandler(new ActionErrorHandlerBuilder(handler));
            return query;
        }

        public static BaseStatement OnError(this BaseStatement stmt, Action<Exception> handler)
        {
            stmt.AddErrorHandler(new ActionErrorHandlerBuilder(handler));
            return stmt;
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
