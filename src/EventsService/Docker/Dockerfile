# src/EventsService/Docker/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Копируем только .csproj — чтобы restore кэшировался отдельным слоем
COPY src/Shared/Contracts/Contracts.csproj src/Shared/Contracts/
COPY src/Shared/Common/Common.csproj src/Shared/Common/
COPY src/EventsService/Domain/Domain.csproj src/EventsService/Domain/
COPY src/EventsService/Application/Application.csproj src/EventsService/Application/
COPY src/EventsService/Infrastructure/Infrastructure.csproj src/EventsService/Infrastructure/
COPY src/EventsService/Presentation/Presentation.csproj src/EventsService/Presentation/
COPY src/EventsService/Web/Web.csproj src/EventsService/Web/

RUN dotnet restore src/EventsService/Web/Web.csproj

# 2. Копируем весь код и собираем
COPY src/Shared/ src/Shared/
COPY src/EventsService/ src/EventsService/

RUN dotnet publish src/EventsService/Web/Web.csproj -c Release -o /app --no-restore

# 3. Финальный образ — без SDK, только runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web.dll"]