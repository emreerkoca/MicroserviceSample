#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Services/NotificationService/NotificationServicee/NotificationServicee.csproj", "src/Services/NotificationService/NotificationServicee/"]
RUN dotnet restore "src/Services/NotificationService/NotificationServicee/NotificationServicee.csproj"
COPY . .
WORKDIR "/src/src/Services/NotificationService/NotificationServicee"
RUN dotnet build "NotificationServicee.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationServicee.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationServicee.dll"]