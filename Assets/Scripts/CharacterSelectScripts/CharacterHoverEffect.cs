using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterHoverEffect : MonoBehaviour, IPointerEnterHandler
{
    public CharacterStatUI statUI;
    public CharacterSelectionManager selectionManager;
    public int characterID;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectionManager == null || statUI == null)
            return;

        // 확인창 또는 이름입력창이 열려 있으면 Hover 막기
        if (selectionManager.isConfirmOpen || selectionManager.isNamePanelOpen)
            return;

        if (characterID == 0)
            statUI.ShowKnight();
        else if (characterID == 1)
            statUI.ShowMage();
        else if (characterID == 2)
            statUI.ShowArcher();
    }
}