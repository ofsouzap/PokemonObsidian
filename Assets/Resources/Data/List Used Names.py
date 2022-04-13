import csv;

def load_csv(fn):
    with open(fn) as f:
        r = csv.reader(f);
        ds = list(r);
    return ds;

def load_csv_column(fn, i):

    ds = load_csv(fn);

    outs = [d[i] for d in ds[1:]];

    return outs;

def main():

    global MAX_ID;

    generic_npcs_fn = input("Generic NPCs CSV Filename> ");
    trainers_fn = input("Trainers CSV Filename> ");

    names_used = [];
    
    for n in load_csv_column(generic_npcs_fn, 1): names_used.append(n);
    for n in load_csv_column(trainers_fn, 2): names_used.append(n);

    print("Names Used:");
    for n in sorted(names_used):
        print(n);

    input("Press ENTER to continue...");

if __name__ == "__main__":
    main();
