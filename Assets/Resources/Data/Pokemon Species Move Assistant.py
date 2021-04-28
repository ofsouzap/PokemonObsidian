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

    using_space = not (MAIN_DELIMETER in string);

    for entry in string.split(SECONDARY_DELIMETER):

        entry = entry.strip(" ");

        lvl = name = None;

        if not using_space:

            entry_parts = entry.split(MAIN_DELIMETER);
            lvl = entry_parts[0];
            name = entry_parts[1];

        else:

            entry_parts = entry.split(" ");
            lvl = entry_parts[0];
            name = " ".join(entry_parts[1:]);

        move_id = get_move_id(move_data, extra_move_ids, name);

        if move_id != None:

            output = output + lvl + MAIN_DELIMETER + move_id + SECONDARY_DELIMETER;

        else:

            not_found.append(name);

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

def main_individual(filename, extra_move_ids):

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

        input_data = input("Input> ").replace("*","");

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

def main_multiple(filename, extra_move_ids):

    #Assumes that data is in form:
    #Pokemon name (ignored)
    #Base moves
    #Level-up moves
    #Disc moves
    #Egg moves
    #Blank line (ignored)
    #(repeat)

    lines = [];

    print("End input by entering \"END\"");

    while True:

        x = input(">");
        if x == "END":
            break;
        else:
            lines.append(x);

    move_data = load_data(filename);

    output = "";
    not_found = [];

    for i in range(len(lines)):

        entry_index = i % 6;
        
        if entry_index in [0,5]:
            continue;
        elif entry_index in [1,3,4]:
            entry_output = parse_list(lines[i], move_data, extra_move_ids)
        elif entry_index == 2:
            entry_output = parse_level_up(lines[i], move_data, extra_move_ids)
        else:
            raise Exception("Unhandled entry_index");

        for x in entry_output[1]:
            not_found.append(x);

        if entry_index == 4: #Final one
            output = output + entry_output[0] + "\n";
        else:
            output = output + entry_output[0] + "\t";

    if len(not_found) > 0:
        print("Not Found:\n" + '\n'.join(sorted(list(set(not_found)))));
    print("Copying output...");
    pyperclip.copy(output);
    print("Output Copied");

def main():

    #Modes
    #0 - Individual
    #1 - Multiple

    mode = int(input("Mode> "));

    filename = input("Moves File> ");

    extra_move_ids = get_extra_move_ids_input();

    if mode == 0:
        main_individual(filename, extra_move_ids);
    elif mode == 1:
        main_multiple(filename, extra_move_ids);

if __name__ == "__main__":
    
    main();
