FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TICinema.Gateway/TICinema.Gateway/TICinema.Gateway.csproj", "TICinema.Gateway/TICinema.Gateway/"]
COPY ["TICinema.Contracts/TICinema.Contracts/TICinema.Contracts.csproj", "TICinema.Contracts/TICinema.Contracts/"]

RUN dotnet restore "TICinema.Gateway/TICinema.Gateway/TICinema.Gateway.csproj"

COPY . .
WORKDIR "/src/TICinema.Gateway/TICinema.Gateway"
RUN dotnet build "TICinema.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TICinema.Gateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080 
ENTRYPOINT ["dotnet", "TICinema.Gateway.dll"]