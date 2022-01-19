import pandas as pd
import numpy as np
import json
from DFCleaner import *
from pathlib import Path


def readRawCSVFile(datapath,id_key,timestamp_key='timestamp',actor_key='actor',verb_key='verb',object_key='object'):
    assert Path(datapath).exists(), "File not found"
    data = pd.DataFrame(pd.read_csv(datapath)) # Read the csv file according to path passed by argument
    stamp = pd.to_datetime(data[timestamp_key],format='%Y-%m-%dT%H:%M:%S') # Collect the timestamp
    actor_list_of_dict = [json.loads(d) if not (d is np.nan) else np.nan for d in data[actor_key]] # Convert each actor entry into dict (it was str)
    verb_list_of_dict = [json.loads(d) if not (d is np.nan) else np.nan for d in data[verb_key]] # Convert each verb entry into dict (it was str)
    obj_list_of_dict = [json.loads(d) if not (d is np.nan) else np.nan for d in data[object_key]] # Convert each obj entry into dict (it was str)
    return cleanInvalidDFEntries(data,id_key,stamp,actor_list_of_dict,verb_list_of_dict,obj_list_of_dict)
