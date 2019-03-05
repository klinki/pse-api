echo "Deploy script started"
cd ~/inviser
docker-compose pull app_pse
docker-compose down app_pse
docker-compose up -d app_pse
echo "Deploy script finished execution"
