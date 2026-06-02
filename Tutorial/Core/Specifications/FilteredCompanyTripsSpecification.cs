using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Core.Specifications;

public class FilteredCompanyTripsSpecification : BaseSpecification<Trip>
{
    public FilteredCompanyTripsSpecification(DashboardTripFilter filter) : base(t =>
            // 1. ZORUNLU KURAL: Firma ID kesinlikle eşleşmeli
            t.CompanyId == filter.CompanyId &&

            // 2. ESNEK (OPSİYONEL) KURALLAR
            // Taktik: Eğer filtre gönderilmediyse (!filter.HasValue == true) sağ tarafa hiç bakmaz!
            (!filter.RouteId.HasValue || t.RouteId == filter.RouteId) &&

            (!filter.VehicleId.HasValue || t.VehicleId == filter.VehicleId) &&

            (!filter.DriverId.HasValue || t.DriverId == filter.DriverId) &&

            (!filter.Status.HasValue || t.TripStatus == filter.Status) &&

            (!filter.StartDate.HasValue || t.DepartureTime >= filter.StartDate.Value) &&

            (!filter.EndDate.HasValue || t.DepartureTime <= filter.EndDate.Value)
        )
    {
    }
}
