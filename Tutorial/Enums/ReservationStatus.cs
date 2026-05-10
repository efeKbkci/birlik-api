namespace Tutorial.Enums
{
    public enum ReservationStatus
    {
        Pending = 1,    // Beklemede (Yazıhane onayı bekleniyor)
        Confirmed = 2,  // Onaylandı (Yer ayrıldı)
        Cancelled = 3,  // İptal Edildi (Kullanıcı veya yazıhane iptal etti)
    }
}
