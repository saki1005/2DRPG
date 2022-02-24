using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PokemonBase;

[System.Serializable] // インスペクターでポケモンリストを設定できるようにする（PokemonParty,MapAreaに反映）
// レベルに応じたステータスの違うポケモンを作成するクラス
// 注意：データのみ扱う（純粋なC#のクラス）
public class Pokemon
{
    // インスペクターからデータを設定できるようにする
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    // ベースとなるデータ
    public PokemonBase Base { get => _base; }
    public int Level { get => level; }

    public int HP { get; set; }

    // 使える技
    public List<Move> Moves { get; set; }
    // 初期ステータスと追加ステータス
    public Dictionary<Stat, int> Stats { get; set; }
    public Dictionary<Stat, int> StatBoosts { get; set; }
    // ・ログをためておく変数を作る:出し入れがかんたんなリスト(Queue)
    public Queue<string> StatusChanges { get; private set; }

    // 状態異常の変数
    public Condition Status { get; private set; }
    public int SleepTime { get; set; }

    public bool HpChange { get; set; }

    Dictionary<Stat, string> statDic = new Dictionary<Stat, string>()
    {
        {Stat.Attack, "こうげき" },
        {Stat.Defense, "ぼうぎょ" },
        {Stat.SpAttack, "とくこう" },
        {Stat.SpDefense, "とくぼう" },
        {Stat.Speed, "すばやさ" },
    };

    // コンストラクター：生成時の初期設定をする => Init関数に変更
    public void Init()
    {
        StatusChanges = new Queue<string>(); 
        Moves = new List<Move>();
        // 使える技の設定：覚える技のレベル以上なら、Movesに追加
        foreach (LearnableMove learnableMove in Base.LernableMoves)
        {
            if (Level >= learnableMove.Level)
            {
                // 技を覚える
                Moves.Add(new Move(learnableMove.Base));
            }

            // 4つ以上の技は使えない
            if (Moves.Count >= 4)
            {
                break;
            }
        }
        CalculateStats();
        HP = MaxHP;
        ResetStatBoost();

    }

    void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
        };
    }

    public void OnBattleOver()
    {
        ResetStatBoost();
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        MaxHP = Mathf.FloorToInt((Base.MaxHP * Level) / 100f) + 10 + Level;
    }

    int GetStat(Stat stat)
    {
        int statValue = Stats[stat];
        // ステータス変化の値も計算する
        int boost = StatBoosts[stat];
        float[] boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            // 強化なら
            statValue = Mathf.FloorToInt(statValue * boostValues[boost]);
        }
        else
        {
            // 弱体化なら
            statValue = Mathf.FloorToInt(statValue / boostValues[-boost]);
        }

        return statValue;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        // ステータス変化を反映
        foreach (StatBoost statBoost in statBoosts)
        {
            // どのステートを
            Stat stat = statBoost.stat;
            // 何段階
            int boost = statBoost.boost;
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}の{statDic[stat]}があがった");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}の{statDic[stat]}がさがった");
            }
        }
    }

    //プロパティ
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }
    public int MaxHP { get; private set; }

    // ３つの情報を渡す　=> {戦闘不能、クリティカル、相性}
    // ダメージを計算して与え、生死をboolで判断
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        // 攻撃力の計算
        // クリティカル
        float critical = 1f;
        //6.25%でクリティカル value= 0 ~1のランダム
        if (Random.value * 100 <= 6.25f)
        {
            critical = 2f;
        }
        // 相性
        float type = TypeChart.GetEffectiveness(move.Base.Type, Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, Base.Type2);
        DamageDetails damageDitails = new DamageDetails
        {
            Fainted = false,
            Critical = critical,
            TypeEffectiveness = type
        };

        // 特殊攻撃の場合の修正
        float attack = attacker.Attack;
        float defense = Defense;
        if (move.Base.Category == MoveCategory.Special)
        {
            attack = attacker.SpAttack;
            defense = SpDefense;
        }
        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) +2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);
        return damageDitails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        HpChange = true;
    }

    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

    public void SetStatus(ConditionID conditionID)
    {
        if (Status != null)
        {
            return;
        }
        // どの状態異常になるのか設定
        Status = ConditionDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        // ログに追加
        StatusChanges.Enqueue($"{Base.Name}{Status.StartMessage}");

        

    }

    // 状態異常から回復
    public void CureStatus()
    {
        Status = null;
    }

    public bool OnBeforeMove()
    {
        if (Status?.OnBeforeMove != null)
        {
            return Status.OnBeforeMove(this);
        }
        return true;
    }

    // ターン終了時にやること（状態異常）
    public void OnAfterTurn()
    {
        //if (Status != null)
        //{
        //    Status.OnAfterTurn(this);
        //}

        Status?.OnAfterTurn?.Invoke(this); // ?でnullチェックができる

    }



}

    public class DamageDetails
    {
        public bool Fainted { get; set; } // 戦闘不能かどうか
        public float Critical { get; set; }
        public float TypeEffectiveness { get; set; }
    }