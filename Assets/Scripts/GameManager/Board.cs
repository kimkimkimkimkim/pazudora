using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 盤面クラス
public class Board : MonoBehaviour {

    // serialize field.
    [SerializeField]
    private GameObject dropPrefab;

    // private.
    private Drop[,] board;
    private int width;
    private int height;
    private int dropWidth;
    private int randomSeed;
    private Vector2[] directions = new Vector2[4]{new Vector2(1,0),new Vector2(-1,0),new Vector2(0,1),new Vector2(0,-1)};

    //-------------------------------------------------------
    // Public Function
    //-------------------------------------------------------
    // 特定の幅と高さに盤面を初期化する
    public void InitializeBoard(int boardWidth, int boardHeight)
    {
        width = boardWidth;
        height = boardHeight;

        dropWidth = Screen.width / boardWidth;

        board = new Drop[width, height];

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                CreateDrop(new Vector2(i, j));
            }
        }
    }

    // 入力されたクリック(タップ)位置から最も近いピースの位置を返す
    public Drop GetNearestDrop(Vector3 input)
    {
        var minDist = float.MaxValue;
        Drop nearestDrop = null;

        // 入力値と盤面のピース位置との距離を計算し、一番距離が短いピースを探す
        foreach (var p in board)
        {
            var dist = Vector3.Distance(input, p.transform.localPosition);
            if (dist < minDist)
            {
                minDist = dist;
                nearestDrop = p;
            }
        }

        return nearestDrop;
    }

    // 盤面上のピースを交換する
    public void SwitchDrop(Drop p1, Drop p2)
    {
        // 位置を移動する
        var p1Position = p1.transform.localPosition;
        p1.transform.localPosition = p2.transform.localPosition;
        p2.transform.localPosition = p1Position;

        // 盤面データを更新する
        var p1BoardPos = GetDropBoardPos(p1);
        var p2BoardPos = GetDropBoardPos(p2);
        board[(int)p1BoardPos.x, (int)p1BoardPos.y] = p2;
        board[(int)p2BoardPos.x, (int)p2BoardPos.y] = p1;
    }

    // 盤面上にマッチングしているピースがあるかどうかを判断する
    public bool HasMatch()
    {
        foreach (var drop in board)
        {
            if (IsMatchDrop(drop))
            {
                return true;
            }
        }
        return false;
    }

    // マッチングしているピースを削除する
    public IEnumerator DeleteMatchDrop(Action endCallBadk)
    {
        foreach (var drop in board)
        {
            if (drop != null && IsMatchDrop(drop))
            {
                var pos = GetDropBoardPos(drop);
                DestroyMatchDrop(pos, drop.GetKind());
                yield return new WaitForSeconds(0.5f);
            }
        }

        endCallBadk();
    }

    // ピースが消えている場所を詰めて、新しいピースを生成する
    public IEnumerator FillDrop(Action endCallBack)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                FillDrop(new Vector2(i, j));
            }
        }

        yield return new WaitForSeconds(1f);
        endCallBack();
    }

    //-------------------------------------------------------
    // Private Function
    //-------------------------------------------------------
    // 特定の位置にピースを作成する
    private void CreateDrop(Vector2 position)
    {
        // ピースの生成位置を求める
        var createPos = GetDropWorldPos(position);
        // 生成するピースの種類をランダムに決める
        var kind = (DropKind)UnityEngine.Random.Range(0, Enum.GetNames(typeof(DropKind)).Length);

        // ピースを生成、ボードの子オブジェクトにする
        /*
        var drop = Instantiate(dropPrefab, createPos, Quaternion.identity).GetComponent<Drop>();
        drop.transform.SetParent(transform);
        drop.SetSize(dropWidth);
        drop.SetKind(kind);
        */
        var drop = Instantiate(dropPrefab).GetComponent<Drop>();
        drop.transform.SetParent(transform);
        drop.transform.localPosition = createPos;
        drop.SetSize(dropWidth);
        drop.SetKind(kind);

        // 盤面にピースの情報をセットする
        board[(int)position.x, (int)position.y] = drop;
    }

    // 盤面上の位置からピースオブジェクトのワールド座標での位置を返す
    private Vector3 GetDropWorldPos(Vector2 boardPos)
    {
        return new Vector3(boardPos.x* dropWidth + (dropWidth / 2), boardPos.y* dropWidth + (dropWidth / 2), 0);
    }

    // ピースが盤面上のどの位置にあるのかを返す
    private Vector2 GetDropBoardPos(Drop drop)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (board[i, j] == drop)
                {
                    return new Vector2(i, j);
                }
            }
        }

        return Vector2.zero;
    }

    // 対象のピースがマッチしているかの判定を行う
    private bool IsMatchDrop(Drop drop)
    {
        // ピースの情報を取得
        var pos = GetDropBoardPos(drop);
        var kind = drop.GetKind();

        // 縦方向にマッチするかの判定 MEMO: 自分自身をカウントするため +1 する
        var verticalMatchCount = GetSameKindDropNum(kind, pos, Vector2.up) + GetSameKindDropNum(kind, pos, Vector2.down) + 1;

        // 横方向にマッチするかの判定 MEMO: 自分自身をカウントするため +1 する
        var horizontalMatchCount = GetSameKindDropNum(kind, pos, Vector2.right) + GetSameKindDropNum(kind, pos, Vector2.left) + 1;

        return verticalMatchCount >= GameManager.MachingCount || horizontalMatchCount >= GameManager.MachingCount;
    }

    // 対象の方向に引数で指定したの種類のピースがいくつあるかを返す
    private int GetSameKindDropNum(DropKind kind, Vector2 dropPos, Vector2 searchDir)
    {
        var count = 0;
        while (true)
        {
            dropPos += searchDir;
            if (IsInBoard(dropPos) && board[(int)dropPos.x, (int)dropPos.y].GetKind() == kind)
            {
                count++;
            }
            else
            {
                break;
            }
        }
        return count;
    }

    // 対象の座標がボードに存在するか(ボードからはみ出していないか)を判定する
    private bool IsInBoard(Vector2 pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
    }

    // 特定のピースのが削除されているかを判断し、削除されているなら詰めるか、それができなければ新しく生成する
    private void FillDrop(Vector2 pos)
    {
        var drop = board[(int)pos.x, (int)pos.y];
        if (drop != null && !drop.deleteFlag)
        {
            // ピースが削除されていなければ何もしない
            return;
        }

        // 対象のピースより上方向に有効なピースがあるかを確認、あるなら場所を移動させる
        var checkPos = pos + Vector2.up;
        while (IsInBoard(checkPos))
        {
            var checkDrop = board[(int)checkPos.x, (int)checkPos.y];
            if (checkDrop != null && !checkDrop.deleteFlag)
            {
                checkDrop.transform.localPosition = GetDropWorldPos(pos);
                board[(int)pos.x, (int)pos.y] = checkDrop;
                board[(int)checkPos.x, (int)checkPos.y] = null;
                return;
            }
            checkPos += Vector2.up;
        }

        // 有効なピースがなければ新しく作る
        CreateDrop(pos);
    }

    // 特定のピースがマッチしている場合、ほかのマッチしたピースとともに削除する
    private void DestroyMatchDrop(Vector2 pos, DropKind kind)
    {
        // ピースの場所が盤面以外だったら何もしない
        if (!IsInBoard(pos))
        {
            return;
        }

        // ピースが無効であったり削除フラグが立っていたりそもそも、種別がちがうならば何もしない
        var drop = board[(int)pos.x, (int)pos.y];
        if (drop == null || drop.deleteFlag || drop.GetKind() != kind)
        {
            return;
        }

        // ピースが同じ種類でもマッチングしてなければ何もしない
        if (!IsMatchDrop(drop))
        {
            return;
        }

        // 削除フラグをたてて、周り４方のピースを判定する
        drop.deleteFlag = true;
        foreach (var dir in directions)
        {
            DestroyMatchDrop(pos + dir, kind);
        }

        // ピースを削除する
        Destroy(drop.gameObject);
    }
}
