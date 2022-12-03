
# docker compose redeploy script
# stops all containers listed in the docker-compose.yml file
# at the current directory, wipes the docker system, and then
# starts all the containers back up 

# exit when any command fails and logs some stuff
set -xe

# stop all containers listed in docker-compose.yml file
docker compose down

# clean the docker system
docker system prune -a --volumes -f

# pull all images for containers listed in docker-compose.yml file
docker compose pull

# start all containers listed in docker-compose.yml file
docker compose up -d
