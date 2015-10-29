using System;
using System.Threading;
using MiskoPersist.Data;
using MiskoPersist.Enums;

namespace MiskoPersist.Interfaces
{
    public interface IOController
    {
    	Int32 RowsPerPage { get; }
        
        ServerLocation ServerLocation { get; }

        String Host { get; }

        Int16 Port { get; }

        String Script { get; }

        Boolean UseSSL { get; }
        
        SerializationType SerializationType { get; }
        
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
