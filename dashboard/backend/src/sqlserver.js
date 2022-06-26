const connectionString = process.env.SQL_CONNECTION_STRING
const username = process.env.USERNAMESQL;
const password = process.env.PASSWORDSQL;
const SQLserver = process.env.YOUR_SERVER;

const config = {
    "server": SQLserver,
    "user": username,
    "password": password,
    "options": { "encrypt": true, "database": "dbParking" }
};

module.exports = config