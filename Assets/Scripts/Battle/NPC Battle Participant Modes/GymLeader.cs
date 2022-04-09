using Pokemon;
using Items;

namespace Battle.NPCBattleParticipantModes
{
    public class GymLeader : BasicTrainer
    {

        /// <summary>
        /// If this leader's active pokemon's health is below this threshold then they will try to heal the pokemon
        /// </summary>
        protected const float healingHealthThreshold = 0.2F;

        protected int timesHealed;

        /// <summary>
        /// The id of the item that the gym leader should use to heal their pokemon
        /// </summary>
        public const int healingItemId = 25; //Hyper potion
        public Item HealingItem => Item.GetItemById(healingItemId);

        /// <summary>
        /// How many times the gym leader can heal their pokemon
        /// </summary>
        public const int maxTimesHealed = 5;

        public GymLeader(string name,
            PokemonInstance[] pokemon,
            byte basePayout,
            string[] defeatMessages) : base(name, pokemon, basePayout, defeatMessages)
        {

            timesHealed = 0;

        }

        protected override void ChooseAction(BattleData battleData)
        {

            if (actionHasBeenChosen)
                return;

            if (ActivePokemon.HealthProportion <= healingHealthThreshold)
            {

                if (timesHealed < maxTimesHealed)
                {
                    SetActionToHeal();
                    return;
                }

            }

            base.ChooseAction(battleData);

        }

        protected void SetActionToHeal()
        {

            SetChosenAction(new Action(this)
            {
                type = Action.Type.UseItem,
                useItemItemToUse = HealingItem,
                useItemTargetPartyIndex = activePokemonIndex
            });

            timesHealed++;

        }

    }
}
