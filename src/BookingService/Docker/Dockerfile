# src/BookingService/Docker/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Копируем только .csproj — чтобы restore кэшировался отдельным слоем
COPY src/Shared/Contracts/Contracts.csproj src/Shared/Contracts/
COPY src/Shared/Common/Common.csproj src/Shared/Common/
COPY src/BookingService/Domain/Domain.csproj src/BookingService/Domain/
COPY src/BookingService/Application/Application.csproj src/BookingService/Application/
COPY src/BookingService/Infrastructure/Infrastructure.csproj src/BookingService/Infrastructure/
COPY src/BookingService/Presentation/Presentation.csproj src/BookingService/Presentation/
COPY src/BookingService/Web/Web.csproj src/BookingService/Web/

RUN dotnet restore src/BookingService/Web/Web.csproj

# 2. Копируем весь код и собираем
COPY src/Shared/ src/Shared/
COPY src/BookingService/ src/BookingService/

RUN dotnet publish src/BookingService/Web/Web.csproj -c Release -o /app --no-restore

# 3. Финальный образ — без SDK, только runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web.dll"]