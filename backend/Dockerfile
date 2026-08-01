# Backend .NET Core — build multi-stage DENTRO del contenedor (el host no tiene .NET SDK).
# Etapa de build: SDK completo.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Backend.csproj ./
RUN dotnet restore Backend.csproj
COPY . ./
RUN dotnet publish Backend.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa de runtime: solo el runtime ASP.NET (imagen ligera).
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Backend.dll"]
