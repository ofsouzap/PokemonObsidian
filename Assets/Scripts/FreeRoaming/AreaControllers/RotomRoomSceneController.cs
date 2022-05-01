using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Pokemon;
using FreeRoaming.Decorations;
using Audio;

namespace FreeRoaming.AreaControllers
{
    public class RotomRoomSceneController : FreeRoamSceneController
    {

        public const string rotomRoomSceneIdentifier = "Rotom Room";

        public const string battleMusicName = BattleEntranceArguments.defaultPokemonBattleMusicName;

        #region Rotom

        /// <summary>
        /// The messages displayed if the player completes the puzzle but has already caught rotom
        /// </summary>
        public static readonly string[] noEncounterPuzzleCompletionMessages = new string[]
        {
            "You hear something move...",
            "The exit suddenly appears."
        };

        /// <summary>
        /// The messages displayed if the player completes the puzzle and is going to start the rotom encounter
        /// </summary>
        public static readonly string[] rotomEncounterMessages = new string[]
        {
            "You feel a strange tingling in your spine...",
            "You turn around slowly...",
            "There's nothing there.",
            "You turn back around...",
            "A pokemon attacks you!"
        };

        public const string rotomBattleBackgroundResourceName = "indoor";

        public const int rotomSpeciesId = 479;

        public const int rotomLevel = 10;

        public static readonly PokemonInstance.BasicSpecification rotomSpecification = new PokemonInstance.BasicSpecification()
        {
            gender = PokemonInstance.BasicSpecification.Gender.Genderless,
            speciesId = rotomSpeciesId,
            useAutomaticMoves = true,
            level = rotomLevel,
            EVs = new Stats<byte>()
            {
                attack = 0,
                defense = 0,
                specialAttack = 0,
                specialDefense = 0,
                speed = 0,
                health = 0,
            },
            useRandomIVs = true,
        };

        public static PokemonInstance GenerateRotomInstance()
            => PokemonFactory.GenerateFromBasicSpecification(rotomSpecification);

        #endregion

        public GameObject exitDoor;
        public GameObject exitDoorWall;

        [SerializeField]
        private float leftXPos;

        [SerializeField]
        private float rightXPos;

        [SerializeField]
        private RotomRoomBookshelfController[] bookshelves;

        /// <summary>
        /// Which side each bookshelf is on. Right is true, left is false.
        /// </summary>
        private bool[] bookshelfStates;

        public const int bookshelfCount = 4;

        private bool puzzleEnabled;

        /// <summary>
        /// Which bookshelves each bookshelf affects. Keys are ids of bookshelves and values are ids of bookshelves that are affected by the bookshelf
        /// </summary>
        public static readonly Dictionary<int, int[]> bookshelfAffectors = new Dictionary<int, int[]>()
        {
            { 0, new int[] { 1, 3 } },
            { 1, new int[] { 0, 2, 3 } },
            { 2, new int[] { 3 } },
            { 3, new int[] { 0, 1 } }
        };

        public static readonly bool[] defaultBookshelfStates = new bool[bookshelfCount]
        {
            true,
            false,
            false,
            true
        };

        public static readonly bool[] goalBookshelfStates = new bool[bookshelfCount]
        {
            false,
            true,
            true,
            false
        };

        private bool PuzzleCompleted
        {
            get
            {

                for (int i = 0; i < bookshelfStates.Length; i++)
                    if (bookshelfStates[i] != goalBookshelfStates[i])
                        return false;

                return true;

            }
        }

        private void OpenExit()
        {
            exitDoor.SetActive(true);
            exitDoorWall.SetActive(false);
        }

        private void CloseExit()
        {
            exitDoor.SetActive(false);
            exitDoorWall.SetActive(true);
        }

        private void SetBookshelfInteractActions()
        {

            for (int i = 0; i < bookshelves.Length; i++)
            {
                int currBookshelfIndex = i;
                bookshelves[i].InteractAction = () => OnBookshelfInteract(currBookshelfIndex);
            }

        }

        protected override void Start()
        {

            base.Start();

            puzzleEnabled = true;

            if (bookshelves.Length != bookshelfCount)
                Debug.LogError("Incorrect number of bookshelves provided");

            SetBookshelfInteractActions();

            SetBookshelfStates(defaultBookshelfStates);

            //When player first enters room, exit door isn't available
            CloseExit();

        }

        private void OnBookshelfInteract(int bookshelfId)
        {

            if (puzzleEnabled)
                StartCoroutine(BookshelfUsedCoroutine(bookshelfId));

        }

