#!/usr/bin/python3

from flask import request,Flask, jsonify 
from flask_api import FlaskAPI
from flask_cors import CORS,cross_origin
import json
import time
import os

app = FlaskAPI(__name__, static_url_path='')
cors = CORS(app, resources={r"/*": {"origins": "*"}})
app = Flask(__name__, static_folder="")

@app.route('/new_commands',methods=["POST"])
def new_comands():
    print("entra")
    data = json.loads(request.data)
    print(data)
    #commands.clear();
    #pins.clear();
    #value.clear();
    command_number=len(data['allCommands'])
    
    organization = data['allCommands'][0]["orden"];
    date_from = data['allCommands'][0]["pin"];
    date_to = data['allCommands'][0]["value"];
    
    send= "mongoexport --db learninglocker_v2 --collection statements --out ./statements.json --query=\'{\"organisation\": {\"$oid\": \""+organization + "\"},\"timestamp\": {\"$lt\": {\"$date\": \"" + date_from + "\"},\"$gte\": {\"$date\": \"" + date_to + "\"}}}'";
    os.system(send);

@app.route('/read_json',methods=["GET","POST"])
@cross_origin(headers=['Access-Control-Allow-Origin', '*'])
def read_json():
    studentsList=[];
    with open('statements.json') as f:
        for jsonObj in f:
            studentDict = json.loads(jsonObj)
            studentsList.append(studentDict)
    f.close()
    send= "rm statements.json";
    os.system(send);
    return json.dumps(studentsList)
    #return {color: "GPIO.input(LEDS[color])"}

if __name__ == "__main__":
    app.run()
