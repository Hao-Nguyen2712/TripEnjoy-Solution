using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TripEnjoy.ShareKernel.Email
{
    public class EmailConfiguration
    {
        public string Host { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}