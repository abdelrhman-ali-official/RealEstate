using Domain.Entities.BrokerEntities;
using Shared.BrokerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Specifications
{
    public class BrokerCountSpecifications : Specifications<Broker>
    {
        public BrokerCountSpecifications(BrokerSpecificationsParameters parameters)
            : base(broker =>
                (string.IsNullOrWhiteSpace(parameters.City) || broker.City.ToLower().Contains(parameters.City.ToLower().Trim())) &&
                (string.IsNullOrWhiteSpace(parameters.Government) || broker.Government.ToLower().Contains(parameters.Government.ToLower().Trim())) &&
                (string.IsNullOrWhiteSpace(parameters.AgencyName) || (broker.AgencyName != null && broker.AgencyName.ToLower().Contains(parameters.AgencyName.ToLower().Trim()))) &&
                (string.IsNullOrWhiteSpace(parameters.FullName) || broker.FullName.ToLower().Contains(parameters.FullName.ToLower().Trim())))
        {
        }
    }
} 