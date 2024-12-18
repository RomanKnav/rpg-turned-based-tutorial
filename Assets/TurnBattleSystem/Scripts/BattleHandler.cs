using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script put on empty BattleHandler object.
// this script is PASSED the prefab instances of the player and enemy. 

public class BattleHandler : MonoBehaviour {

    // what are static instances? MAKES IT A FUCKING SINGLETON!!!!!
    // a single, globally accessible object created from a class with static members (variables and methods)
    private static BattleHandler instance;

    public static BattleHandler GetInstance() {
        return instance;
    }

    // wtf are CharacterBattle objs? the characters themselves
    [SerializeField] private Transform pfCharacterBattle;     // HERE'S WHERE WE PUT THE "pfCharacterBattle" PREFAB

    // shit not even used in here. Used in CharacterBattle:
    public Texture2D playerSpritesheet;
    public Texture2D enemySpritesheet;

    // how are these assigned? via SpawnCharacter():
    private CharacterBattle playerCharacterBattle;             // these are PREFAB instances
    private CharacterBattle playerCharacterBattle2;


    private CharacterBattle enemyCharacterBattle;
    private CharacterBattle activeCharacterBattle;          // what determines current character
    private State state;

    private enum State {
        WaitingForPlayer,
        Busy,
    }

    // MY OWN CRAP:
    // 
    private static List<CharacterBattle> friendlies = new List<CharacterBattle>();

    private void Awake() {
        instance = this;
    }

    private void Start() {
        // this causes the characters to not be drawn UNTIL game is initiated (fucking crazy):
        playerCharacterBattle = SpawnCharacter(true, -10);
        playerCharacterBattle2 = SpawnCharacter(true, +10);

        friendlies.Add(playerCharacterBattle);
        friendlies.Add(playerCharacterBattle2);

        // why this set to false? to set it as ENEMY:
        enemyCharacterBattle = SpawnCharacter(false, 0);       

        SetActiveCharacterBattle(playerCharacterBattle);

        // initial character state:
        state = State.WaitingForPlayer;     
    }

    private void Update() {
        if (state == State.WaitingForPlayer) {
            if (Input.GetKeyDown(KeyCode.Space)) {

                // if space is pressed down, player is "busy" and initiates attack:
                state = State.Busy;

                // where's this "Attack" function from? in CharacterBattle.cs:
                playerCharacterBattle.Attack(enemyCharacterBattle, () => {
                    ChooseNextActiveCharacter();
                });
            }
        }
    }

    // REMEMBER: this used ONCE per character (returns one instance AT A TIME):
    private CharacterBattle SpawnCharacter(bool isPlayerTeam, int vertPosition) {
        Vector3 position;

        if (isPlayerTeam) {
            position = new Vector3(-50, vertPosition);     // positioned in relation to what? (drawn in vertical center)
        } else {
            position = new Vector3(+50, vertPosition);
        }

        // this creates fucking CLONES:
        Transform characterTransform = Instantiate(pfCharacterBattle, position, Quaternion.identity);    

        // this component is a SCRIPT: 
        CharacterBattle characterBattle = characterTransform.GetComponent<CharacterBattle>();
        characterBattle.Setup(isPlayerTeam);

        return characterBattle;
    }

    private void SetActiveCharacterBattle(CharacterBattle characterBattle) {
        if (activeCharacterBattle != null) {
            activeCharacterBattle.HideSelectionCircle();
        }

        activeCharacterBattle = characterBattle;
        activeCharacterBattle.ShowSelectionCircle();
    }

    private void ChooseNextActiveCharacter() {
        if (TestBattleOver()) {
            return;
        }

        if (activeCharacterBattle == playerCharacterBattle) {
            SetActiveCharacterBattle(enemyCharacterBattle);
            state = State.Busy;
            
            enemyCharacterBattle.Attack(playerCharacterBattle, () => {
                ChooseNextActiveCharacter();        // AAHHHH RECURSION
            });
        }
        else if (activeCharacterBattle == playerCharacterBattle2) {
            SetActiveCharacterBattle(enemyCharacterBattle);
            state = State.Busy;

            enemyCharacterBattle.Attack(playerCharacterBattle2, () => {
                ChooseNextActiveCharacter();
            });

        } else {
            SetActiveCharacterBattle(playerCharacterBattle);
            state = State.WaitingForPlayer;
        }
    }

    private bool TestBattleOver() {
        if (playerCharacterBattle.IsDead() && playerCharacterBattle2.IsDead()) {
            // Player dead, enemy wins
            //CodeMonkey.CMDebug.TextPopupMouse("Enemy Wins!");
            BattleOverWindow.Show_Static("Enemy Wins!");
            return true;
        }
        if (enemyCharacterBattle.IsDead()) {
            // Enemy dead, player wins
            //CodeMonkey.CMDebug.TextPopupMouse("Player Wins!");
            BattleOverWindow.Show_Static("Player Wins!");
            return true;
        }
        return false;
    }
}
