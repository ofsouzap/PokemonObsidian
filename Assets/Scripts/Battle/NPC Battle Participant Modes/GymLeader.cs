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

        protected int healingItemId;
        protected int maxTimesHealed;

        protected int timesHealed;

        /// <summary>
        /// The id of the item that the gym leader should use to heal their pokemon by default
        /// </summary>
        public const int defaultHealingItemId = 25; //Hyper potion
        /// <summary>
        /// How many times the gym leader can heal their by default
        /// </summary>
        public const int defaultMaxTimesHealed = 3;

        public Item HealingItem => Item.GetItemById(healingItemId);

        public GymLeader(TrainersData.TrainerDetails details) : base(details)
        {

            healingItemId = details.leaderHealingItemId;
            maxTimesHealed = details.leaderMaxTimesHealed;

            timesHealed = 0;

        }

        protected override void ChooseAction(BattleData battleData)
        {

            if (actionHasBeenChosen)
                return;

            if (ActivePokemon.HealthProportion <= healingHealthThreshold)
            {

                if (timesHealed < defaultMaxTimesHealed)
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
