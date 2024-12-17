using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script put on empty BattleHandler object.
// this script is PASSED the prefab instances of the player and enemy. 

public class BattleHandler : MonoBehaviour {

    private static BattleHandler instance;

    public static BattleHandler GetInstance() {
        return instance;
    }

    // wtf are CharacterBattle objs? the characters themselves
    [SerializeField] private Transform pfCharacterBattle;     // HERE'S WHERE WE PUT THE "pfCharacterBattle" PREFAB
    public Texture2D playerSpritesheet;
    public Texture2D enemySpritesheet;

    // how are these assigned?
    private CharacterBattle playerCharacterBattle;             // these are PREFAB instances
    private CharacterBattle enemyCharacterBattle;
    private CharacterBattle activeCharacterBattle;
    private State state;

    private enum State {
        WaitingForPlayer,
        Busy,
    }

    private void Awake() {
        instance = this;
    }

    private void Start() {
        // this causes the characters to not be drawn UNTIL game is initiated:
        playerCharacterBattle = SpawnCharacter(true);
        enemyCharacterBattle = SpawnCharacter(false);

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

    private CharacterBattle SpawnCharacter(bool isPlayerTeam) {
        Vector3 position;
        if (isPlayerTeam) {
            position = new Vector3(-50, 0);     // positioned in relation to what? (drawn in vertical center)
        } else {
            position = new Vector3(+50, 0);
        }
        // how to position objects on-screen via code:
        Transform characterTransform = Instantiate(pfCharacterBattle, position, Quaternion.identity);     
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
                ChooseNextActiveCharacter();
            });
        } else {
            SetActiveCharacterBattle(playerCharacterBattle);
            state = State.WaitingForPlayer;
        }
    }

    private bool TestBattleOver() {
        if (playerCharacterBattle.IsDead()) {
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
