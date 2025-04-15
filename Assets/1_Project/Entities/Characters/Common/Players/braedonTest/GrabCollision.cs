using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GrabCollision : NetworkBehaviour
{

    public GrabPlayer grabPlayer;

    private void Awake()
    {
        if (grabPlayer == null)
        {
            grabPlayer = GetComponentInParent<GrabPlayer>();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("hit player");
                grabPlayer.StartGrab(other.GameObject());
                gameObject.SetActive(false);
            }
        }
        
    }

}
