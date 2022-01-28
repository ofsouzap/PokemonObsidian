using System;
using Serialization;

namespace Battle
{
    /// <summary>
    /// A subclass of the player battle participant for the player battle participant when the player is battling against a network opponent and so must send its actions to the other machine
    /// </summary>
    public class BattleParticipantNetworkedPlayer : BattleParticipantPlayer
    {

        public Serializer Serializer => battleManager.Serializer;

        /// <summary>
        /// The function to be run when this player has chosen their action for this turn to let it be sent to the other paticipant's machine (probably using the established network stream)
        /// </summary>
        protected Action<Action> publishChosenActionAction;

        public void SetPublishChosenActionAction(Action<Action> action)
            => publishChosenActionAction = action;

        public override void SetChosenAction(Action action)
        {

            base.SetChosenAction(action);

            publishChosenActionAction?.Invoke(action);

        }

    }
}
