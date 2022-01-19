import pandas as pd
import numpy as np
import re

## Different than what was expected, creating a unique for for every DF column
## performed a slower execution than having different fors for each DF column

def cleanInvalidDFEntries(id_key,stamp,actor_column,verb_column,object_column):

    day = [] # Check which day of the week the timestamp represent
    day_shift = [] # Check which shift of the day the time stamp represent: night, morning, afternoon
    actor = [] # List of respective actor's name 
    lang = [] # List of respective language of the verb
    action = [] # List of verbs
    object_aim = [] # List of afected object
    for i in range(len(id_key)):

    # Test which part of the day is and add it to the list
        day.append(stamp[i].day_name()) # Check which day of the week is
        if stamp[i].hour < 6 or stamp[i].hour >= 20:
            day_shift.append('night')
        elif stamp[i].hour >= 6 and stamp[i].hour < 12:
            day_shift.append('morning')
        else:
            day_shift.append('afternoon')

    # Grab the raw actor text in the CSV file as str, convert it to dict and collect the actor's name
        if not (actor_column[i] is np.nan): # Check if actor's name exists, if not, add a NaN which will be purged later
            if 'name' in actor_column[i].keys(): # Dict format: {'name':'person'}. Check if the key 'name' exists
                if actor_column[i]['name'] != "":
                    actor.append(actor_column[i]['name']) # This line appends the actor of the current iteration
                else:
                    actor.append(np.nan)
            else:
                actor.append(np.nan)
        else:
            actor.append(np.nan)

    # Grab the raw verb text in the CSV file as str, convert it to dict and collect the verb and the language
        if not (verb_column[i] is np.nan):
            if 'id' in verb_column[i].keys(): # dict format: {'display': {'language':'verb'}}. Check if 'display' exists
                if verb_column[i]['id'] != "":
                    action.append(re.split("/",verb_column[i]['id'])[-1]) # collects the verb in the current iteration and append it, NaN otherwise
                else:
                    action.append(np.nan)
            else:
                action.append(np.nan)
            if 'display' in verb_column[i].keys(): # dict format: {'display': {'language':'verb'}}. Check if 'display' exists
                if verb_column[i]['display'][list(verb_column[i]['display'].keys())[0]] != "":
                    lang.append(list(verb_column[i]['display'].keys())[0]) # this line appends the language value to the lang list, NaN otherwise
                else:
                    lang.append(np.nan)
            else:
                lang.append(np.nan)
        else:
            action.append(np.nan)
            lang.append(np.nan)
            
    # Grab the raw verb text in the CSV file as str, convert it to dict and collect the object
        if not (object_column[i] is np.nan): # dict format: {'definition':{'name':{'es-US':'object'}}}
            if 'definition' in object_column[i].keys(): # check if the key 'definition' exists. Appends NaN otherwise
                if 'name' in object_column[i]['definition'].keys(): # check if the key 'name' exists. Appends NaN otherwise
                    if object_column[i]['definition'][list(object_column[i]['definition'].keys())[0]] \
                    [list(object_column[i]['definition'][list(object_column[i]['definition'].keys())[0]].keys())[0]] != "":
                        object_aim.append(object_column[i]['definition'] # This line appends the object of the current iteration
                            [list(object_column[i]['definition'].keys())[0]]
                            [list(object_column[i]['definition'][list(object_column[i]['definition'].keys())[0]].keys())[0]])
                    else:
                        object_aim.append(np.nan)
                else:
                    object_aim.append(np.nan)
            else:
                object_aim.append(np.nan)
        else:
            object_aim.append(np.nan)
            
    d = pd.DataFrame(data={'id':id_key,'timestamp':stamp,'weekday':day,'dayshift':day_shift,'actor':actor,'verb':action,'object':object_aim,'language':lang})

    return dropInvalidValues(d)

def dropInvalidValues(data):
# This function purges all lines that contains NaN or NaT values of a DataFrame
    data.dropna(inplace=True)
    data.reset_index(drop=True,inplace=True)
    return data