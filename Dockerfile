# 1. Uygulamanın içinde yaşayacağı en hafif ve temel odayı hazırlıyoruz.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base  
WORKDIR /app
EXPOSE 8080

# 2. Kodlarımızı derlemek için büyük alet çantamızı (SDK) sahaya getiriyoruz. 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# YENİ: Her iki projenin de .csproj dosyalarını kendi klasör yapılarına uygun olarak kopyala.
COPY ["Birlik.API/Tutorial/Tutorial.csproj", "Birlik.API/Tutorial/"]
COPY ["Birlik.Shared/Birlik.Shared.csproj", "Birlik.Shared/"]

# Sadece ana projeyi restore et (Shared proje bağımlı olduğu için otomatik restore edilecektir).
RUN dotnet restore "Birlik.API/Tutorial/Tutorial.csproj"

# YENİ: Her iki projenin de tüm kodlarını ilgili klasörlere kopyala.
# COPY . . yapmak yerine sadece ihtiyacımız olan klasörleri kopyalamak daha güvenlidir.
COPY ["Birlik.API/", "Birlik.API/"]
COPY ["Birlik.Shared/", "Birlik.Shared/"]

# Ana projenin olduğu klasöre git
WORKDIR "/src/Birlik.API/Tutorial"

# Kodu Release (Canlı Ortam) modunda derle
RUN dotnet build "Tutorial.csproj" -c Release -o /app/build

# 3. Kodlar derlendi, şimdi onları çalışmaya en hazır ve küçültülmüş hale getirme vakti.
FROM build AS publish
RUN dotnet publish "Tutorial.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. İlk adımda oluşturduğumuz temiz ve hafif odaya geri dönüyoruz.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Tutorial.dll"]