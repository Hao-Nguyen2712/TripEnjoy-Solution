using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TripEnjoy.Domain.Account.Enums
{
    public enum AccountStatusEnum
    {
        PendingVerification,
        Active,
        Locked,
        Banned,
        Deleted 
    }
}