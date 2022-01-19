import pandas as pd
from DFCleaner import *
from pathlib import Path

def readRawJsonFile(datapath,id_key,timestamp_key='timestamp',actor_key='actor',verb_key='verb',object_key='object'):
    assert Path(datapath).exists(), "File not found"
    data = pd.DataFrame(pd.read_json(datapath)) # Read the csv file according to path passed by argument
    dataread = pd.DataFrame(data['statement'].tolist())
    stamp = pd.to_datetime(dataread['stored'])
    actor_list_of_dict = dataread['actor'].to_list()
    verb_list_of_dict = dataread['verb'].to_list()
    obj_list_of_dict = dataread['object'].to_list()
    return cleanInvalidDFEntries(dataread,id_key,stamp,actor_list_of_dict,verb_list_of_dict,obj_list_of_dict)