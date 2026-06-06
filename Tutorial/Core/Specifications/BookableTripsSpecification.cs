using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Birlik.Shared.DTOs;
using Tutorial.Entities;
using Birlik.Shared.Enums;

namespace Tutorial.Core.Specifications;

public class BookableTripsSpecification : BaseSpecification<Trip>
{
    public BookableTripsSpecification(PassengerTripFilter filter) : base(t =>
                t.CompanyId == filter.CompanyId &&
                t.RouteId == filter.RouteId &&
                t.TripStatus == TripStatus.OnSale && // İş kuralı: Sadece satışta olanlar
                t.Capacity > t.PassengerNumbers      // İş kuralı: Boş yer olanlar
            )
    {

        var (startTime, endTime) = GetDateInterval(filter.DaySelection);
        Criterias.Add(t => t.DepartureTime >= startTime && t.DepartureTime < endTime);
    }

    private static (DateTime startTime, DateTime endTime) GetDateInterval(TripDaySelection selection)
    {
        // 1. ZAMANI YAKALA: Botun mesajı attığı o kritik an (Örn: 23.05.2026 14:46:12)
        DateTime messageTime = DateTime.UtcNow;

        // 2. BUGÜNÜN GECE YARISI: Saati sıfırla (Örn: 23.05.2026 00:00:00)
        DateTime todayMidnight = messageTime.Date;

        DateTime startTime;
        DateTime endTime;

        // Kullanıcı belirtmediyse (null ise) veya kasten Today seçtiyse
        if (selection == TripDaySelection.Today)
        {
            // ----------------------------------------------------------------
            // SENARYO 1: BUGÜN (23.05.2026 14:46 - 23.05.2026 23:59)
            // ----------------------------------------------------------------
            startTime = messageTime; // Başlangıç: 23.05.2026 14:46
            endTime = todayMidnight.AddDays(1); // Bitiş: 24.05.2026 00:00 (Yarının başlangıcı)
        }
        else
        {
            // ----------------------------------------------------------------
            // SENARYO 2: YARIN (24.05.2026 00:00 - 24.05.2026 23:59)
            // ----------------------------------------------------------------
            startTime = todayMidnight.AddDays(1); // Başlangıç: 24.05.2026 00:00
            endTime = startTime.AddDays(1); // Bitiş: 25.05.2026 00:00
        }

        return (startTime, endTime);
    }
}
