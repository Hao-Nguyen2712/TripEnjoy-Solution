# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["TripEnjoyServer.sln", "./"]
COPY ["TripEnjoy.Api/TripEnjoy.Api.csproj", "TripEnjoy.Api/"]
COPY ["TripEnjoy.Application/TripEnjoy.Application.csproj", "TripEnjoy.Application/"]
COPY ["TripEnjoy.Domain/TripEnjoy.Domain.csproj", "TripEnjoy.Domain/"]
COPY ["TripEnjoy.Infrastructure/TripEnjoy.Infrastructure.csproj", "TripEnjoy.Infrastructure/"]
COPY ["TripEnjoy.Infrastructure.Persistence/TripEnjoy.Infrastructure.Persistence.csproj", "TripEnjoy.Infrastructure.Persistence/"]
COPY ["TripEnjoy.ShareKernel/TripEnjoy.ShareKernel.csproj", "TripEnjoy.ShareKernel/"]

# Restore dependencies
RUN dotnet restore "TripEnjoy.Api/TripEnjoy.Api.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/TripEnjoy.Api"
RUN dotnet build "TripEnjoy.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TripEnjoy.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "TripEnjoy.Api.dll"]
