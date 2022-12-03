
# description of script

# exit when any command fails and logs stuff
set -xe

# variables
# docker hub profile
PROFILE="chrwalte"
# project name
PROJECT="sysinternals.installer"
# version of the project
VERSION=$(cat ../VERSION)

# docker commands:
docker build --no-cache -t $PROJECT:$VERSION ..
docker tag $PROJECT:$VERSION $PROFILE/$PROJECT:$VERSION
docker tag $PROJECT:$VERSION $PROFILE/$PROJECT:latest
docker push $PROFILE/$PROJECT:$VERSION
docker push $PROFILE/$PROJECT:latest
