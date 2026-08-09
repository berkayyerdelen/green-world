# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore (copy csproj/sln first for layer caching)
COPY GreenWorld.sln ./
COPY GreenWorld.Domain/*.csproj              GreenWorld.Domain/
COPY GreenWorld.Application/*.csproj         GreenWorld.Application/
COPY GreenWorld.Infrastructure/*.csproj      GreenWorld.Infrastructure/
COPY GreenWorld.Api/*.csproj                 GreenWorld.Api/
COPY GreenWorld.SharedKernel/*.csproj        GreenWorld.SharedKernel/
COPY GreenWorld.Domain.Tests/*.csproj        GreenWorld.Domain.Tests/
COPY GreenWorld.Application.Tests/*.csproj   GreenWorld.Application.Tests/
RUN dotnet restore GreenWorld.Api/GreenWorld.Api.csproj

# Build + publish
COPY . .
RUN dotnet publish GreenWorld.Api/GreenWorld.Api.csproj -c Release -o /app --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GreenWorld.Api.dll"]
