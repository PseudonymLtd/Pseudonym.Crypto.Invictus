using System;
using System.Collections.Generic;
using System.Linq;

namespace Pseudonym.Crypto.Invictus.Shared.Hosting.Models
{

    public class ExceptionDetail
    {
        public string Message { get; set; }

        public List<string> StackTrace { get; set; }

        public ExceptionDetail InnerException { get; set; }

        public static ExceptionDetail Create(Exception e)
        {
            return new ExceptionDetail()
            {
                StackTrace = e.StackTrace
                    ?.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    ?.Select(f => f.Replace("\r", string.Empty).Trim())
                    ?.ToList() ?? new List<string>(),
                Message = e.Message,
                InnerException = e.InnerException != null
                    ? Create(e.InnerException)
                    : null,
            };
        }
    }
}
