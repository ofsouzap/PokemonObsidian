using System;
using System.Collections.Generic;
using System.Linq;

public static class GameSettings
{

    #region General

    #endregion

    #region Text Speed

    public static readonly TextSpeed[] textSpeedOptions = new TextSpeed[]
    {
        new TextSpeed("Fast", 0.01F),
        new TextSpeed("Very Fast", 0),
        new TextSpeed("Medium", 0.03F),
        new TextSpeed("Slow", 0.6F)
    };

    public struct TextSpeed
    {

        /// <summary>
        /// A name to present the user with for the text speed setting
        /// </summary>
        public string name;

        /// <summary>
        /// The time (in seconds) to delay between showing each character
        /// </summary>
        public float characterDelay;

        public TextSpeed(string name, float characterDelay)
        {
            this.name = name;
            this.characterDelay = characterDelay;
        }
    }

    public static TextSpeed textSpeed = textSpeedOptions[0];

    #endregion

}
