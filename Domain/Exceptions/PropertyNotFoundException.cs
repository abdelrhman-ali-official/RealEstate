using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class PropertyNotFoundException : EntityNotFoundException
    {
        public PropertyNotFoundException(string id)
            : base($"The property with ID {id} was not found.")
        {
        }
    }
} 