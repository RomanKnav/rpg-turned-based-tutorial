/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Testing : MonoBehaviour {

    private void Start() {
        //DamagePopup.Create(Vector3.zero, 300);
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            bool isCriticalHit = Random.Range(0, 100) < 30;
            //DamagePopup.Create(UtilsClass.GetMouseWorldPosition(), 100, isCriticalHit);
        }
    }
}
