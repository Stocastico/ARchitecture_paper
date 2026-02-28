#!/usr/bin/python3

from flask import request, Flask, jsonify
from flask_api import FlaskAPI
from flask_cors import CORS, cross_origin
import json
import os
import re
import subprocess

app = FlaskAPI(__name__, static_url_path='')
cors = CORS(app, resources={r"/*": {"origins": "*"}})
app = Flask(__name__, static_folder="")

# Allowlist patterns to validate user-supplied parameters before passing to mongoexport
_OID_RE = re.compile(r'^[0-9a-f]{24}$')
_DATE_RE = re.compile(r'^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?Z$')


def _validate_oid(value):
    """Raise ValueError if value is not a valid MongoDB ObjectId hex string."""
    if not _OID_RE.match(value):
        raise ValueError(f"Invalid organisation OID: {value!r}")
    return value


def _validate_date(value):
    """Raise ValueError if value is not a valid ISO-8601 UTC date string."""
    if not _DATE_RE.match(value):
        raise ValueError(f"Invalid date string: {value!r}")
    return value


@app.route('/new_commands', methods=["POST"])
def new_comands():
    print("entra")
    data = json.loads(request.data)
    print(data)
    command_number = len(data['allCommands'])

    try:
        organization = _validate_oid(data['allCommands'][0]["orden"])
        date_from = _validate_date(data['allCommands'][0]["pin"])
        date_to = _validate_date(data['allCommands'][0]["value"])
    except ValueError as exc:
        return jsonify({"error": str(exc)}), 400

    query = (
        '{"organisation": {"$oid": "' + organization + '"}, '
        '"timestamp": {"$lt": {"$date": "' + date_from + '"}, '
        '"$gte": {"$date": "' + date_to + '"}}}'
    )

    # Use a list of arguments – never a shell string – to prevent injection.
    subprocess.run(
        [
            "mongoexport",
            "--db", "learninglocker_v2",
            "--collection", "statements",
            "--out", "./statements.json",
            "--query", query,
        ],
        check=True,
    )


@app.route('/read_json', methods=["GET", "POST"])
@cross_origin(headers=['Access-Control-Allow-Origin', '*'])
def read_json():
    students_list = []
    with open('statements.json') as f:
        for json_obj in f:
            student_dict = json.loads(json_obj)
            students_list.append(student_dict)
    os.remove('statements.json')
    return json.dumps(students_list)


if __name__ == "__main__":
    app.run()
