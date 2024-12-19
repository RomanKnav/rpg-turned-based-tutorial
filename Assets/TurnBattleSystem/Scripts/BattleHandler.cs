using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script put on empty BattleHandler object.
// this script is PASSED the prefab instances of the player and enemy. 

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

    private enum State {
        WaitingForPlayer,
        Busy,
    }

    // MY OWN STUFF:
    private static List<Character> friendlies = new List<Character>();

    private void Awake() {
        instance = this;
    }

    private void Start() {
        // this causes the characters to not be drawn UNTIL game is initiated (crazy):
        playerCharacter = SpawnCharacter(true, -10);
        playerCharacter2 = SpawnCharacter(true, +10);

        friendlies.Add(playerCharacter);
        friendlies.Add(playerCharacter2);

        // why this set to false? to set it as ENEMY:
        enemyCharacter = SpawnCharacter(false, 0);       

        SetActiveCharacter(playerCharacter);

        // initial character state:
        state = State.WaitingForPlayer;     
    }

    private void Update() {
        if (state == State.WaitingForPlayer) {
            if (Input.GetKeyDown(KeyCode.Space)) {

                // if space is pressed down, player is "busy" and initiates attack:
                state = State.Busy;

                // where's this "Attack" function from? in Character.cs:
                playerCharacter.Attack(enemyCharacter, () => {
                    ChooseNextActiveCharacter();
                });
            }
        }
    }

    // REMEMBER: this used ONCE per character (returns one instance AT A TIME):
    private Character SpawnCharacter(bool isPlayerTeam, int vertPosition) {
        Vector3 position;

        if (isPlayerTeam) {
            position = new Vector3(-50, vertPosition);     // positioned in relation to what? (drawn in vertical center)
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

    private void SetActiveCharacter(Character character) {
        if (activeCharacter != null) {
            activeCharacter.HideSelectionCircle();
        }

        activeCharacter = character;
        activeCharacter.ShowSelectionCircle();
    }

    private void ChooseNextActiveCharacter() {
        if (BattleOver()) {
            return;
        }

        // if player is currently active, switch it to the enemy:
        if (activeCharacter == playerCharacter) {
            SetActiveCharacter(enemyCharacter);
            state = State.Busy;
            
            // where is Attack function defined? in Character.cs:
            enemyCharacter.Attack(playerCharacter, () => {
                ChooseNextActiveCharacter();        
                // AAHHHH RECURSION
            });
        }
        else if (activeCharacter == playerCharacter2) {
            SetActiveCharacter(enemyCharacter);
            state = State.Busy;

            enemyCharacter.Attack(playerCharacter2, () => {
                ChooseNextActiveCharacter();
            });

        } else {
            SetActiveCharacter(playerCharacter);
            state = State.WaitingForPlayer;
        }
    }

    // crazy ass way of assigning a bool (determines who won):
    private bool BattleOver() {
        if (playerCharacter.IsDead() && playerCharacter2.IsDead()) {
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
