using Unity.Netcode;
using UnityEngine;

public class Player_SpawnOffset : NetworkBehaviour {
    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-2, 2);

    void Start() {
        if (IsClient && IsOwner) {
            fn_OffsetTransfromPosition_XZ(this.transform);
        }
    }
    public void fn_OffsetTransfromPosition_XZ(Transform transform) {
        transform.position = new Vector3(Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y), 0,
                   Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y));
    }
}