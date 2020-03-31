using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ゲーム管理クラス
public class GameManager : MonoBehaviour {

    // const.
    public const int MachingCount = 3;

    // enum.
    private enum GameState
    {
        Idle,
        DropMove,
        MatchCheck,
        DeleteDrop,
        FillDrop,
        Wait,
    }

    // serialize field.
    [SerializeField]
    private Board board;
    [SerializeField]
    private Text stateText;

    // private.
    private GameObject grabedDropObject;
    private GameState currentState;
    private Drop selectedDrop;
    private const float SelectedDropAlpha = 0.2f;
    private const float GrabedDropAlpha = 0.6f;

    //-------------------------------------------------------
    // MonoBehaviour Function
    //-------------------------------------------------------
    // ゲームの初期化処理
    private void Start()
    {
        board.InitializeBoard(6, 6);

        currentState = GameState.Idle;
    }

    // ゲームのメインループ
    private void Update()
    {
        switch (currentState)
        {
            case GameState.Idle:
                Idle();
                break;
            case GameState.DropMove:
                DropMove();
                break;
            case GameState.MatchCheck:
                MatchCheck();
                break;
            case GameState.DeleteDrop:
                DeleteDrop();
                break;
            case GameState.FillDrop:
                FillDrop();
                break;
            default:
                break;
        }
        stateText.text = currentState.ToString();
    }

    //-------------------------------------------------------
    // Private Function
    //-------------------------------------------------------
    // プレイヤーの入力を検知し、ピースを選択状態にする
    private void Idle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectDrop();
        }
    }

    // プレイヤーがピースを選択しているときの処理、入力終了を検知したら盤面のチェックの状態に移行する
    private void DropMove()
    {
        if (Input.GetMouseButton(0))
        {
            var drop = board.GetNearestDrop(Input.mousePosition);
            if (drop != selectedDrop)
            {
                board.SwitchDrop(selectedDrop, drop);
            }
            grabedDropObject.transform.localPosition = Input.mousePosition + Vector3.up * 10;
        }
        else if (Input.GetMouseButtonUp(0)) {
            selectedDrop.SetDropAlpha(1f);
            Destroy(grabedDropObject);
            currentState = GameState.MatchCheck;
        }
    }

    // 盤面上にマッチングしているピースがあるかどうかを判断する
    private void MatchCheck()
    {
        if (board.HasMatch())
        {
            currentState = GameState.DeleteDrop;
        }
        else
        {
            currentState = GameState.Idle;
        }
    }

    // マッチングしているピースを削除する
    private void DeleteDrop()
    {
        currentState = GameState.Wait;
        StartCoroutine(board.DeleteMatchDrop(() => currentState = GameState.FillDrop));
    }

    // 盤面上のかけている部分にピースを補充する
    private void FillDrop()
    {
        currentState = GameState.Wait;
        StartCoroutine(board.FillDrop(() => currentState = GameState.MatchCheck));
    }

    //　ドロップを選択する処理
    private void SelectDrop()
    {
      selectedDrop = board.GetNearestDrop(Input.mousePosition);
      var drop = board.InstantiateDrop(Input.mousePosition);
      drop.SetKind(selectedDrop.GetKind());
      drop.SetSize((int)(board.dropWidth * 1.2f));
      drop.SetDropAlpha(GrabedDropAlpha);
      grabedDropObject = drop.gameObject;

      selectedDrop.SetDropAlpha(SelectedDropAlpha);
      currentState = GameState.DropMove;
    }
}
