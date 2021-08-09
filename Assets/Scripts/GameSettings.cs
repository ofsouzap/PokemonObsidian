using System;
using System.Collections.Generic;
using System.Linq;

public class GameSettings
{

    public static GameSettings singleton = new GameSettings();

    #region General

    public float musicVolume = 0.5f;
    public float sfxVolume = 1;

    #endregion

    #region Text Speed

    public static readonly TextSpeed[] textSpeedOptions = new TextSpeed[]
    {
        new TextSpeed("Fast", 0.01F),
        new TextSpeed("Very Fast", 0),
        new TextSpeed("Medium", 0.05F),
        new TextSpeed("Slow", 0.1F)
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

    private int textSpeedIndex = 1;

    public TextSpeed textSpeed
    {
        get
        {
            return textSpeedOptions[textSpeedIndex];
        }
        set
        {
            textSpeedIndex = Array.IndexOf(textSpeedOptions, value);
        }
    }

    #endregion

}
