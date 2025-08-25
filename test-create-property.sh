#!/bin/bash

echo "🧪 Test du endpoint createProperty"
echo "=================================="

# URL du endpoint
URL="http://localhost:5000/api/property/create"

# Payload JSON pour le test
PAYLOAD='{
  "title": "Appartement moderne a Douala",
  "description": "Magnifique appartement 3 chambres avec vue sur la ville, entierement meuble et equipe. Ideal pour famille ou professionnel.",
  "propertyType": "Appartement",
  "maxGuests": 6,
  "bedrooms": 3,
  "beds": 4,
  "bathrooms": 2,
  "squareMeters": 120,
  "address": "123 Avenue de l'\''Independance",
  "neighborhood": "Akwa",
  "city": "Douala",
  "postalCode": "237",
  "latitude": 4.0511,
  "longitude": 9.7679,
  "pricePerNight": 150000,
  "cleaningFee": 10000,
  "serviceFee": 5000,
  "ownerId": "550e8400-e29b-41d4-a716-446655440000",
  "amenities": [
    {
      "name": "Climatisation",
      "description": "Climatisation centrale",
      "type": 1,
      "category": 1,
      "isAvailable": true,
      "isIncludedInRent": true,
      "additionalCost": 0,
      "icon": "ac",
      "priority": 1
    },
    {
      "name": "Internet",
      "description": "WiFi haute vitesse",
      "type": 2,
      "category": 2,
      "isAvailable": true,
      "isIncludedInRent": true,
      "additionalCost": 0,
      "icon": "wifi",
      "priority": 1
    }
  ],
  "images": [
    {
      "imageUrl": "https://example.com/image1.jpg",
      "altText": "Salon principal",
      "isMainImage": true,
      "displayOrder": 1
    },
    {
      "imageUrl": "https://example.com/image2.jpg",
      "altText": "Chambre principale",
      "isMainImage": false,
      "displayOrder": 2
    }
  ]
}'

echo "📡 Envoi de la requête POST vers: $URL"
echo "📦 Payload:"
echo "$PAYLOAD" | jq '.' 2>/dev/null || echo "$PAYLOAD"

echo ""
echo "🔄 Exécution de la requête..."

# Exécution de la requête
RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$URL" \
  -H "Content-Type: application/json" \
  -d "$PAYLOAD")

# Séparation de la réponse et du code HTTP
HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$RESPONSE" | head -n -1)

echo ""
echo "📊 Résultat:"
echo "Code HTTP: $HTTP_CODE"

if [ "$HTTP_CODE" = "201" ] || [ "$HTTP_CODE" = "200" ]; then
    echo "✅ Succès! Propriété créée avec succès."
    echo "📄 Réponse:"
    echo "$RESPONSE_BODY" | jq '.' 2>/dev/null || echo "$RESPONSE_BODY"
else
    echo "❌ Erreur lors de la création de la propriété."
    echo "📄 Réponse d'erreur:"
    echo "$RESPONSE_BODY" | jq '.' 2>/dev/null || echo "$RESPONSE_BODY"
fi

echo ""
echo "🏁 Test terminé."
