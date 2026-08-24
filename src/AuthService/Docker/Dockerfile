# src/AuthService/Docker/Dockerfile

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Копируем только .csproj — чтобы restore кэшировался отдельным слоем
COPY src/Shared/Contracts/Contracts.csproj src/Shared/Contracts/
COPY src/Shared/Common/Common.csproj src/Shared/Common/
COPY src/AuthService/Domain/Domain.csproj src/AuthService/Domain/
COPY src/AuthService/Application/Application.csproj src/AuthService/Application/
COPY src/AuthService/Infrastructure/Infrastructure.csproj src/AuthService/Infrastructure/
COPY src/AuthService/Presentation/Presentation.csproj src/AuthService/Presentation/
COPY src/AuthService/Web/Web.csproj src/AuthService/Web/

RUN dotnet restore src/AuthService/Web/Web.csproj

# 2. Копируем весь код и собираем
COPY src/Shared/ src/Shared/
COPY src/AuthService/ src/AuthService/

RUN dotnet publish src/AuthService/Web/Web.csproj -c Release -o /app --no-restore

# 3. Финальный образ — без SDK, только runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web.dll"]