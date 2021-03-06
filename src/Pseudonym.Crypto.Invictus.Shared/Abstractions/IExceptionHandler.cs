﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pseudonym.Crypto.Invictus.Shared.Abstractions
{
    public interface IExceptionHandler
    {
        Task HandleAsync(HttpContext context, Exception e);
    }
}
