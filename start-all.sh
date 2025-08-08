#!/bin/bash

echo "🚀 Démarrage de tous les services Pioloop..."

# Démarrer l'API backend + PostgreSQL
echo "📦 Démarrage de l'API backend et PostgreSQL..."
cd Pioloop-api
docker-compose up -d

# Attendre que l'API soit prête
echo "⏳ Attente que l'API soit prête..."
sleep 10

# Démarrer le frontend
echo "🌐 Démarrage du frontend..."
cd ../Pioloop-web
docker-compose up -d

echo "✅ Tous les services sont démarrés !"
echo ""
echo "📋 URLs d'accès :"
echo "   Frontend: http://localhost:3000"
echo "   API Backend: http://localhost:64604"
echo "   Swagger: http://localhost:64604/swagger"
echo "   PostgreSQL: localhost:5432"
echo ""
echo "🔍 Vérification des containers :"
docker ps --filter "name=pioloop" 