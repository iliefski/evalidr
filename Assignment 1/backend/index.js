var express = require("express");
const path = require("path");
const low = require('lowdb') // minimal json db
const FileSync = require('lowdb/adapters/FileSync')
const adapter = new FileSync('db.json')
const db = low(adapter)

var app = express();
app.listen(3000, () => {
 console.log("Server running on port 3000");
});

app.use(express.json()); // for parsing incoming json request bodies
app.use(express.static(path.join(__dirname, '../frontend/dist/')))

// serve vue fronten
app.get('/', function (req, res) {
    res.status(200)
    res.sendFile(__dirname, '../frontend/dist/index.html')   });

//incoming post-req for a new pattern, write to db and respond with updated set of patterns
app.post('/pattern', (req, res) => {
    newPattern = req.body
    db.get('patterns')
        .push(newPattern)
        .write()

    let patterns = db.get('patterns');
    res.status(200).json(patterns)
    })  

// incoming get-req for all patterns
app.get("/patterns", (req, res, next) => {
    let patterns = db.get('patterns');
    res.json(patterns);
    });

