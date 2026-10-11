<h1>Scripts</h1>

For docker/db usage only, not tested on Windows.

- BuildImage.sh
    - Build the image with Docker with no cache
- Run.sh
    - Runs the PURRNext base compose project (using SQLite as database)
- DB/SetupDB.js
    - Used on the MongoDB database first startup, creates users, setups databases and collections
- DB/SetupDB.sql
    - Used on the Postgres:18 database first startup, creates users, setups databases and collections