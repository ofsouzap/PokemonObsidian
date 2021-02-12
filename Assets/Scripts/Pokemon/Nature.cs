using System.Linq;
using Pokemon;

namespace Pokemon {

    public struct Nature
    {

        public int id;
        public string name;
        public Stats<bool?> boosts;

        public Nature getNatureById(int id)
        {
            return registry.First((x) => x.id == id);
        }

        public Nature getNatureByName(string name)
        {
            return registry.First((x) => x.name == name);
        }

        private static Nature[] _registry;
        public static Nature[] registry
        {
            get
            {

                if (registry == null)
                {
                    LoadRegistry();
                }

                return _registry;

            }
        }

        public static void LoadRegistry()
        {

            //TODO - load natures from a file

        }

    }

}
