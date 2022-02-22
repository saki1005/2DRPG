using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Condition
{
    public string Name { get; set; }
    public string Description { get; set; }
    // 状態異常時のメッセージ
    public string StartMessage { get; set; }

    public Action<Pokemon> OnAfterTurn;

}
