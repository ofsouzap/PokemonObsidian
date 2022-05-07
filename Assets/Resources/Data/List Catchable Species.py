import csv;

DEFAULT_SPECIES_FN = "pokemonSpecies.csv";
DEFAULT_WILD_AREA_FN = "wildPokemonAreas.csv";

def load_csv(fn):
    with open(fn) as f:
        r = csv.reader(f);
        ds = list(r);
    return ds;

def load_wild_area_species(fn):

    ds = load_csv(fn);

    species = set();

    for d in ds[1:]:
        for x in d[3].split(";"):
            species.add(int(x));

    return species;

def load_species(fn):

    ds = load_csv(fn);

    species_names = {};

    for d in ds[1:]:
        species_names[int(d[0])] = d[1];

    return species_names;

def main():

    global MAX_ID;

    species_fn = input("Species CSV Filename> ");

    if not species_fn:
        species_fn = DEFAULT_SPECIES_FN;
        wild_area_fn = DEFAULT_WILD_AREA_FN;
    else:
        wild_area_fn = input("Wild Area CSV Filename> ");
    
    species_names = load_species(species_fn);
    used_species = tuple(sorted(load_wild_area_species(wild_area_fn)));

    # Used Species

    print("Used Species:");
    for s in used_species:
        print(f"{s} - {species_names[s]}");

    # Unused Species

    print("-" * 20);
    print("Unused Species:");
    for i in sorted(species_names.keys()):
        if not (i in used_species):
            print(f"{i} - {species_names[i]}");

    input("Press ENTER to continue...");

if __name__ == "__main__":
    main();
