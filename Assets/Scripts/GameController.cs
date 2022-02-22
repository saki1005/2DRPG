using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    FreeRoam, // マップ移動
    Battle,
}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerCtrl playerCtrl;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state = GameState.FreeRoam;

    private void Start()
    {
        playerCtrl.OnEncounted += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        // パーティと野生ポケモンの取得
        PokemonParty playerParty = playerCtrl.GetComponent<PokemonParty>();
        // FindObjrctOfType：シーン内から一致するコンポーネントを一つ取得する
        Pokemon wildPokemon = FindObjectOfType<MapArea>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    public void EndBattle()
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

    }

    // ゲームの状態を管理（探索 / 戦闘）
    void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerCtrl.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }


}
