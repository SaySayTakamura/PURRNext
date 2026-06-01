echo "- BUILDING IMAGE -"
(cd ../. && exec docker build --no-cache -t purr-next-dev -f Dockerfile .)
echo "- DONE -"