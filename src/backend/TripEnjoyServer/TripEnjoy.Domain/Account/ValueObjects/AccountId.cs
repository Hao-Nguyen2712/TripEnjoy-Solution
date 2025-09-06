using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TripEnjoy.Domain.Common.Models;

namespace TripEnjoy.Domain.Account.ValueObjects
{
    public class AccountId : ValueObject
    {
        public Guid Id { get; private set; }

        public AccountId(Guid id)
        {
            Id = id;
        }

        public static AccountId Create(Guid id)
        {
            return new AccountId(id);
        }

        public static AccountId CreateUnique()
        {
            return new AccountId(Guid.NewGuid());
        }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}