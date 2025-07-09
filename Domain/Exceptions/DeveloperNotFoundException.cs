using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class DeveloperNotFoundException : EntityNotFoundException
    {
        public DeveloperNotFoundException(string id)
            : base($"The developer with ID {id} was not found.")
        {
        }
    }
} 