
# description of script

# exit when any command fails and logs stuff
set -xe

# variables
# docker hub profile
PROFILE="chrwalte"
# project name
PROJECT="sysinternals.installer"
# version of the project
VERSION=$(cat ../VERSION)build

# docker commands:
docker build --no-cache -t $PROJECT:$VERSION ..
docker tag $PROJECT:$VERSION $PROFILE/$PROJECT:$VERSION
docker tag $PROJECT:$VERSION $PROFILE/$PROJECT:build
docker push $PROFILE/$PROJECT:$VERSION
docker push $PROFILE/$PROJECT:build
