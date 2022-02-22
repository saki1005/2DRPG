using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    // モンスターはBattleSystemから受け取る
    
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    Vector3 originalPos;
    Color originalColor;
    Image image;

    public Pokemon Pokemon { get; set; }
    public bool IsPlayerUnit { get => isPlayerUnit;}
    public BattleHud Hud { get => hud;}

    // バトルで使うモンスターを保持
    // モンスターの画像を反映する

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = transform.localPosition;
        originalColor = image.color;

    }

    public void Setup(Pokemon pokemon)
    {
        // _baseからレベルに応じたモンスターを生成する
        // BattleSystem.csで使うからプロパティ化
        Pokemon = pokemon;

        if (IsPlayerUnit)
        {
            image.sprite = Pokemon.Base.BackSprite;
        }
        else
        {
            image.sprite = Pokemon.Base.FrontSprite;
        }
        hud.SetData(pokemon);
        image.color = originalColor;
        PlayerEnterAnimation();
    }

    public void PlayerEnterAnimation()
    {
        if (IsPlayerUnit)
        {
            transform.localPosition = new Vector3(-480, originalPos.y);
        }
        else
        {
            transform.localPosition = new Vector3(480, originalPos.y);
        }
        // 戦闘時の位置までアニメーション
        transform.DOLocalMoveX(originalPos.x, 1f);
    }

    // 攻撃Anim
    public void PlayerAttackAnimation()
    {
        // シーケンス
        // 見芸に動いたあと、元の位置に戻る
        Sequence sequence = DOTween.Sequence();
        if (IsPlayerUnit)
        {
            sequence.Append(transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
            sequence.Append(transform.DOLocalMoveX(originalPos.x, 0.2f));

        }
        else
        {
            sequence.Append(transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
            sequence.Append(transform.DOLocalMoveX(originalPos.x, 0.2f));
        }
    }
    // ダメージAnim
    public void PlayerHitAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 1f));
        sequence.Append(image.DOColor(originalColor, 1f));
    }
    // 戦闘不能Anim
    public void PlayerFaintAnimation()
    {
        // 下に下りながら、薄くなる
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0, 0.5f));
    }

}
