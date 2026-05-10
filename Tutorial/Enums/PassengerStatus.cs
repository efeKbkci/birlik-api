namespace Tutorial.Enums
{
    public enum PassengerStatus
    {
        Reserved = 1,   // Yer ayrıldı (Henüz otobüse gelmedi)
        Boarded = 2,    // Bindi (Araç içinde)
        NoShow = 3,     // Gelmedi (Sefer kalktı ama yolcu ortada yok)
        Cancelled = 4   // İptal (Biletini iptal etti)
    }
}
