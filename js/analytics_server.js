import express from 'express';
import fetch from 'node-fetch';
import {Builder, Browser, By, Key, until, promise} from 'selenium-webdriver';
import writeFile from 'fs';
import promisify from 'util';
import firefox from 'selenium-webdriver/firefox.js';
import bodyParser from 'body-parser';

import asyncHandler from 'express-async-handler';

promise.USE_PROMISE_MANAGER = false;

let app = express();
let driver = await new Builder().forBrowser('firefox').setFirefoxOptions(new firefox.Options().setBinary('/usr/bin/firefox').addArguments("--headless")).build();

app.set('view engine', 'ejs');
app.use(bodyParser.urlencoded({ extended: false}));
app.use(bodyParser.json())

const ANALYTICS_EVENTS = [
]

app.get('/analytics/:event_name', function(req, res) {
    console.log(`${req.params.event_name}`)
    if (true || ANALYTICS_EVENTS.includes(req.params.event_name)) {
        res.status(200);
        res.render('pages/analytics', {title: req.params.event_name});
    } else {
        res.status(404);
        res.send("Invalid Analytics Event")
    }
})

app.post('/analytics', asyncHandler (async (req, res) => {
        console.log(req.body);
        await driver.get(`localhost:8080/analytics/${req.body.event_name}`);
        await driver.wait(until.titleIs(req.body.event_name));
        await driver.wait(async () => {
                const readyState = await driver.executeScript('return document.readyState');
                return readyState === 'complete';
        });
        await driver.executeScript('gtag(\"config\", \"G-ZQD4Q4J1XZ\")');
        const data = await driver.takeScreenshot();
}));

app.listen(8080);

console.log('Server is listening on port 8080');
