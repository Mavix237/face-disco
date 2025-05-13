using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI outText;
    public void UpdateText(string text)
    {
        outText.text = text;
    }
}
