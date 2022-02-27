using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FreeRoaming.Menu.PlayerMenus.SettingsMenu
{
    [RequireComponent(typeof(Canvas))]
    public class CreditsCanvasController : MonoBehaviour
    {

        public const string sourcesResourcePath = "Data/sources";
        public const string tilesetCreditsResourcePath = "Data/tilesetCredits";

        public Button buttonBack;

        public Text textCredits;

        public void SetUp(SettingsMenuController menuController)
        {

            textCredits.text = GetCreditsText();

            buttonBack.onClick.AddListener(() =>
            {
                HideCanvas();
                menuController.SelectDefaultSelectedGameObject();
            });

            HideCanvas();

        }

        public static string GetCreditsText()
        {

            string output = "Credits for Pokemon Obsidian:\n";

            #region Introduction

            output += "\nOriginal games made by Game Freak, published by Nintendo\n";

            output += "\nMade with Unity\n";

            #endregion

            #region Sources File

            output += "\nSources:";

            string[][] entries = CSV.ReadCSVResource(sourcesResourcePath, true);

            foreach (string[] entry in entries)
            {

                string item = entry[0];
                string url = entry[1];
                string author = entry[2];

                output += $"\n{item}: {author} ({url})";

            }

            #endregion

            #region Tileset Credits

            output += "\n\nStart credits for tileset:";

            TextAsset tilesetCreditsAsset = Resources.Load(tilesetCreditsResourcePath) as TextAsset;

            string tilesetCreditsString = tilesetCreditsAsset.text;

            output += '\n' + tilesetCreditsString;

            output += "\n\nEnd credits for tileset";

            #endregion

            return output;

        }

        private void ShowCanvas() => GetComponent<Canvas>().enabled = true;
        private void HideCanvas() => GetComponent<Canvas>().enabled = false;

        public void Show()
        {

            ShowCanvas();
            EventSystem.current.SetSelectedGameObject(buttonBack.gameObject);

        }

    }
}