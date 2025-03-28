FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PaymentService.csproj", "./"]
RUN dotnet restore "PaymentService.csproj" --disable-parallel
COPY . .
RUN dotnet build "PaymentService.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"] 