#!/bin/bash

# TripEnjoy Docker Deployment Script
# This script helps you set up and deploy the TripEnjoy application stack

set -e

echo "============================================"
echo "   TripEnjoy Docker Deployment Setup"
echo "============================================"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    echo "   Visit: https://docs.docker.com/get-docker/"
    exit 1
fi

echo "✅ Docker is installed: $(docker --version)"

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    if ! docker compose version &> /dev/null; then
        echo "❌ Docker Compose is not installed."
        echo "   Visit: https://docs.docker.com/compose/install/"
        exit 1
    else
        COMPOSE_CMD="docker compose"
    fi
else
    COMPOSE_CMD="docker-compose"
fi

echo "✅ Docker Compose is available"
echo ""

# Check if .env file exists
if [ ! -f .env ]; then
    echo "⚠️  .env file not found. Creating from template..."
    cp .env.example .env
    echo "✅ Created .env file from .env.example"
    echo ""
    echo "❗ IMPORTANT: Please edit the .env file with your credentials:"
    echo "   - EMAIL_ADDRESS and EMAIL_PASSWORD (Gmail App Password)"
    echo "   - CLOUDINARY credentials (Cloud Name, API Key, API Secret)"
    echo ""
    echo "   After editing .env, run this script again."
    exit 0
fi

echo "✅ .env file found"
echo ""

# Check if required environment variables are set
source .env

MISSING_VARS=0

if [ -z "$EMAIL_ADDRESS" ] || [ "$EMAIL_ADDRESS" = "your-email@gmail.com" ]; then
    echo "❌ EMAIL_ADDRESS is not configured in .env"
    MISSING_VARS=1
fi

if [ -z "$EMAIL_PASSWORD" ] || [ "$EMAIL_PASSWORD" = "your-app-password" ]; then
    echo "❌ EMAIL_PASSWORD is not configured in .env"
    MISSING_VARS=1
fi

if [ -z "$CLOUDINARY_CLOUD_NAME" ] || [ "$CLOUDINARY_CLOUD_NAME" = "your-cloud-name" ]; then
    echo "❌ CLOUDINARY_CLOUD_NAME is not configured in .env"
    MISSING_VARS=1
fi

if [ -z "$CLOUDINARY_API_KEY" ] || [ "$CLOUDINARY_API_KEY" = "your-api-key" ]; then
    echo "❌ CLOUDINARY_API_KEY is not configured in .env"
    MISSING_VARS=1
fi

if [ -z "$CLOUDINARY_API_SECRET" ] || [ "$CLOUDINARY_API_SECRET" = "your-api-secret" ]; then
    echo "❌ CLOUDINARY_API_SECRET is not configured in .env"
    MISSING_VARS=1
fi

if [ $MISSING_VARS -eq 1 ]; then
    echo ""
    echo "❗ Please configure the missing variables in .env file and run this script again."
    exit 1
fi

echo "✅ All required environment variables are configured"
echo ""

# Ask what to do
echo "What would you like to do?"
echo "1) Start all services"
echo "2) Stop all services"
echo "3) Restart all services"
echo "4) View logs"
echo "5) Check service status"
echo "6) Clean up (remove containers and volumes)"
echo "7) Rebuild and start"
echo ""
read -p "Enter your choice (1-7): " choice

case $choice in
    1)
        echo ""
        echo "🚀 Starting all services..."
        $COMPOSE_CMD up -d
        echo ""
        echo "✅ Services started successfully!"
        echo ""
        echo "📍 Access URLs:"
        echo "   - Blazor Client:  http://localhost:7100"
        echo "   - API (Swagger):  http://localhost:7199/swagger"
        echo "   - Seq Logs:       http://localhost:5341"
        echo ""
        echo "💡 Check status: $COMPOSE_CMD ps"
        echo "💡 View logs:    $COMPOSE_CMD logs -f"
        ;;
    2)
        echo ""
        echo "⏸️  Stopping all services..."
        $COMPOSE_CMD down
        echo "✅ Services stopped"
        ;;
    3)
        echo ""
        echo "🔄 Restarting all services..."
        $COMPOSE_CMD restart
        echo "✅ Services restarted"
        ;;
    4)
        echo ""
        echo "📋 Showing logs (Ctrl+C to exit)..."
        $COMPOSE_CMD logs -f
        ;;
    5)
        echo ""
        $COMPOSE_CMD ps
        ;;
    6)
        echo ""
        read -p "⚠️  This will remove all containers and volumes. Continue? (y/N): " confirm
        if [ "$confirm" = "y" ] || [ "$confirm" = "Y" ]; then
            echo "🗑️  Cleaning up..."
            $COMPOSE_CMD down -v
            echo "✅ Cleanup complete"
        else
            echo "Cancelled"
        fi
        ;;
    7)
        echo ""
        echo "🔨 Rebuilding and starting services..."
        $COMPOSE_CMD up -d --build
        echo ""
        echo "✅ Services rebuilt and started!"
        echo ""
        echo "📍 Access URLs:"
        echo "   - Blazor Client:  http://localhost:7100"
        echo "   - API (Swagger):  http://localhost:7199/swagger"
        echo "   - Seq Logs:       http://localhost:5341"
        ;;
    *)
        echo "Invalid choice"
        exit 1
        ;;
esac
