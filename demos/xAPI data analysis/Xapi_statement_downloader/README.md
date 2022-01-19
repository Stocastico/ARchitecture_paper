# StatementDownloader

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 8.1.2.

## Project

This server is meant for doing a mongoexport command for a learninglocker database. this command is modified with the data introduced in the angular fields organization, date from and date to.

In order to do the cli command and obtain the json for the download a second server had been written in python (fl-app.py) that has two possible access new_commands for the mongoexport command and the read_json for the json.

## Python server

This server is done with Flask in order to add cors and an easy to program server by default it serve in the host: and port:5000

This file obtains the data from the angular fields inside the http post in json format and place it in the query that is sent to the cli. This query will create a file in the same directory called statements.json with the information of the organization introduced between the dates selected.

It also reads the file statements.json in order to send the information and download it in the computer that is showing the angular page. The python server answer with a dump of the json as a return of the http petition. 

## Angular server

This server is done with angular and is mainly the part of the system that can be interacted by the user. It is a simple server that has three math-fields which is passed to the python server with the Send request button and another button Obtain data that receive the json and format it to be downloaded. 

The main part of the angular server code is inside the route interfaz/mapeo-pins2/src/app/material, in this route you have both the app.component html and ts files that are the most important part for the server.

The html file contains the three mat-form-filed with the propierties configured for the id, example and requirety. In the case of the dates it also includes the code to make a pop-up with a calendar selector. It includes the two buttons that redirect to the functions sendJSON and obtainJSON.

the ts file contains the components and variables needed for the http conection and json composition as the two functions for the buttons, the sendJSON one obtains the data from the fields and transform the dates into the necessary data, when converted it does an http post with a json containing in the payload the data converted. The obtainJSON function will do an http post to be answered the json data and it will create and auto clickable invisible hyperlink wich will automatically download the file without the user notincing the adition of the hyperlink.

## Quick run the project

For a quick run two terminals are needed, one for the python server and another for the angular one. In the first case the command is ` python3 fl-app.py`, and int the second case `npm start`, although for a more serious deployment a build must be done.

For the python server some packets are needed to be installed with pip, which are flask, flask_api and flask_cors

### Development server

Run `ng serve --port 8082` for a dev server. Navigate to `http://localhost:8082/`. The app will automatically reload if you change any of the source files. in other python3 fl-app.py

### Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

### Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `--prod` flag for a production build.