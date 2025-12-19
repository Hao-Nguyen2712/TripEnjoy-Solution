# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["TripEnjoy.Client.csproj", "./"]

# Restore dependencies
RUN dotnet restore "TripEnjoy.Client.csproj"

# Copy all source files
COPY . .

# Build and publish
RUN dotnet publish "TripEnjoy.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - use nginx to serve the Blazor WASM app
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy published Blazor app
COPY --from=build /app/publish/wwwroot .

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
