# 1. Uygulamanın içinde yaşayacağı en hafif ve temel odayı hazırlıyoruz.

# Microsoft'un resmi deposundan sadece uygulamayı çalıştırmaya yarayan hafif ASP.NET Core 8.0 Runtime imajını indir.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base  
# app isminde bir klasör oluştur ve bundan sonraki komutları bu klasör içerisinde çalıştır. 
WORKDIR /app
# Konteynırın dış dünya ile haberleşeceği portu 8080 olarak bildir. (Metadata'dır, portu gerçekten dışarı açmak için docker-compose içindeki ports ayarını kullanırız)
EXPOSE 8080

# 2. Kodlarımızı derlemek için büyük alet çantamızı (SDK) sahaya getiriyoruz. 

# Kod derlemek için gereken tüm ağır aletleri içeren .NET SDK 8.0 imajını indir.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Derleme işlemleri için konteynır içerisine src isminde bir klasör oluştur. 
WORKDIR /src

# Nuget paketlerinin listesini kopyala. .csproj dosyasında değişiklik olmadığı sürece indirme işlemi tekrar yapılmayacak. Önbellek kullanılacak. 
COPY ["Tutorial/Tutorial.csproj", "Tutorial/"]

# Projenin ihtiyaç duyduğu tüm dış kütüphaneleri (NuGet paketlerini) internetten indir.
RUN dotnet restore "Tutorial/Tutorial.csproj"

# Tüm projeyi /src klasörünün altına kopyala.
COPY . .

WORKDIR "/src/Tutorial"

# Kodu Release (Canlı Ortam) modunda, performanslı çalışacak şekilde derle ve oluşan dosyaları /app/build klasörüne koy.
RUN dotnet build "Tutorial.csproj" -c Release -o /app/build

# 3. Kodlar derlendi, şimdi onları çalışmaya en hazır ve küçültülmüş hale getirme vakti.

FROM build AS publish

# Uygulamayı canlı ortama çıkacak şekilde yayınla (publish). Gereksiz tüm geliştirici dosyalarını çöpe at ve sadece saf, çalıştırılabilir dosyaları /app/publish klasörüne yerleştir
RUN dotnet publish "Tutorial.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. ilk adımda oluşturduğumuz temiz ve hafif odaya geri dönüyoruz.

# En başta oluşturduğumuz hafif "base" imajını çağır ve bu son aşamaya "final" adını ver.
FROM base AS final

WORKDIR /app

# Ağır SDK imajının (publish aşaması) içindeki /app/publish klasörüne git, oradaki derlenmiş ve temizlenmiş dosyaları al, 
# şu an bulunduğum hafif imajın içine kopyala. Ağır SDK imajı artık tamamen çöpe gidebilir, onunla işimiz bitti.
COPY --from=publish /app/publish .

# Konteyner çalıştırıldığı anda (örneğin docker-compose up dediğinde) otomatik olarak dotnet ProjeAdi.dll komutunu tetikle ve uygulamayı ayağa kaldır.
ENTRYPOINT ["dotnet", "Tutorial.dll"]