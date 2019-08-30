using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamagePopUp : MonoBehaviour{

    private TextMeshPro textMesh;
    private float disappearTimer = 1;
    private Color textColor;

    public static DamagePopUp Create(Vector3 position, int damageAmount) {
        Transform dmgTranform = Instantiate(GameAssets.i.pfDamagePopUp, position, Quaternion.identity);
        DamagePopUp damagePopUp = dmgTranform.GetComponent<DamagePopUp>();
        damagePopUp.Setup(damageAmount);
        return damagePopUp;
    }

    private void Awake() {
        textMesh = transform.GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount) {
        textMesh.SetText(damageAmount.ToString());
        textColor = textMesh.color;
    }

    private void Update() {
        float moveYSpeed = 0.8f;
        transform.position += new Vector3(0, moveYSpeed, 0) * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0) {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor. a < 0) {
                Destroy(gameObject);
            }
        }
    }

}
