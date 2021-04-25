import csv;
import pyperclip;

MAIN_DELIMETER = ';';
SECONDARY_DELIMETER = ',';

def load_data(filename):

    with open(filename) as file:

        reader = csv.reader(file)
        data = list(reader);

    return data;

def get_move_id(move_data, extra_move_ids, raw_move_name):

    move_name = raw_move_name.lower();

    for entry in move_data:

        if (entry[1].lower() == move_name.lower()):

            return entry[0];

    if move_name in extra_move_ids.keys():

        return extra_move_ids[move_name];

    return None;

def parse_list(string, move_data, extra_move_ids):

    #For basic lists of moves (eg. base, egg, tutor)

    output = "";
    not_found = [];

    if (not (MAIN_DELIMETER in string)) and (SECONDARY_DELIMETER in string):
        used_delimeter = SECONDARY_DELIMETER;
    else:
        used_delimeter = MAIN_DELIMETER;

    for move_name in string.split(used_delimeter):

        move_name = move_name.strip(" ");

        move_id = get_move_id(move_data, extra_move_ids, move_name);

        if move_id != None:
            
            output = output + move_id + MAIN_DELIMETER;
            
        else:
            
            not_found.append(move_name);

    output = output[:-1]; #Remove final MAIN_DELIMETER

    return output, not_found;

def parse_level_up(string, move_data, extra_move_ids):

    #For level-up moves

    output = "";
    not_found = [];

    for entry in string.split(SECONDARY_DELIMETER):

        entry_parts = entry.split(MAIN_DELIMETER);

        entry_parts[1] = entry_parts[1].strip(" ");

        move_id = get_move_id(move_data, extra_move_ids, entry_parts[1]);

        if move_id != None:

            output = output + entry_parts[0] + MAIN_DELIMETER + move_id + SECONDARY_DELIMETER;

        else:

            not_found.append(entry_parts[1]);

    output = output[:-1]; #Remove final SECONDARY_DELIMETER

    return output, not_found;

def get_extra_move_ids_input():

    _input = input("Extra Move Ids (name:id,name:id...)> ");

    if not (":" in _input) and (";" in _input):
        entry_splitter = ";";
    else:
        entry_splitter = ":";

    move_ids = {};

    for entry in _input.split(","):

        parts = entry.split(entry_splitter);

        if len(parts) != 2:
            print("Invalid extra move IDs format. Ignoring");
            return {};

        move_ids[parts[0]] = parts[1];

    return move_ids;

def main():

    filename = input("Moves File> ");

    extra_move_ids = get_extra_move_ids_input();

    while True:

        while True:
            
            mode_input = input("Regular (0) or Level-Up (1)?> ");

            if mode_input == "":

                return 0;
            
            elif mode_input == "0":
                
                mode = 0;
                break;
            
            else:
                
                mode = 1;
                break;

        input_data = input("Input> ");

        move_data = load_data(filename);

        if mode == 0:
            
            output, not_found = parse_list(input_data, move_data, extra_move_ids);
            
        elif mode == 1:
            
            output, not_found = parse_level_up(input_data, move_data, extra_move_ids);

        print(output);

        pyperclip.copy(output);
        print("Output copied");

        if len(not_found) > 0:
            print("Not Found:\n" + '\n'.join(not_found));

if __name__ == "__main__":
    
    main();
