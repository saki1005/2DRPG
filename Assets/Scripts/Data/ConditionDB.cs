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
    };

    //static void Poison(Pokemon pokemon)　↑ラムダ式関数
    //{
    //    // 毒ダメージ
    //}

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