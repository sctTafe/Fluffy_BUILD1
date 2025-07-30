using UnityEngine;
using DG.Tweening;

public class MainMenuCameraMover : MonoBehaviour
{
    [Header("Camera Move Settings")]
    public Vector3 mainMenuPosition;
    public Vector3 settingsMenuPosition;
    public float moveDuration = 1f;
    public Ease moveEase = Ease.InOutSine;

    public void MoveToSettings()
    {
        if (settingsMenuPosition != null)
            transform.DOMove(settingsMenuPosition, moveDuration).SetEase(moveEase);
    }

    public void MoveToMainMenu()
    {
        if (mainMenuPosition != null)
            transform.DOMove(mainMenuPosition, moveDuration).SetEase(moveEase);
    }
}