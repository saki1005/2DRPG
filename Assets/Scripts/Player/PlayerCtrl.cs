using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCtrl : MonoBehaviour
{
    [SerializeField] float moveSpeed;

    bool isMoving;
    Vector2 input;

    Animator animator;

    // 壁判定のLayer
    [SerializeField] LayerMask solidObjectsLayer;
    // 草むら判定のLayer
    [SerializeField] LayerMask longGrassLayer;
    // 相互依存を解消:UnityAction(関数を登録する)
    public UnityAction OnEncounted;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // 斜め移動の阻止
            if (input.x != 0)
            {
                input.y = 0;
            }

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                //コルーチンを使って１マスずつ移動する
                Vector2 targetPos = transform.position;
                targetPos += input;
                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }

        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        // 移動中は入力を受け付けない
        isMoving = true;

        // targetPosとの差があるなら繰り返す
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // taegetPosに近づける
            transform.position = Vector3.MoveTowards(
                transform.position, // 現在の場所
                targetPos,          // 目的地
                moveSpeed * Time.deltaTime);
            yield return null ;
        }
        transform.position = targetPos;
        isMoving = false;
        CheckForEncounters();
    }
    // targetPosに移動可能化を調べる関数
    bool IsWalkable(Vector2 targetPos)
    {
        // targatPosに半径0.2fの円のRayを飛ばして、ぶつかったらtrue,
        // ぶつからなければfalse;
        return !Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer);
    }

    // 自分の場所から、円のRayを飛ばして、草むらLayerにぶつかったらランダムエンカウント発生
    void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.1f, longGrassLayer))
        {
            // ランダムエンカウント
            if (Random.Range(0, 100) < 10)
            {
                OnEncounted();
                animator.SetBool("isMoving", false);
            }
        }
    }
}
