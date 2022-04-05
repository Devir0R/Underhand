
using UnityEngine;
using TMPro;

public class ShuffleText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RectTransform myRect = GetComponent<RectTransform>();
        Vector3 tableSize = Table.Instance.tableAnimation.backgroundSpriteRenderer.bounds.size;
        float newXScale = (tableSize.x/myRect.rect.width)*0.7f;
        float newYScale = (tableSize.y/myRect.rect.height)*0.7f;
        transform.localScale = new Vector3(newXScale,newYScale,transform.localScale.z);
        transform.position = Vector3.zero;

    }

    public void Show(){
        Table.Instance.Darken();
        GetComponent<TextMeshPro>().sortingLayerID = SortingLayer.NameToID("ShuffleLayer");
    }
    public void Hide(){
        Table.Instance.LightUp();
        GetComponent<TextMeshPro>().sortingLayerID = SortingLayer.NameToID("behindScene");
    }
}
