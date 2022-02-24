using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum BattleState
{
    Start,
    ActionSelection, // 行動選択
    MoveSelection,   // 技選択
    PerformMove,     // 技の実行
    Busy,            // 待機（処理中）
    PartyScreen,
    BattleOver       // バトル終了状態
}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public UnityAction OnBattleOver;

    BattleState state;

    int currentAction; // 0:Fight 1:Run
    int currentMove; // 0:左上 1:右上 2:左下 3:右下
    int currentMember; 

    // これらの変数をどこから取得するか
    PokemonParty playerParty;
    Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        state = BattleState.Start;

        // それぞれをデータベースから初期化・生成
        playerUnit.Setup(playerParty.GetHealthyPokemon());// Playerの戦闘可能なポケモンをセット
        enemyUnit.Setup(wildPokemon); // 野生ポケモンをセット
        partyScreen.Init(); // パーティーリストをセット
        // 描画
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return (dialogBox.TypeDialog($"やせいの { enemyUnit.Pokemon.Base.Name}があらわれた！"));
        ChooseFirstTurn();

    }

    // 戦闘開始時とポケモン入れ替え時に反映
    void ChooseFirstTurn()
    {
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
        {
            // プレイヤーのターン
            ActionSelection();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnableActionSelector(true);
        StartCoroutine(dialogBox.TypeDialog("どうする？"));
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableMoveSelector(true);
    }

    void OpenPartyAction()
    {
        state = BattleState.PartyScreen;
        // パーティスクリーンを表示
        // ポケモンデータを反映
        partyScreen.gameObject.SetActive(true);
        partyScreen.SetPartyData(playerParty.Pokemons);

    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        // やられたモンスターが
        // playerUnitなら
        //  ・他にモンスターがいるなら選択画面
        //  ・いないならバトル終了
        // enemyUnitならバトル終了

        if (faintedUnit.IsPlayerUnit)
        {
            // 戦えるポケモンがいるなら選択画面を出す、いなければ戦闘終了
            Pokemon nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon == null)
            {
                BattleOver();
            }
            else
            {
                OpenPartyAction();
            }
        }
        else
        {
            BattleOver();
        }
    }

    void BattleOver()
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver();
    }

    // 目標:プレイヤーとAIの共通コードを整理


    // PlayerMoveの実行（技の発動）
    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        Move move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }

    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        Move move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if (state == BattleState.PerformMove)
        {
            ActionSelection();
        }
    }

    // 技の実装(実行するUnit, 対象Unit, 技)
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        // まひなら技を出せない
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        if (!canRunMove)
        {
            yield break;
        }

        move.PP--;
        yield return (dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}の{move.Base.Name}！"));
        sourceUnit.PlayerAttackAnimation();
        yield return new WaitForSeconds(0.7f);
        targetUnit.PlayerHitAnimation();

        // ステータス変化なら攻撃しない
        if (move.Base.Category == MoveCategory.Stat)
        {
            yield return RunMoveEffects(move, sourceUnit.Pokemon, targetUnit.Pokemon);
        }
        else
        {
            // ダメージ計算 （fainted=気絶）
            DamageDetails damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);

            // HP反映
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);

        }



        // 戦闘不能ならメッセージ
        if (targetUnit.Pokemon.HP <= 0)
        {
            yield return (dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name}はたおれた"));
            targetUnit.PlayerFaintAnimation();
            yield return new WaitForSeconds(1);
            CheckForBattleOver(targetUnit);
        }

        // ターン終了時
        // 状態異常効果を受ける
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return (dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}はたおれた"));
            sourceUnit.PlayerFaintAnimation();
            yield return new WaitForSeconds(1);
            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        MoveEffects effects = move.Base.Effects;
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self)
            {
                // 自分に対してステータスを変化

                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                // 相手に対してステータスを変化
                target.ApplyBoosts(effects.Boosts);
            }
        }

        if (effects.Status != ConditionID.None)
        {
            target.SetStatus(effects.Status);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    // ステータス変化のログを表示する関数
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        // ログがなくなるまで繰り返す
        while (pokemon.StatusChanges.Count > 0)
        {
            string message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {


        if (damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog($"きゅうしょにあたった！");
            if (damageDetails.TypeEffectiveness > 1f)
            {
                yield return dialogBox.TypeDialog($"こうかはバツグンだ");
            }
            else if (damageDetails.TypeEffectiveness < 1f)
            {
                yield return dialogBox.TypeDialog($"こうかはいまひとつ");

            }
        }
        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog($"こうかはバツグンだ");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog($"こうかはいまひとつ");

        }

        
    }


    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }

    }

    // PlayerActionでの行動を処理する
    void HandleActionSelection()
    {
        // 0:Fight   1:Bag
        // 2:Pokemon 3:Run

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAction++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentAction--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            if (currentAction == 2)
            {
                OpenPartyAction();
            }

        }
    }

    // 技選択のモード
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMove++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMove--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;

        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;

        }

        currentMove = Mathf.Clamp(currentMove, 0, 3);
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            // 技決定
            // ・UIの更新
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            // ・技の処理
            StartCoroutine(PlayerMove());
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            // 技の非表示
            dialogBox.EnableMoveSelector(false);
            // ・UIの更新
            dialogBox.EnableDialogText(true);
            // ・アクション選択に戻る
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMember++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMember--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;

        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;

        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count -1);

        // 選択中のモンスター名に色を付ける
        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            // モンスターの決定
            Pokemon selectedMember = playerParty.Pokemons[currentMember];
            // 入れ替える：現在のキャラと戦闘不能は選べない
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessage("そのモンスターはたたかえない！");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessage("おなじだよ");
                return;
            }


            // ポケモン選択画面を消す
            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            // 入れ替えの処理をする
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        // 戦闘不能なら、戻れはいらない&相手のターンにならない
        // 前のポケモンを戻す
        bool fainted = playerUnit.Pokemon.HP <= 0;
        if (!fainted)
        {
            yield return dialogBox.TypeDialog($"戻れ！{playerUnit.Pokemon.Base.Name}！");
            playerUnit.PlayerFaintAnimation();
            yield return new WaitForSeconds(1.5f);
        }
        // 新しいポケモンを出す
        // モンスターのセットと描画
        playerUnit.Setup(newPokemon);// 選択したポケモンをセット
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        yield return (dialogBox.TypeDialog($"いけ！ { playerUnit.Pokemon.Base.Name}！"));

        if (fainted)
        {
            // 自分のポケモンがやられたら必ず自分のターンだった => 早いほうが先行
            ChooseFirstTurn();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
        //PlayerAction(); TODO:戦闘不能での入れ替えなら自分のターン
    }

}
