using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    // ポケモン選択画面の管理
    PartyMemberUI[] memberSlots;

    List<Pokemon> pokemons;

    // PartyMemberUIの取得
    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    // BattleSystemから手持ちのポケモンデータをもらって、それぞれにデータをセットする
    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;
        for (int i=0; i<memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].SetData(pokemons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
        messageText.text = "ポケモンをえらんでください";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        // selectedMemberと一致するなら名前の色を変える
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
