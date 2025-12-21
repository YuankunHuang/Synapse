FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["Synapse.Server/Synapse.Server.csproj", "Synapse.Server/"]
COPY ["Synapse.Shared/Synapse.Shared.csproj", "Synapse.Shared/"]

RUN dotnet restore "Synapse.Server/Synapse.Server.csproj"

COPY . .

WORKDIR "/src/Synapse.Server"

RUN dotnet publish "Synapse.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Synapse.Server.dll"]