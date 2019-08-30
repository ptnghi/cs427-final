using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour{


    [SerializeField]
    private Image foreGroundImage;

    [SerializeField]
    private Text healthDisplay;

    private float updateSpeedSeconds = 0.2f;

    public event Action OnHealthUpdateDone = delegate { };

    private void Awake() {
        GetComponentInParent<Unit>().OnHealthChange += HandleHealthChange;
    }

    private void HandleHealthChange(int currHealth, int maxHealth) {
        StartCoroutine(UpdateHealthBar(currHealth, maxHealth));
    }

    private IEnumerator UpdateHealthBar (int currHealth, int maxHealth) {
        float currPct = currHealth * 1.0f / maxHealth * 1.0f;
        float preChange = foreGroundImage.fillAmount;
        float elapse = 0f;

        while (elapse < updateSpeedSeconds) {
            elapse += Time.deltaTime;
            foreGroundImage.fillAmount = Mathf.Lerp(preChange, currPct, elapse / updateSpeedSeconds);
            yield return null;
        }

        foreGroundImage.fillAmount = currPct;
        healthDisplay.text = currHealth + "/" + maxHealth;
        OnHealthUpdateDone();
    }

    private void LateUpdate() {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }
}
