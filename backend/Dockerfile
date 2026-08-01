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
# curl para el healthcheck de compose: la imagen de runtime no trae wget ni curl,
# así que el healthcheck anterior fallaba siempre ("wget: not found") y el
# contenedor quedaba `unhealthy` pese a responder correctamente.
RUN apt-get update \
 && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Backend.dll"]