        /// <summary>
        /// Gets an indexed bookshelf's position that it should be in based on its state (ie either the left or right position)
        /// </summary>
        private Vector2 GetBookshelfIntendedPosition(int index)
        {

            bool state = bookshelfStates[index];
            RotomRoomBookshelfController controller = bookshelves[index];

            return new Vector2(
                state ? rightXPos : leftXPos, //X-position is left or right depending on the bookshelf's state in the puzzle
                controller.transform.position.y); //Y-position is unaltered

        }

        /// <summary>
        /// Puts the bookshelves in their correct positions
        /// </summary>
        private void RefreshBookshelfPositions()
        {

            for (int i = 0; i < bookshelves.Length; i++)
                bookshelves[i].transform.position = GetBookshelfIntendedPosition(i);

        }

        private void SetBookshelfStates(bool[] states)
        {

            bookshelfStates = new bool[states.Length];
            Array.Copy(states, bookshelfStates, states.Length);

            RefreshBookshelfPositions();

        }

        private IEnumerator BookshelfUsedCoroutine(int usedBookshelfIndex)
        {

            //Pause scene
            SetSceneRunningState(false);

            int[] targetBookshelves = bookshelfAffectors[usedBookshelfIndex];

            //Logically move the bookshelves
            foreach (int otherIndex in targetBookshelves)
                bookshelfStates[otherIndex] = !bookshelfStates[otherIndex];

            //Physically move the bookshelves
            yield return StartCoroutine(MoveBookshelvesAnimation(targetBookshelves));

            //Check if puzzle completed and, if so, start battle, otherwise, continue
            if (PuzzleCompleted)
                StartCoroutine(PuzzleCompletionCoroutine());
            else
            {
                //Resume scene
                SetSceneRunningState(true);
            }

        }

        private IEnumerator PuzzleCompletionCoroutine()
        {

            //(Assume scene already paused)

            //Disable puzzle
            puzzleEnabled = false;

            //Check if rotom already caught
            bool rotomCaught = PlayerData.singleton.GetProgressionEventCompleted(ProgressionEvent.RotomRoom_RotomCaught);

            //Show text box for messages
            textBoxController.Show();

            if (!rotomCaught) //If rotom not already caught, battle it
            {

                foreach (string m in rotomEncounterMessages)
                {
                    yield return StartCoroutine(
                        textBoxController.RevealText(m, true)
                    );
                }

                StartBattle();

            }
            else //If rotom already caught, just reveal the exit door
            {

                foreach (string m in noEncounterPuzzleCompletionMessages)
                {
                    yield return StartCoroutine(
                        textBoxController.RevealText(m, true)
                    );
                }

                //Unpause scene
                SetSceneRunningState(true);

            }

            //Hide text box
            textBoxController.Hide();

            //Show the exit
            OpenExit();

        }

        private void StartBattle()
        {

            SetBattleEntranceArguments();

            //Set so that, if rotom caught, mark in player progression events
            BattleManager.OnBattleEnd += (info) =>
            {
                if (info.outcome == BattleManager.BattleOutcome.CatchOpponent)
                {
                    PlayerData.singleton.AddCompletedProgressionEvent(ProgressionEvent.RotomRoom_RotomCaught);
                }
            };

            MusicSourceController.singleton.SetTrack(battleMusicName, true);

            GameSceneManager.LaunchBattleScene();

        }

        private void SetBattleEntranceArguments()
        {

            BattleEntranceArguments.argumentsSet = true;

            BattleEntranceArguments.initialWeatherId = 0;
            BattleEntranceArguments.battleBackgroundResourceName = rotomBattleBackgroundResourceName;

            BattleEntranceArguments.battleType = BattleType.WildPokemon;
            BattleEntranceArguments.wildPokemonBattleArguments = new BattleEntranceArguments.WildPokemonBattleArguments()
            {
                opponentInstance = rotomSpecification.Generate()
            };

        }

        private const float moveAnimationTime = 1;

        private IEnumerator MoveBookshelvesAnimation(int[] indexes)
        {

            float startTime = Time.time;

            Vector2[] startPositions = indexes
                .Select(i => (Vector2)bookshelves[i].transform.position)
                .ToArray();

            Vector2[] endPositions = indexes
                .Select(i => GetBookshelfIntendedPosition(i))
                .ToArray();
            
            while (Time.time < startTime + moveAnimationTime)
            {

                float t = (Time.time - startTime) / moveAnimationTime;

                //Move each bookshelf to its interpolated position
                for (int i = 0; i < indexes.Length; i++)
                {

                    int bookshelfIndex = indexes[i];
                    bookshelves[bookshelfIndex].transform.position = Vector2.Lerp(
                        startPositions[i],
                        endPositions[i],
                        t);

                }

                yield return new WaitForEndOfFrame();

            }

            //To make sure positions are correct
            RefreshBookshelfPositions();

        }

    }
}