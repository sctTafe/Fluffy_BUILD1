using UnityEngine;
using UnityEngine.UI;

public class QTE : MonoBehaviour
{
	public GameObject hit_point;
	public GameObject dude;
	float goal;
	float progress = -100;
	float speed = 120;

    void Start()
    {
		goal = Random.Range(60, 90);
		hit_point.GetComponent<RectTransform>().anchoredPosition = new Vector2(goal, 0);
        
    }

    void Update()
    {
		progress += Time.deltaTime * speed;
		dude.GetComponent<RectTransform>().anchoredPosition = new Vector2(progress, 0);

		if(Input.GetKeyDown(KeyCode.X))
		{
			Debug.Log($"progress {progress} || goal {goal}");
			if(Mathf.Abs(progress - goal) < 22)
			{
				call_player_win();
			}
			else
			{
				call_player_fail();
			}
		}

		if(progress > 120)
		{
			Destroy(gameObject);
		}
        
    }

	void call_player_win()
	{
		GetComponentInParent<PlayerInventory>().complete_qte();
		Destroy(gameObject);
	}

	void call_player_fail()
	{
		Destroy(gameObject);
	}
}
