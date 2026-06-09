using Birlik.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Route = Tutorial.Entities.Route;

namespace Tutorial.Core.Specifications;

public class FilteredRoutesSpecification : BaseSpecification<Route>
{
    // ILike : büyük - küçük harf duyarsız yapmak için kullandığımız fonksiyon.
    public FilteredRoutesSpecification(RouteFilter filter) : base(r =>
        (string.IsNullOrEmpty(filter.ArrivalCityName) ||
         EF.Functions.ILike(r.ArrivalCity.Name, $"%{filter.ArrivalCityName}%"))
        &&
        (string.IsNullOrEmpty(filter.DepartureCityName) ||
         EF.Functions.ILike(r.DepartureCity.Name, $"%{filter.DepartureCityName}%"))
    )
    {
    }
}