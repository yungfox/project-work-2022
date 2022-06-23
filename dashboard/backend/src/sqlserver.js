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

// logica di interrogazione del server...

// https://docs.microsoft.com/en-us/sql/connect/node-js/step-3-proof-of-concept-connecting-to-sql-using-node-js?view=sql-server-ver16
// https://github.com/tediousjs/tedious/tree/master/examples