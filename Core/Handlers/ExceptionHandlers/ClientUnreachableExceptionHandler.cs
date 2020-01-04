﻿using Core.Handlers.ExceptionHandlers.Interfaces;
using Core.Models.Exceptions.ServerExceptions;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Handlers.ExceptionHandlers
{
    public class ClientUnreachableExceptionHandler : IExceptionHandler<ClientUnreachableException>
    {
        public Task HandleExceptionAsync(ClientUnreachableException exception, Socket socket, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task HandleExceptionAsync(Exception exception, Socket socket, CancellationToken cancellationToken)
            => await this.HandleExceptionAsync(exception as ClientUnreachableException, socket, cancellationToken);
    }
}
