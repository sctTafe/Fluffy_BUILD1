using System.Collections;
using UnityEngine;

public class ScottsBackup_ResoruceInteractor : MonoBehaviour
{
    public enum OperationType
    {
        Add,
        Subtract
    }

    [Header("Interaction Settings")]
    [SerializeField] private string targetTag;
    [SerializeField] private ScottsBackup_ResourceMng.ResourceType targetResourceType;
    [SerializeField] private OperationType operationType = OperationType.Add;
    [SerializeField] private float amount = 10f;

    [Header("Effect Trigger Mode")]
    [SerializeField] private bool isPeriodic = false;
    [SerializeField] private float intervalSeconds = 1f;

    [Header("One-Shot Cooldown")]
    [SerializeField] private bool hasCooldown = false;
    [SerializeField] private float cooldownSeconds = 2f;

    [Header("Post Effect Behavior")]
    [SerializeField] private bool _disableAfterEffect = false;
    [SerializeField] private Transform _rootGOToDisable;

    private bool isOnCooldown = false;
    private Coroutine periodicCoroutine;


    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(targetTag) && !other.CompareTag(targetTag))
            return;

        ScottsBackup_ResourceMng[] resourceManagers = other.GetComponents<ScottsBackup_ResourceMng>();
        foreach (var resMng in resourceManagers)
        {
            if (resMng.ResourceTypeID == targetResourceType)
            {
                if (isPeriodic)
                {
                    periodicCoroutine = StartCoroutine(PeriodicEffect(resMng));
                }
                else
                {
                    TryApplyEffect(resMng);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPeriodic && periodicCoroutine != null)
        {
            StopCoroutine(periodicCoroutine);
            periodicCoroutine = null;
        }
    }

    private IEnumerator PeriodicEffect(ScottsBackup_ResourceMng resMng)
    {
        while (true)
        {
            TryApplyEffect(resMng);
            yield return new WaitForSeconds(intervalSeconds);
        }
    }

    private void TryApplyEffect(ScottsBackup_ResourceMng resMng)
    {
        if (hasCooldown && isOnCooldown)
            return;

        if (operationType == OperationType.Add)
        {
            resMng.fn_TryIncreaseValue(amount);
        }
        else if (operationType == OperationType.Subtract)
        {
            resMng.fn_TryReduceValue(amount);
        }

        if (hasCooldown)
        {
            StartCoroutine(CooldownRoutine());
        }

        if (_disableAfterEffect)
        {
            gameObject.SetActive(false);
            _rootGOToDisable.gameObject.SetActive(false);
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(cooldownSeconds);
        isOnCooldown = false;
    }
}
