using UnityEngine;
using Unity.Netcode;
using TMPro;
using FMODUnity;

public class PlayerStealth : NetworkBehaviour
{
	GameObject geometry;
	private Renderer[] renderers;
	private Material[][] materialInstances; // Store material instances
	private bool in_bush = false;
	private float time_in_bush = 0;
	private float force_reveal = 0;
	private TMP_Text stealth_prompt;

	private bool played_sound = false;
	
	// Dissolve shader control
	[Header("Dissolve Settings")]
	[SerializeField] private string dissolvePropertyName = "_DitherAlpha";
	[SerializeField] private float dissolveSpeed = 2f;
	[SerializeField] private float hiddenDissolveValue = 0.0f;
	[SerializeField] private float visibleDissolveValue = 1.0f;
	
	private float targetDissolveValue = 1.0f;
	private float currentDissolveValue = 1.0f;
	
	public EventReference hide_sound;

    void Start()
    {
    	geometry = transform.GetChild(3).gameObject;
		
		// Get all renderers in the geometry object and its children
		renderers = geometry.GetComponentsInChildren<Renderer>();
		Debug.Log($"Found {renderers.Length} renderers in geometry");
		
		// Create material instances for runtime modification
		InitializeMaterialInstances();
		
		if(IsOwner)
		{
			GameObject stealthObject = GameObject.FindWithTag("stealth_prompt");
            if (stealthObject)
				stealth_prompt = stealthObject.GetComponent<TMP_Text>();
		}
    }

	void InitializeMaterialInstances()
	{
		materialInstances = new Material[renderers.Length][];
		
		for(int i = 0; i < renderers.Length; i++)
		{
			if(renderers[i] != null)
			{
				// Create instances of materials for runtime modification
				Material[] originalMaterials = renderers[i].materials;
				Material[] instanceMaterials = new Material[originalMaterials.Length];
				
				Debug.Log($"Renderer {i} ({renderers[i].name}) has {originalMaterials.Length} materials");
				
				for(int j = 0; j < originalMaterials.Length; j++)
				{
					if(originalMaterials[j] != null)
					{
						instanceMaterials[j] = new Material(originalMaterials[j]);
						Debug.Log($"Created material instance for {originalMaterials[j].name} using shader {originalMaterials[j].shader.name}");
						
						// Check if the material has the property we need
						if(instanceMaterials[j].HasProperty(dissolvePropertyName))
						{
							Debug.Log($"Material {originalMaterials[j].name} HAS {dissolvePropertyName} property");
							// Also enable ALPHATEST if it's not enabled
							if(instanceMaterials[j].HasProperty("_ALPHATEST_ON"))
							{
								instanceMaterials[j].EnableKeyword("_ALPHATEST_ON");
								Debug.Log("Enabled _ALPHATEST_ON keyword");
							}
						}
						else
						{
							Debug.LogError($"Material {originalMaterials[j].name} DOES NOT HAVE {dissolvePropertyName} property!");
						}
					}
				}
				
				// Assign the instanced materials back to the renderer
				renderers[i].materials = instanceMaterials;
				materialInstances[i] = instanceMaterials;
			}
		}
	}

	void Update()
	{
		if(in_bush)
		{
			time_in_bush += Time.deltaTime;

			if(time_in_bush > 0.8f && force_reveal <= 0)
			{
				// Set dissolve for all players (visual effect)
				targetDissolveValue = hiddenDissolveValue;
				
				// Handle UI only for the owner
				if(IsOwner)
				{
					stealth_prompt.text = "[ Hidden! ]";
					if(!played_sound)
					{
						play_hide_sound();
						played_sound = true;
					}
				}
			}
		}
		else
		{
			played_sound = false;
		}

		force_reveal -= Time.deltaTime;

		// Handle forced reveal
		if(force_reveal > 0)
		{
			targetDissolveValue = visibleDissolveValue;
			
			if(IsOwner)
			{
				stealth_prompt.text = "[ Revealed! ]";
			}
		}

		// Smoothly transition dissolve value
		if(!Mathf.Approximately(currentDissolveValue, targetDissolveValue))
		{
			currentDissolveValue = Mathf.MoveTowards(currentDissolveValue, targetDissolveValue, dissolveSpeed * Time.deltaTime);
			UpdateDissolveShader(currentDissolveValue);
		}
	}

	void UpdateDissolveShader(float dissolveValue)
	{
		Debug.Log($"Updating dissolve shader to {dissolveValue} on {materialInstances.Length} renderers");
        for (int i = 0; i < materialInstances.Length; i++)
		{
			if(materialInstances[i] != null)
			{
				for(int j = 0; j < materialInstances[i].Length; j++)
				{
					Material mat = materialInstances[i][j];
					if(mat != null && mat.HasProperty(dissolvePropertyName))
					{
						mat.SetFloat(dissolvePropertyName, dissolveValue);
						Debug.Log($"Setting {dissolvePropertyName} to {dissolveValue} on material {mat.name} (Shader: {mat.shader.name})");
					}
					else if(mat != null)
					{
						Debug.LogWarning($"Material {mat.name} (Shader: {mat.shader.name}) does not have property {dissolvePropertyName}");
					}
				}
			}
		}
	}

	void play_hide_sound()
	{
		RuntimeManager.PlayOneShot(hide_sound, transform.position);
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("hide_trigger"))
		{
			in_bush = true;
			time_in_bush = 0;
			Debug.Log("Entered bush - starting stealth timer");
		}
	}

	void OnTriggerExit(Collider other)
	{
		if(other.CompareTag("hide_trigger"))
		{
			in_bush = false;
			Debug.Log("Exited bush - becoming visible");

			// Start becoming visible again for all players
			targetDissolveValue = visibleDissolveValue;
			
			// Handle UI only for owner
			if(IsOwner)
			{
				stealth_prompt.text = "";
			}
		}
	}

	// Forces the player to reveal for 10 seconds
	// Called with mutant scan attack
	public void force_unhide()
	{
		force_reveal = 10;
		targetDissolveValue = visibleDissolveValue;
		
		// Only send RPC if not owner (owner already handles it locally above)
		if(!IsOwner)
		{
			force_reveal_ServerRPC();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void force_reveal_ServerRPC()
	{
		client_reveal_ClientRPC();
	}

	[Rpc(SendTo.ClientsAndHost)]
	private void client_reveal_ClientRPC()
	{
		if(IsOwner)
		{
			Debug.Log("Server RPC to reveal player was called");
			force_reveal = 10;
			stealth_prompt.text = "[ Revealed! ]";
		}
	}

	// Add a public method to test the dissolve effect manually
	[ContextMenu("Test Dissolve")]
	public void TestDissolve()
	{
		targetDissolveValue = hiddenDissolveValue;
		currentDissolveValue = hiddenDissolveValue;
		UpdateDissolveShader(currentDissolveValue);
		Debug.Log("Manual dissolve test triggered");
	}

	[ContextMenu("Test Reveal")]
	public void TestReveal()
	{
		targetDissolveValue = visibleDissolveValue;
		currentDissolveValue = visibleDissolveValue;
		UpdateDissolveShader(currentDissolveValue);
		Debug.Log("Manual reveal test triggered");
	}

	void OnDestroy()
	{
		// Clean up material instances to prevent memory leaks
		if(materialInstances != null)
		{
			for(int i = 0; i < materialInstances.Length; i++)
			{
				if(materialInstances[i] != null)
				{
					for(int j = 0; j < materialInstances[i].Length; j++)
					{
						if(materialInstances[i][j] != null)
						{
							Destroy(materialInstances[i][j]);
						}
					}
				}
			}
		}
	}
}
