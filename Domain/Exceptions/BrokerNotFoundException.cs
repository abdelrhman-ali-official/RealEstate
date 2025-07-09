using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class BrokerNotFoundException : EntityNotFoundException
    {
        public BrokerNotFoundException(string id)
            : base($"The broker with ID {id} was not found.")
        {
        }
    }
} 