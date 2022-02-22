using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class PokemonBase : ScriptableObject
{
    [SerializeField] new string name;
    public int Level { get; set; }
    [TextArea] 
    [SerializeField] string description;

    // 画像
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    // タイプ
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // ステータス
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    [SerializeField] int maxHP;

    // 覚える技一覧
    [SerializeField] List<LearnableMove> learnableMoves;


    //public int Attack()
    //{
    //    return attack;
    //}

    // 変数のカプセル化
    // ほかファイルから値の取得ができる。（set=>を使えば変更可能：インスペクターで設定しないもの）
    public int Attack { get => attack; }
    public int Defense { get => defense; }
    public int SpAttack { get => spAttack; }
    public int SpDefense { get => spDefense; }
    public int Speed { get => speed; }
    public int MaxHP { get => maxHP; }

    public List<LearnableMove> LernableMoves { get => learnableMoves; }
    public string Name { get => name; }
    public string Description { get => description; }
    public Sprite FrontSprite { get => frontSprite; }
    public Sprite BackSprite { get => backSprite; }
    public PokemonType Type1 { get => type1; }
    public PokemonType Type2 { get => type2; }

    // 覚えられる技
    [Serializable]
    public class LearnableMove
    {
        [SerializeField] MoveBase _base;
        [SerializeField] int level;

        public MoveBase Base { get => _base; }
        public int Level { get => level; }
    }

    public enum PokemonType
    {
        NONE,
        NORMAL,
        FIRE,
        WATER,
        ELECTRIC,
        GRASS,
        ICE,
        FIGHTING,
        POISON,
        GROUND,
        FLYING,
        PSYCHIC,
        BUG,
        ROCK,
        GHOST,
        DRAGON,
    }

    public enum Stat
    {
        Attack,
        Defense,
        SpAttack,
        SpDefense,
        Speed,
    }

    // ・技の相性計算
    // ・クリティカルヒット
    // ・ダイアログに表示

    public class TypeChart
    {
        static float[][] chart =
        {
            //攻撃\防御          NOR  FIR   WAT  ELEC GRAS
            /*NOR*/ new float[]{  1f,  1f,  1f,  1f,  1f},
            /*FIR*/ new float[]{  1f,0.5f,0.5f,  2f,  2f},
            /*WAT*/ new float[]{  1f,  2f,0.5f,0.5f,0.5f},
            /*ELE*/ new float[]{  1f,0.5f,  2f,  2f,  1f},
            /*GRA*/ new float[]{  1f,0.5f,  2f,  1f,0.5f},
        };

        public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
        {
            if (attackType == PokemonType.NONE || defenseType == PokemonType.NONE)
            {
                return 1f;
            }
            int row = (int)attackType - 1;
            int col = (int)defenseType - 1;
            return chart[row][col];
        }
    }

}
