using System;
using System.Threading;
using MiskoPersist.Core;
using MiskoPersist.Enums;
using Newtonsoft.Json;
using System.ComponentModel;

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
