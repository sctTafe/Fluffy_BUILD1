using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NetworkObjectsListType1SO : ScriptableObject
{
    public List<NetworkObjectsType1SO> notworkObjectType1SOList;

    public bool TryGetIndexMatch(NetworkObjectsType1SO matchObjectSO, out int index)
    {
        index = notworkObjectType1SOList.IndexOf(matchObjectSO);

        if (index == -1)
        {
            Debug.LogWarning("No Index Match!");
            return false;
        }

        return true;
    }
}
