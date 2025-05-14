echo "- BUILDING IMAGE -"
(cd ../. && exec docker build -t purr-next-dev -f Dockerfile .)
echo "- DONE -"