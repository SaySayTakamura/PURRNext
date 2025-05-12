echo "- Running Container -"
(exec docker compose run --detach --service-ports --name=PURRNext-devel purr-next-dev)
echo "- Container Created -"