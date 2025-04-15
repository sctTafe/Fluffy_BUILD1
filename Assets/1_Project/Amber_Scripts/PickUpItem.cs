using UnityEngine;

public class PickUpItem : MonoBehaviour
{
	/**
	* Script created by Amber to attach to items that should be picked up.
	* Has the item's name.
	* Remember to tag the item pick_up_item
	**/

	public string name = "item";

	public string GetItem()
	{
		return name;
	}
}
