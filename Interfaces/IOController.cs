using System;
using System.Threading;
using MiskoPersist.Enums;

namespace MiskoPersist.Interfaces
{
    public interface IOController
    {
    	Int32 RowsPerPage { get; }
        
        DataSource DataSource  { get; }
        
        ConnectionProvider GetConnectionProvider();

        void Debug(Object obj);
        
        void Status(String message);

        void MessageReceived();

        void MessageSent();

        void ExceptionHandler(Object sender, ThreadExceptionEventArgs e);

        void Error(String message);

        void Warning(String message);

        void Info(String message);

        Boolean Confirm(String message);
    }
}
