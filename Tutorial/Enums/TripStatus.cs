namespace Tutorial.Enums
{
    public enum TripStatus
    {
        Scheduled = 1,  // Planlandı (Henüz başlamadı)
        OnSale = 2,    // Satışta (Biletler satışa çıktı)
        Underway = 3,     // Seferde (Şu an yolda)
        Completed = 4,  // Tamamlandı (Hedefe vardı)
        Cancelled = 5   // İptal Edildi
    }
}
