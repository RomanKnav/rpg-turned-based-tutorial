using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script put on empty BattleHandler object.
// creates instances of a given prefab.

public class BattleHandler : MonoBehaviour {

    // what are static instances? MAKES IT A SINGLETON!!!!!
    // a single, globally accessible object created from a class with static members (variables and methods)
    private static BattleHandler instance;

    public static BattleHandler GetInstance() {
        return instance;
    }

    // wtf are Character objs? the characters themselves
    [SerializeField] private Transform pfCharacter;     // HERE'S WHERE WE PUT THE "pfCharacter" PREFAB

    // not even used in here. Used in Character:
    public Texture2D playerSpritesheet;
    public Texture2D enemySpritesheet;

    // how are these assigned? via SpawnCharacter():
    private Character playerCharacter;             // these are PREFAB instances
    private Character playerCharacter2;

    private Character enemyCharacter;
    private Character activeCharacter;          // what determines current character
    private State state;

    // wtf does Busy state even mean? when a character is attacking
    private enum State {
        WaitingForPlayer,
        Busy,               // applied to both character and enemy
    }

    // MY OWN STUFF:
    private static List<Character> allCharacters = new List<Character>();
    private static List<Character> friendlies = new List<Character>();
    private int turnIndex = 0;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        // this causes the characters to not be drawn UNTIL game is initiated (crazy):
        playerCharacter = SpawnCharacter(true, -10);
        playerCharacter2 = SpawnCharacter(true, +10);

        // why this set to false? to set it as ENEMY:
        enemyCharacter = SpawnCharacter(false, 0);  

        friendlies.Add(playerCharacter);
        friendlies.Add(playerCharacter2);

        // THIS will determine turn-order:
        allCharacters.Add(playerCharacter);
        allCharacters.Add(enemyCharacter);
        allCharacters.Add(playerCharacter2); 

        DrawActiveCircle(playerCharacter);

        // initial character state:
        state = State.WaitingForPlayer;     
    }

    private void Update() {
        // player attack:
        if (state == State.WaitingForPlayer) {
            if (Input.GetKeyDown(KeyCode.Space)) {

                // if space is pressed down, player is "busy" and initiates attack:
                state = State.Busy;

                // where's this "Attack" function from? in Character.cs:
                playerCharacter.Attack(enemyCharacter, () => {
                    ChooseNextActiveCharacter(0);
                });
            }
        }
    }

    // REMEMBER: this used ONCE per character (returns one instance AT A TIME):
    private Character SpawnCharacter(bool isPlayerTeam, int vertPosition) {
        Vector3 position;

        // draw friendlies on LEFT:
        if (isPlayerTeam) {
            position = new Vector3(-50, vertPosition);     // positioned in relation to what? (drawn in vertical center)

        // draw enemies on RIGHT:
        } else {
            position = new Vector3(+50, vertPosition);
        }

        // this creates CLONES:
        Transform characterTransform = Instantiate(pfCharacter, position, Quaternion.identity);    

        // this component is a SCRIPT: 
        Character character = characterTransform.GetComponent<Character>();
        character.Setup(isPlayerTeam);

        return character;
    }

    // simply responsible for setting circle
    private void DrawActiveCircle(Character character) {
        if (activeCharacter != null) {
            activeCharacter.HideSelectionCircle();
        }
        activeCharacter = character;
        activeCharacter.ShowSelectionCircle();
    }

    // this runs AFTER the current character's turn is up:
    private void ChooseNextActiveCharacter(int currIndex) {
        // battle is indefinitely OVER:
        if (BattleOver()) {
            return;
        }

        // returns either currIndex + 1 or 0, depending on value of currIndex.
        static int nextIndex(int currIndex) {
            if (currIndex == allCharacters.Count) {
                return 0;
            }
            else {
                return currIndex + 1;
            }
        }

        // TODO:
        DrawActiveCircle(allCharacters[currIndex]);
        if (activeCharacter == playerCharacter || activeCharacter == playerCharacter2)
        {
            state = State.Busy;
        }

        // allCharacters is a list of pfCharacter CLONES (instances)

        // if player is currently active, switch it to the enemy:
        if (activeCharacter == playerCharacter) {
            DrawActiveCircle(enemyCharacter);
            state = State.Busy;         // enemy in progress of attacking
            
            // where is Attack function defined? in Character.cs:
            enemyCharacter.Attack(playerCharacter, () => {
                ChooseNextActiveCharacter(nextIndex(currIndex));        
                // AAHHHH RECURSION
            });
        }
        else if (activeCharacter == playerCharacter2) {
            DrawActiveCircle(enemyCharacter);
            state = State.Busy;           

            enemyCharacter.Attack(playerCharacter2, () => {
                ChooseNextActiveCharacter(nextIndex(currIndex));
            });

        } else {
            DrawActiveCircle(playerCharacter);
            state = State.WaitingForPlayer;
        }
    }

    // crazy ass way of assigning a bool (determines who won):
    private bool BattleOver() {
        if (playerCharacter.IsDead()) {
            // Player dead, enemy wins
            //CodeMonkey.CMDebug.TextPopupMouse("Enemy Wins!");
            BattleOverWindow.Show_Static("Enemy Wins!");
            return true;
        }
        if (enemyCharacter.IsDead()) {
            // Enemy dead, player wins
            //CodeMonkey.CMDebug.TextPopupMouse("Player Wins!");
            BattleOverWindow.Show_Static("Player Wins!");
            return true;
        }
        return false;
    }
}
