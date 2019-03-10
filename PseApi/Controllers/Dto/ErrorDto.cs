using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PseApi.Controllers.Dto
{
    public class ErrorDto
    {
        public string Error { get; private set; }

        public ErrorDto(string error)
        {
            Error = error;
        }
    }
}
