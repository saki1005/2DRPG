using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    // HPの増減の描画をする
    [SerializeField] GameObject health;

    

    public void SetHP(float hp)
    {
        health.transform.localScale = new Vector3(hp, 1, 1);
    }

    public IEnumerator SetHPSmooth(float newHP)
    {
        float currentHP = health.transform.localScale.x;
        float changeAmount = currentHP - newHP;

        // currentHP と newHP に差があるなら繰り返す
        while (currentHP - newHP > Mathf.Epsilon)
        {
            currentHP -= changeAmount * Time.deltaTime;
            health.transform.localScale = new Vector3(currentHP, 1, 1);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHP, 1, 1);
    }
}
