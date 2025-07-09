using Domain.Entities;
using System;
using System.Linq;

namespace Services.Specifications
{
    public class AppointmentWithDetailsSpecifications : Specifications<Appointment>
    {
        public AppointmentWithDetailsSpecifications(Shared.AppointmentModels.AppointmentSpecificationsParameters parameters)
            : base(a =>
                (!parameters.CustomerId.HasValue || a.CustomerId == parameters.CustomerId.ToString()) &&
                (!parameters.PropertyId.HasValue || a.PropertyId == parameters.PropertyId.Value) &&
                (!parameters.DeveloperId.HasValue || a.DeveloperId == parameters.DeveloperId.Value) &&
                (!parameters.BrokerId.HasValue || a.BrokerId == parameters.BrokerId.Value) &&
                (!parameters.Status.HasValue || a.Status == parameters.Status.Value) &&
                (!parameters.FromDate.HasValue || a.AppointmentDate >= parameters.FromDate.Value) &&
                (!parameters.ToDate.HasValue || a.AppointmentDate <= parameters.ToDate.Value)
            )
        {
            AddInclude(a => a.Property);
            AddInclude(a => a.Developer);
            AddInclude(a => a.Developer.User);
            AddInclude(a => a.Broker);
            AddInclude(a => a.Customer);
        }

        public AppointmentWithDetailsSpecifications(int appointmentId)
            : base(a => a.Id == appointmentId)
        {
            AddInclude(a => a.Property);
            AddInclude(a => a.Developer);
            AddInclude(a => a.Developer.User);
            AddInclude(a => a.Broker);
            AddInclude(a => a.Customer);
        }
    }
} 