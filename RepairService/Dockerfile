FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY RepairService/RepairService.csproj RepairService/
RUN dotnet restore RepairService/RepairService.csproj
COPY RepairService/ RepairService/
WORKDIR /src/RepairService
RUN dotnet publish RepairService.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RepairService.dll"]
