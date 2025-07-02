#!/bin/bash
set -e

# 🧱 Configuration
IMAGE_NAME="ship-ses-extractor"
DOCKER_CONTEXT="./docker-context"
BUILD_CONFIGURATION="Release"

# 🧪 Get current Git commit hash
COMMIT_HASH=$(git rev-parse --short HEAD)
echo "🔢 Commit hash: $COMMIT_HASH"

# 📦 .NET publish
echo "📦 Publishing .NET Worker project..."
dotnet publish ./src/Ship.Ses.Extractor.Worker/Ship.Ses.Extractor.Worker.csproj -c $BUILD_CONFIGURATION -o ./publish

# 🛠️ Inject commit hash into appsettings.Production.json
echo "🔧 Updating appsettings.Production.json with commit hash..."
sed -i "s/\"commitHash\": \".*\"/\"commitHash\": \"$COMMIT_HASH\"/" ./src/Ship.Ses.Extractor.Worker/appsettings.Production.json

# 🧱 Prepare Docker context
echo "📁 Preparing Docker context at $DOCKER_CONTEXT..."
mkdir -p $DOCKER_CONTEXT/publish
cp -r ./publish/* $DOCKER_CONTEXT/publish/
cp ./src/Ship.Ses.Extractor.Worker/appsettings.Production.json $DOCKER_CONTEXT/
cp ./src/Ship.Ses.Extractor.Worker/Dockerfile $DOCKER_CONTEXT/

# 🐳 Build Docker image
echo "🐳 Building Docker image..."
#docker build -t $IMAGE_NAME:uat -f $DOCKER_CONTEXT/Dockerfile $DOCKER_CONTEXT
docker build -f $DOCKER_CONTEXT/Dockerfile -t $IMAGE_NAME:uat .

# 🔐 Docker login (make sure you're logged in already)
DOCKER_USERNAME=$(echo "https://index.docker.io/v1/" | docker-credential-desktop get 2>/dev/null | grep Username | cut -d'"' -f4)

# if [[ -z "$DOCKER_USERNAME" ]]; then
#  echo "⚠️ You are not logged in to Docker Hub. Run: docker login"
#  exit 1
#else
#  echo "✅ Logged in as: $DOCKER_USERNAME"
#fi

# 🏷️ Tag image
DOCKERHUB_USERNAME="laolu" # 👈 Replace or make dynamic
echo "🏷️ Tagging image as $DOCKERHUB_USERNAME/$IMAGE_NAME:uat"
docker tag $IMAGE_NAME:uat $DOCKERHUB_USERNAME/$IMAGE_NAME:uat

# 📤 Push to Docker Hub
echo "📤 Pushing image to Docker Hub..."
docker push $DOCKERHUB_USERNAME/$IMAGE_NAME:uat

echo "✅ Done. Image pushed as $DOCKERHUB_USERNAME/$IMAGE_NAME:uat"
