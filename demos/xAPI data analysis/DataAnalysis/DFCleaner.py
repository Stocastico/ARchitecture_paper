import pandas as pd
import numpy as np
import re

def cleanInvalidDFEntries(data,id_key,stamp,actor_column,verb_column,object_column):

    day = [] # Check which day of the week the timestamp represent
    day_shift = [] # Check which shift of the day the time stamp represent: night, morning, afternoon
    for s in stamp:
        day.append(s.day_name())
        if s.hour < 6 or s.hour >= 20:
            day_shift.append('night')
        elif s.hour >= 6 and s.hour < 12:
            day_shift.append('morning')
        else:
            day_shift.append('afternoon')

    # Grab the raw actor text in the CSV file as str, convert it to dict and collect the actor's name
    actor = [] 
    for name in actor_column:
        if not (name is np.nan): # Check if actor's name exists, if not, add a NaN which will be purged later
            if 'name' in name.keys(): # Dict format: {'name':'person'}. Check if the key 'name' exists
                if name['name'] != "":
                    actor.append(name['name']) # This line appends the actor of the current iteration
                else:
                    actor.append(np.nan)
            else:
                actor.append(np.nan)
        else:
            actor.append(np.nan)

    # Grab the raw verb text in the CSV file as str, convert it to dict and collect the verb and the language
    lang = []
    action = []
    for verb in verb_column:
        if not (verb is np.nan):
            if 'id' in verb.keys(): # dict format: {'display': {'language':'verb'}}. Check if 'display' exists
                if verb['id'] != "":
                    action.append(re.split("/",verb['id'])[-1]) # collects the verb in the current iteration and append it, NaN otherwise
                else:
                    action.append(np.nan)
            else:
                action.append(np.nan)
            if 'display' in verb.keys(): # dict format: {'display': {'language':'verb'}}. Check if 'display' exists
                if verb['display'][list(verb['display'].keys())[0]] != "":
                    lang.append(list(verb['display'].keys())[0]) # this line appends the language value to the lang list, NaN otherwise
                else:
                    lang.append(np.nan)
            else:
                lang.append(np.nan)
        else:
            action.append(np.nan)
            lang.append(np.nan)

    # Grab the raw verb text in the CSV file as str, convert it to dict and collect the object
    object_aim = []
    for obj in object_column:
        if not (obj is np.nan): # dict format: {'definition':{'name':{'es-US':'object'}}}
            if 'definition' in obj.keys(): # check if the key 'definition' exists. Appends NaN otherwise
                if 'name' in obj['definition'].keys(): # check if the key 'name' exists. Appends NaN otherwise
                    if obj['definition'][list(obj['definition'].keys())[0]] \
                    [list(obj['definition'][list(obj['definition'].keys())[0]].keys())[0]] != "":
                        object_aim.append(obj['definition'] # This line appends the object of the current iteration
                            [list(obj['definition'].keys())[0]]
                            [list(obj['definition'][list(obj['definition'].keys())[0]].keys())[0]])
                    else:
                        object_aim.append(np.nan)
                else:
                    object_aim.append(np.nan)
            else:
                object_aim.append(np.nan)
        else:
            object_aim.append(np.nan)
            
    d = pd.DataFrame(data={'id':data[id_key],'timestamp':stamp,'weekday':day,'dayshift':day_shift,'actor':actor,'verb':action,'object':object_aim,'language':lang})

    return dropInvalidValues(d)

def dropInvalidValues(data):
# This function purges all lines that contains NaN or NaT values of a DataFrame
    data.dropna(inplace=True)
    data.reset_index(drop=True,inplace=True)
    return data