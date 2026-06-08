using Birlik.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Route = Tutorial.Entities.Route;

namespace Tutorial.Core.Specifications;

public class FilteredRoutesSpecification : BaseSpecification<Route>
{
    public FilteredRoutesSpecification(RouteFilter filter) : base(r => 
        ((filter.ArrivalCityName == null) || r.ArrivalCity.Name.ToLower().Contains(filter.ArrivalCityName.ToLower())) &&
        ((filter.DepartureCityName == null) || r.DepartureCity.Name.ToLower().Contains(filter.DepartureCityName.ToLower()))
    )
    { }
}