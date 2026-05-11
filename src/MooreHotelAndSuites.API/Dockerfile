FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore src/MooreHotelAndSuites.API/MooreHotelAndSuites.API.csproj
RUN dotnet publish src/MooreHotelAndSuites.API/MooreHotelAndSuites.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "MooreHotelAndSuites.API.dll"]