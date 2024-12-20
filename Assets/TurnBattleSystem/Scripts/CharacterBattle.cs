﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;     // wtf is this??? in Assets/_/Stuff/CodeMonkey/Utils

// what this script do? Responsible for all the ANIMATION and texture-setting of the characters.

// script placed on: pfCharacter prefab (ON PREFAB ITSELF)
// TWO scripts placed on the prefab. This one, and Character_Base.cs.
public class Character : MonoBehaviour {

    private Character_Base characterBase;
    private State state;
    private Vector3 slideTargetPosition;
    private Action onSlideComplete;
    private bool isPlayerTeam;
    private GameObject selectionCircleGameObject;
    private HealthSystem healthSystem;
    private World_Bar healthBar;

    private enum State {
        Idle,
        Sliding,
        Busy,
    }

    private void Awake() {
        characterBase = GetComponent<Character_Base>(); // this is a component??? yes, it's the script!!!

        selectionCircleGameObject = transform.Find("SelectionCircle").gameObject;
        HideSelectionCircle();
        state = State.Idle;
    }

    private void Start() {
        // Vector3's are for 3D games! why being used here?
        // characterBase.PlayAnimMove(new Vector3(1, 0));
    }

    public void Setup(bool isPlayerTeam) {
        this.isPlayerTeam = isPlayerTeam;
        if (isPlayerTeam) {
            // this animation basically just puts the sword on their back when idle:
            characterBase.SetAnimsSwordTwoHandedBack();

            // set the character's spritesheet!
            characterBase.GetMaterial().mainTexture = BattleHandler.GetInstance().playerSpritesheet;
        } else {
            // set shield animation for enemy:
            // characterBase.SetAnimsSwordShield();
            characterBase.SetAnimsSwordTwoHandedBack();
            characterBase.GetMaterial().mainTexture = BattleHandler.GetInstance().enemySpritesheet;
        }
        healthSystem = new HealthSystem(100);
        healthBar = new World_Bar(transform, new Vector3(0, 10), new Vector3(12, 1.7f), Color.grey, Color.red, 1f, 100, new World_Bar.Outline { color = Color.black, size = .6f });
        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;

        PlayAnimIdle();
    }

    private void HealthSystem_OnHealthChanged(object sender, EventArgs e) {
        healthBar.SetSize(healthSystem.GetHealthPercent());
    }

    private void PlayAnimIdle() {
        if (isPlayerTeam) {
            characterBase.PlayAnimIdle(new Vector3(+1, 0));
        } else {
            characterBase.PlayAnimIdle(new Vector3(-1, 0));
        }
    }

    private void Update() {
        switch (state) {
        case State.Idle:
            break;
        case State.Busy:
            break;
        case State.Sliding:
            float slideSpeed = 10f;
            transform.position += (slideTargetPosition - GetPosition()) * slideSpeed * Time.deltaTime;

            float reachedDistance = 1f;
            if (Vector3.Distance(GetPosition(), slideTargetPosition) < reachedDistance) {
                // Arrived at Slide Target Position
                //transform.position = slideTargetPosition;
                onSlideComplete();
            }
            break;
        }
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    public void Damage(Character attacker, int damageAmount) {
        healthSystem.Damage(damageAmount);
        Vector3 dirFromAttacker = (GetPosition() - attacker.GetPosition()).normalized;

        characterBase.SetColorTint(new Color(1, 0, 0, 1f));
        Blood_Handler.SpawnBlood(GetPosition(), dirFromAttacker);

        CodeMonkey.Utils.UtilsClass.ShakeCamera(1f, .1f);

        if (healthSystem.IsDead()) {
            // Died
            characterBase.PlayAnimLyingUp();
        }
    }

    public bool IsDead() {
        return healthSystem.IsDead();
    }

    // passed object to attack, and action to take after

    // the script placed on Character objs is:
    // targetCharacter is a Character obj, and onAttackComplete is a callback after done
    public void Attack(Character targetCharacter, Action onAttackComplete) {

        // how is this a Vector3?
        Vector3 slideTargetPosition = targetCharacter.GetPosition() + (GetPosition() - targetCharacter.GetPosition()).normalized * 10f;
        Vector3 startingPosition = GetPosition();

        // Slide to Target
        SlideToPosition(slideTargetPosition, () => {
            // Arrived at Target, attack him
            state = State.Busy;
            Vector3 attackDir = (targetCharacter.GetPosition() - GetPosition()).normalized;
            characterBase.PlayAnimAttack(attackDir, () => {
                // Target hit
                int damageAmount = UnityEngine.Random.Range(20, 50);
                targetCharacter.Damage(this, damageAmount);
                }, () => {
                // Attack completed, slide back
                SlideToPosition(startingPosition, () => {
                    // Slide back completed, back to idle
                    state = State.Idle;
                    characterBase.PlayAnimIdle(attackDir);

                    // the only code that runs the callback:
                    onAttackComplete();
                });
            });
        });
    }

    private void SlideToPosition(Vector3 slideTargetPosition, Action onSlideComplete) {
        this.slideTargetPosition = slideTargetPosition;
        this.onSlideComplete = onSlideComplete;
        state = State.Sliding;
        if (slideTargetPosition.x > 0) {
            characterBase.PlayAnimSlideRight();
        } else {
            characterBase.PlayAnimSlideLeft();
        }
    }

    public void HideSelectionCircle() {
        selectionCircleGameObject.SetActive(false);
    }

    public void ShowSelectionCircle() { 
        selectionCircleGameObject.SetActive(true);
    }

}
