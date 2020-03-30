using UnityEngine;
using UnityEngine.UI;

// ピースクラス
public class Drop : MonoBehaviour
{
    // public.
    public bool deleteFlag;

    // private.
    private Image thisImage;
    private RectTransform thisRectTransform;
    private DropKind kind;

    //-------------------------------------------------------
    // MonoBehaviour Function
    //-------------------------------------------------------
    // 初期化処理
    private void Awake()
    {
        // アタッチされている各コンポーネントを取得
        thisImage = GetComponent<Image>();
        thisRectTransform = GetComponent<RectTransform>();

        // フラグを初期化
        deleteFlag = false;
    }

    //-------------------------------------------------------
    // Public Function
    //-------------------------------------------------------
    // ピースの種類とそれに応じた色をセットする
    public void SetKind(DropKind dropKind)
    {
        kind = dropKind;
        SetColor();
    }

    // ピースの種類を返す
    public DropKind GetKind()
    {
        return kind;
    }

    // ピースのサイズをセットする
    public void SetSize(int size)
    {
        this.thisRectTransform.sizeDelta = Vector2.one * size;
        this.thisRectTransform.localScale = new Vector3(0.95f,0.95f,1f);
    }

    //-------------------------------------------------------
    // Private Function
    //-------------------------------------------------------
    // ピースの色を自身の種類の物に変える
    private void SetColor()
    {
        thisImage.sprite = Resources.Load<Sprite>("images/drops/" + ((int)kind + 1).ToString());
    }
}
