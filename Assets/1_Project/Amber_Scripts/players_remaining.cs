using UnityEngine;
using TMPro;

public class players_remaining : MonoBehaviour
{
	TMP_Text text;
	float counter = 0;

    void Start()
    {
    	text = GetComponent<TMP_Text>();    
    }

    void Update()
    {
		counter += Time.deltaTime;

		if(counter > 1)
		{
			int fluffies = GameObject.FindGameObjectsWithTag("Player").Length;
			text.text = $"x{fluffies}";
		}
    }
}
