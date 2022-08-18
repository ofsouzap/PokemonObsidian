public enum ProgressionEvent
{

    /* 
     * N.B. events (enumerator values) created here should NEVER be deleted to maintain save data integrity.
     * Instead, if an event must be removed/marked to not be used, it could be renamed to something showing its obsoleteness
    */

    /// <summary>
    /// The default value, here to help find/reduce game errors: if something accidentally defaults to this value, it shouldn't affect the player's game
    /// </summary>
    None,

    /// <summary>
    /// The player has caught the rotom in the rotom room meaning that it shouldn't appear anymore so the player can't catch it multiple times.
    /// </summary>
    RotomRoom_RotomCaught,

    /// <summary>
    /// The player has started the ice side quest in Chillborough by talking to the relevant NPC
    /// </summary>
    ChillboroughIceSideQuest_Started,

    /// <summary>
    /// The player has found the culprit for the ice side quest in Chillborough by talking to them and listening to their confession
    /// </summary>
    ChillboroughIceSideQuest_CulpritFound,

    /// <summary>
    /// The player has reported back to the ice side quest quest-giver and collected their reward
    /// </summary>
    ChillboroughIceSideQuest_Completed

}