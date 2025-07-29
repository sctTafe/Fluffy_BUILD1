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
			if(Mathf.Abs(progress - goal) < 10)
			{
				call_player_win();
			}
			else
			{
				call_player_fail();
			}
		}
        
    }

	void call_player_win()
	{

	}

	void call_player_fail()
	{
		Destroy(gameObject);
	}
}
