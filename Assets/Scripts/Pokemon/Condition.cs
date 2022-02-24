using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public ConditionID id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    // 状態異常時のメッセージ
    public string StartMessage { get; set; }
    public Action<Pokemon> OnStart;
    public Func<Pokemon, bool> OnBeforeMove; // Func<引数, 返り値>
    public Action<Pokemon> OnAfterTurn; // Action<引数>

}
