using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTyping : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public float WaitSeconds = Constants.Constants.TypingWaitTime;
    public float WaitLine=Constants.Constants.DEFAULT_WAITING_LINE;
    private Coroutine typingCoroutine;
    private bool isTyping;

    public void StartTyping(string text)
    {
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine=StartCoroutine(TypeLine(text));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping=true;
        textDisplay.text = text;
        textDisplay.maxVisibleCharacters = 0;

        for(int i = 0; i <= text.Length; i++)
        {
            textDisplay.maxVisibleCharacters = i;
            yield return new WaitForSeconds(WaitSeconds);
        }

        yield return new WaitForSeconds(WaitLine);
        isTyping = false;
    }

    public void CompleteLine()
    {
        if(typingCoroutine!=null)
        {
            StopCoroutine(typingCoroutine );
        }

        textDisplay.maxVisibleCharacters =textDisplay.text.Length;
        isTyping =false;
    }

    public bool IsTyping()
    { return isTyping; }
}
