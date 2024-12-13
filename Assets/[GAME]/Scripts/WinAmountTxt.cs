using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WinAmountTxt : MonoBehaviour
{
    [SerializeField] Text winAmountTxt;

    void Start()
    {

    }

    public void AnimateText(string _text, Vector3 _startPos) 
    {
        //Debug.Log($"Win amount Txt { _text }");
        GameManager.instance.UpdateIntValue(0, double.Parse(_text), winAmountTxt, 1f);
        //winAmountTxt.text = $"{GameManager.currencySymbol}{_text}";

        _startPos.z = 0;
        winAmountTxt.transform.position = _startPos;

        Vector3 _taregetPos = _startPos;
        _taregetPos.y += 1;

        Sequence sequence = DOTween.Sequence();

        //sequence.Insert(0.1f, winAmountTxt.transform.DOMove(_taregetPos, 1.9f));
        //sequence.Insert(1f, winAmountTxt.transform.DOScale(Vector3.zero, 1f))
        //        .OnComplete(() => {
        //            Destroy(this.gameObject);
        //        });

        sequence.Insert(0f, winAmountTxt.transform.DOScale(Vector3.one, 0.9f));
        sequence.Insert(0.1f, winAmountTxt.transform.DOMove(_taregetPos, 1.9f));
        sequence.Insert(1.2f, winAmountTxt.transform.DOScale(Vector3.zero, 1f))
                .OnComplete(() => {
                    Destroy(this.gameObject);
                });

        sequence.Play();
    }
}