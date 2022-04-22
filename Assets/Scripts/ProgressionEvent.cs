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

}