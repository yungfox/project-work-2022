const tedious = require('tedious')
const connectionString = process.env.SQL_CONNECTION_STRING
const username = process.env.USERNAMESQL;
const password = process.env.PASSWORDSQL;
const SQLserver = process.env.YOUR_SERVER;

// logica di interrogazione del server...
var Connection = tedious.Connection;  
    var config = {  
        server: SQLserver,  //update me
        authentication: {
            type: 'default',
            options: {
                userName: username, //update me
                password: password  //update me
            }
        },
        options: {
            // If you are on Microsoft Azure, you need encryption:
            encrypt: true,
            database: 'dbParking'  //update me
        }
    };  
    const connection = new Connection(config);

    connection.connect((err) => {
    if (err) {
        console.log('Connection Failed');
        throw err;
    }

    executeStatement();
    });
  
    var Request = tedious.Request;  
    var TYPES = tedious.TYPES;  
  
    function executeStatement() {  
        request = new Request("SELECT Id,Status FROM tblParkingSpot for json path", function(err) {  
        if (err) {  
            console.log(err);}  
        });  
        var result = []; 
        request.on('row', function(columns) {    
            var obj = JSON.parse(columns);

            console.log(obj);     
        });  
  
        console.log(result);

        request.on('done', function(rowCount, more) {  
        console.log(rowCount + ' rows returned');  
        });  
        
        // Close the connection after the final event emitted by the request, after the callback passes
        request.on("requestCompleted", function (rowCount, more) {
            connection.close();
        });
        connection.execSql(request);  
    }  

// https://docs.microsoft.com/en-us/sql/connect/node-js/step-3-proof-of-concept-connecting-to-sql-using-node-js?view=sql-server-ver16
// https://github.com/tediousjs/tedious/tree/master/examples