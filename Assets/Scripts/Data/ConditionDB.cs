using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.Poison,
            new Condition()
            {
                id = ConditionID.Poison,
                Name = "どく",
                StartMessage = "はどくになった",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    // 毒ダメージ
                    pokemon.UpdateHP(pokemon.MaxHP/8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}はどくをあびている");
                }
            }
        },
        {
            ConditionID.Burn,
            new Condition()
            {
                id = ConditionID.Burn,
                Name = "やけど",
                StartMessage = "はやけどをおった",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    // 毒ダメージ
                    pokemon.UpdateHP(pokemon.MaxHP/6);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}はやけどをおっている");
                }
            }
        },
        {
            ConditionID.Paralysis,
            new Condition()
            {
                id = ConditionID.Paralysis,
                Name = "まひ",
                StartMessage = "はまひじょうたいになった",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        // 技が出せない
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}はしびれてうごけない");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.Freeze,
            new Condition()
            {
                id = ConditionID.Freeze,
                Name = "こおり",
                StartMessage = "はこおりじょうたいになった",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1,5) == 1)
                    {
                        // 一定確率で氷状態を解除
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}のこおりがとけた");
                        return true;
                    }

                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}はこおってうごけない");
                    return false;
                }
            }
        },        {
            ConditionID.Sleep,
            new Condition()
            {
                id = ConditionID.Sleep,
                Name = "ねむり",
                StartMessage = "はねむった",
                OnStart = (Pokemon pokemon) =>
                {
                    // 技を受けたときに、眠るターン数を設定する
                    pokemon.SleepTime = Random.Range(1,4);
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.SleepTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}はめをさました");
                        return true;
                    }
                    pokemon.SleepTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}はねむっている");
                    return false;
                }
            }
        },

    };

    //static void Poison(Pokemon pokemon)　↑ラムダ式関数
    //{
    //    // 毒ダメージ
    //}

    // まひになると一定の確率で攻撃ができなくなる

}

public enum ConditionID
{
    None,
    Poison, // どく
    Burn,   // やけど
    Sleep,  // ねむり
    Paralysis, //まひ
    Freeze, // こおり
}