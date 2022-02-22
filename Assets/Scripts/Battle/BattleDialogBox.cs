using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 役割：dialogのTextを取得して、変更する
public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Color highlightColor;
    [SerializeField] int letterPerSecond;
    [SerializeField] Text dialogText;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDitails;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    // タイプ形式でテキストを表示させる
    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (char letter in dialog)
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / letterPerSecond);
        }
        yield return new WaitForSeconds(0.7f);
    }

    // UIの表示/非表示の制御

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDitails.SetActive(enabled);
    }

    //選択中のアクションの色を変える
    public void UpdateActionSelection(int selectAction)
    {
        // selectActionが0のときはactionTexts[0]の色を青にする,それ以外を黒にする

        for (int i=0; i<actionTexts.Count; i++)
        {
            if (selectAction == i)
            {
                actionTexts[i].color = highlightColor;
            }
            else
            {
                actionTexts[i].color = Color.black;
            }
        }
        
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i=0; i<moveTexts.Count; i++)
        {
            if (i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = ".";
            }
        }

       
    }

    //選択中の技の色を変える
    public void UpdateMoveSelection(int selectMove, Move move)
    {
        // selectMoveが0のときはactionTexts[0]の色を青にする,それ以外を黒にする

        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (selectMove == i)
            {
                moveTexts[i].color = highlightColor;
            }
            else
            {
                moveTexts[i].color = Color.black;
            }
        }
        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();
    }

}
