using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PokemonBase;

[CreateAssetMenu]
public class MoveBase : ScriptableObject
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy; //正確性
    [SerializeField] int pp;
    // カテゴリー (物理/特殊/ステータス変化)
    [SerializeField] MoveCategory category;
    // ターゲット
    [SerializeField] MoveTarget target;
    // ステータス変化のリスト:どのステータスをどのくらい変化させるか
    [SerializeField] MoveEffects effects;

    public string Name { get => name; }
    public string Description { get => description; }
    public PokemonType Type { get => type; }
    public int Power { get => power;}
    public int Accuracy { get => accuracy; }
    public int PP { get => pp; }
    public MoveCategory Category { get => category; set => category = value;  }
    public MoveTarget Target { get => target;}
    public MoveEffects Effects { get => effects;}

    // 他のファイル(Move.cs)から参照するためにプロパティを使う




}

public enum MoveCategory
{
    Physicial,
    Special,
    Stat,
}

public enum MoveTarget
{
    Foe,
    Self,
}

// 下のリスト
[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;

    public List<StatBoost> Boosts { get => boosts;}
    public ConditionID Status { get => status; }
}

// どのステータスをどのくらい変化させるか
[System.Serializable]

public class StatBoost
{
    public Stat stat;
    public int boost;
}