using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// script is put on: UI/Canvas/BattleOverWindow
public class BattleOverWindow : MonoBehaviour {

    private static BattleOverWindow instance;

    private void Awake() {
        instance = this;
        // initialled deactivated:
        Hide();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show(string winnerString) {
        gameObject.SetActive(true);

        transform.Find("winnerText").GetComponent<Text>().text = winnerString;
    }

    public static void Show_Static(string winnerString) {
        instance.Show(winnerString);
    }
}
