docker push registry.gitlab.com/klinki/docker-registry/pse:latest
docker push registry.gitlab.com/klinki/docker-registry/pse:${VERSION}
cat ./deploy.sh | ssh -i ./deploy_key ${DEPLOY_USER}@${DEPLOY_SERVER}
echo "Deploy finished"
