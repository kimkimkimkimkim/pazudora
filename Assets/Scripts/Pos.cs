using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pos : MonoBehaviour
{
  private GameObject canvas;
  // Start is called before the first frame update
  void Start()
  {
    canvas = GameObject.Find("Canvas");

    Debug.Log("transform.localPosition " + transform.localPosition);
    Debug.Log("transform.position " + transform.position);
    Debug.Log("worldPosition " + GetWorldPositionFromRectPosition(GetComponent<RectTransform>()));
  }

  //RectTransformからワールド座標に変換
  private Vector3 GetWorldPositionFromRectPosition(RectTransform rect)
  {
    //UI座標からスクリーン座標に変換
    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.GetComponent<Canvas>().worldCamera, rect.position);

    //ワールド座標
    Vector3 result = Vector3.zero;

    //スクリーン座標→ワールド座標に変換
    RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screenPos, canvas.GetComponent<Canvas>().worldCamera, out result);

    return result;
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
        Debug.Log("mousePosition " + Input.mousePosition);
    }
  }
}
