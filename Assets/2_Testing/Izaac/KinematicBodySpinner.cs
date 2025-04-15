using UnityEngine;

public class KinematicBodySpinner : MonoBehaviour
{
    Rigidbody rb;
    public Vector3 rotationSpeed = new Vector3(0, 100, 0); // Degrees per second
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    void Update()
    {
        //transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        rb.angularVelocity = rotationSpeed * Time.deltaTime;
    }
}
