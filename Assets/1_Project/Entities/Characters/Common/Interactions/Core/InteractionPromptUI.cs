using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class InteractionPromptUI : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] private GameObject _actionPanelPrefab;
    [SerializeField] private Transform _container;

    private readonly List<GameObject> _currentActions = new();
    public bool IsDisplayed = false;

    void Start()
    {
        _camera = Camera.main;
        HidePrompt();
    }

    void LateUpdate()
    {
        var rotation = _camera.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
    }

    public void ShowPrompt(List<string> actions)
    {
        ClearCurrentActions();

        if (actions.Count == 0)
        {
            HidePrompt();
            return;
        }

        float radius = 200f;
        int count = actions.Count;

        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * Mathf.PI * 2f;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            GameObject actionPanel = Instantiate(_actionPanelPrefab, _container);
            actionPanel.GetComponent<RectTransform>().anchoredPosition = offset;
            actionPanel.GetComponentInChildren<TextMeshProUGUI>().text = actions[i];

            _currentActions.Add(actionPanel);
        }

        _container.gameObject.SetActive(false);
        _container.gameObject.SetActive(true);
        IsDisplayed = true;
    }

    public void HidePrompt()
    {
        _container.gameObject.SetActive(false);
        ClearCurrentActions();
        IsDisplayed = false;
    }

    private void ClearCurrentActions()
    {
        foreach (var action in _currentActions)
        {
            Destroy(action);
        }
        _currentActions.Clear();
    }
}
