FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY IncidentManagementSystem.slnx .
COPY src/IncidentManagement.Api/IncidentManagement.Api.csproj src/IncidentManagement.Api/
COPY src/IncidentManagement.Application/IncidentManagement.Application.csproj src/IncidentManagement.Application/
COPY src/IncidentManagement.Domain/IncidentManagement.Domain.csproj src/IncidentManagement.Domain/
COPY src/IncidentManagement.Infrastructure/IncidentManagement.Infrastructure.csproj src/IncidentManagement.Infrastructure/
COPY tests/IncidentManagement.Tests/IncidentManagement.Tests.csproj tests/IncidentManagement.Tests/
RUN dotnet restore src/IncidentManagement.Api/IncidentManagement.Api.csproj

COPY . .
RUN dotnet publish src/IncidentManagement.Api/IncidentManagement.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "IncidentManagement.Api.dll"]
