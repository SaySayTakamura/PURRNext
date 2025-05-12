echo "- BUILDING IMAGE -"
(exec docker build -t purr-next-dev -f Dockerfile .)
echo "- DONE -