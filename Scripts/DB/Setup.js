print("Creating main users");

db = db.getSiblingDB("admin"); //Gets the Admin database

db.createUser({ user: "ADMIN", pwd: "1234", roles:[{role:"root", db:"admin"}]}); //Creates an DB Admin user
db.createUser({ user: "SERVER", pwd: "996854", roles:[{role:"readWriteAnyDatabase", db:"admin"}]}); //Creates an GUEST user for accessing public DB sections

print("All basic users created, creating Databases and Collections");

print("Setting up the INDEX Database")
index = db.getSiblingDB("Index"); //Index, the index database stores everything that we search using PURR Next
index.createCollection("Posts"); //Posts, all downloaded posts data is inserted here to keep track of them.
index.createCollection("Artists"); //Artists, when you search for an artist (Braeburned, for instance) it will throw its search data into this collection/table.
index.createCollection("Pools"); //Pools, whenever the search founds an post that has a pool, it will throw the Pool data into this collection/table, this includes a list of Post IDs related to this pool.
index.createCollection("News"); //News, holds all the published news about the site and server.
print("Done!");

print("Setting up the DATA Database")
data = db.getSiblingDB("Data"); //Data, the data database stores everything technical related, such as logs, queue and other things to come.
data.createCollection("Logs"); //Logs, when a search is done, the result of the operation is written to this collection/table
data.createCollection("Queue"); //Queue, whenever you make more than one search, the search is sent to this collection/table to be tracked, giving some transparency in the process.
data.createCollection("Users"); //Users, made to lock some content behind a login-wall instead of showing everything to the public.
print("Done!")

print("Setup process finished, exiting current script execution!");