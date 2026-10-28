# בנייה ופרסום של App.Api לשרת Render
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish App.Api/App.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production
# Render מעביר את הפורט במשתנה PORT
CMD ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet App.Api.dll
