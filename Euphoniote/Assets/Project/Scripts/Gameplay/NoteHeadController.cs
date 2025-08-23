// _Project/Scripts/Gameplay/NoteHeadController.cs

using UnityEngine;

public class NoteHeadController : MonoBehaviour
{
    [Header("组件引用")]
    public SpriteRenderer arrowRenderer;
    public SpriteRenderer containerRenderer;
    public Transform fretContainer;
    public GameObject fretSpritePrefab;

    [Header("布局参数")]
    public float heightPerLetter = 0.8f;
    public float verticalPadding = 0.2f;

    private static SpriteAtlas spriteAtlas;

    public void Initialize(NoteData data,bool generateFrets = true)
    {
        if (spriteAtlas == null)
        {
            spriteAtlas = Resources.Load<SpriteAtlas>("GameSpriteAtlas");
            if (spriteAtlas != null) spriteAtlas.Initialize();
            else { Debug.LogError("找不到 SpriteAtlas!"); return; }
        }

        // 1. 设置箭头
        arrowRenderer.sprite = spriteAtlas.GetStrumSprite(data.strumType);

        // 2. 设置容器
        int fretCount = (data.requiredFrets != null) ? data.requiredFrets.Count : 0;
        int containerSizeLevel = (fretCount == 0) ? 1 : fretCount;

        containerRenderer.sprite = spriteAtlas.GetContainerTemplate(data.isSpecial);
        float targetHeight = (containerSizeLevel * heightPerLetter) + (verticalPadding * 2);
        containerRenderer.size = new Vector2(containerRenderer.size.x, targetHeight);

        // 3. 清理并排列字母
        foreach (Transform child in fretContainer) { Destroy(child.gameObject); }
        if (generateFrets && fretCount > 0)
        {
            float totalLetterHeight = fretCount * heightPerLetter;
            float startY = (totalLetterHeight / 2f) - (heightPerLetter / 2f);
            for (int i = 0; i < fretCount; i++)
            {
                GameObject fretObj = Instantiate(fretSpritePrefab, fretContainer);
                fretObj.GetComponent<SpriteRenderer>().sprite = spriteAtlas.GetFretSprite(data.requiredFrets[i]);
                fretObj.transform.localPosition = new Vector3(0, startY - i * heightPerLetter, 0);
            }
        }
    }
}