using System;
using System.Threading;
using MiskoPersist.Core;
using MiskoPersist.Enums;

namespace MiskoPersist.Interfaces
{
    public interface IOController
    {
    	Int32 RowsPerPage { get; }
        
        DataSource DataSource  { get; }
        
        void Status(MessageStatus status);

        void MessageReceived();

        void MessageSent();

        void Exception(Object sender, ThreadExceptionEventArgs e);

        void Error(ErrorMessage message);

        void Warning(ErrorMessage message);

        void Info(ErrorMessage message);

        Boolean Confirm(ErrorMessage message);
    }
}
