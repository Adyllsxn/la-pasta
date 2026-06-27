# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj e restaurar dependências
COPY src/LaPDF.Web/*.csproj src/LaPDF.Web/
RUN dotnet restore src/LaPDF.Web/LaPDF.Web.csproj

# Copiar todo o código e build
COPY src/ .
WORKDIR /src/LaPDF.Web
RUN dotnet publish LaPDF.Web.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "LaPDF.Web.dll"]