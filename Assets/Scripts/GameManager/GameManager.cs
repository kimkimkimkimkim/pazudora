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
    private GameState currentState;
    private Drop selectedDrop;

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
            selectedDrop = board.GetNearestDrop(Input.mousePosition);
            currentState = GameState.DropMove;
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
        }
        else if (Input.GetMouseButtonUp(0)) {
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
}
