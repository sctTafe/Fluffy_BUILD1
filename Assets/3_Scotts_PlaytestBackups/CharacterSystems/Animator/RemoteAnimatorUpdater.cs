using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;


public class RemoteAnimatorUpdater : NetworkBehaviour
{
    [Header("Will Try to auto assign if conponents on same game object")]
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkTransform netTransform;


    [SerializeField] private string forwardParam = "ForwardsSpeed";
    [SerializeField] private string sidewaysParam = "SidewaysSpeed";

    private Vector3 lastPosition;
    private float updateRate = 0.05f; // ~20 fps update
    private float timer = 0f;

    private void Awake()
    {
        if (animator == null)   
        animator = GetComponent<Animator>();
        if (netTransform == null)
        netTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner || IsServer)
        {
            // This only runs on non-owner clients
            enabled = false;
        }
    }

    private void Update()
    {
        if (!IsSpawned || animator == null) return;

        timer += Time.deltaTime;
        if (timer >= updateRate)
        {
            timer = 0f;

            Vector3 worldDelta = transform.position - lastPosition;
            float deltaTime = updateRate;

            Vector3 worldVelocity = worldDelta / deltaTime;
            Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

            animator.SetFloat(forwardParam, localVelocity.z);
            animator.SetFloat(sidewaysParam, localVelocity.x);

            lastPosition = transform.position;
        }
    }
}