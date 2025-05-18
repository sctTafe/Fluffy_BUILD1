using UnityEngine;
using TMPro;

public class LobbyTips : MonoBehaviour
{
	[SerializeField] TMP_Text txt;
	string[] tips = {
		"Fluffies can hide from the mutant in the bushes. Watch out though, the mutant can has a roar that can reveal your position!",
		"Fluffies need to find items hidden around the map and bring them back to the boat to escape the island!",
		"The mutant can destroy bushes using the right mouse button!",
		"The mutant can reveal players hiding in bushes using Q",
		"Fluffies can sprint by holding left shift, this can help them escape from the mutant!",
		"Only fluffies are small enough to fit through the holes in chainlink fences.",
		"Watch out! Red mushrooms are dangerous and will hurt fluffies",
		"Yellow mushrooms will recover some of a fluffy's stamina!",
		"The mutant wins by killing all of the fluffies!",
		"The fluffies win by repairing the boat and escaping the island!"
	};

	void Start()
	{
		txt = GetComponent<TMP_Text>();
		txt.text = "Tip: " + tips[Random.Range(0, tips.Length)];
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			txt.text = "Tip: " + tips[Random.Range(0, tips.Length)];
		}
	}
}
