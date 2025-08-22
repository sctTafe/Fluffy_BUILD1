using UnityEngine;
using System.Collections;

public class EnhancedMutantRevealPlayer : MonoBehaviour
{	
	[Header("Reveal Settings")]
	[SerializeField] private float revealDuration = 8f;
	[SerializeField] private float revealRange = 15f;
	[SerializeField] private float staminaCost = 40f;
	[SerializeField] private float cooldownTime = 10f;
	
	[Header("Visual Effects")]
	[SerializeField] private bool useNewPredatorVision = true;
	[SerializeField] private Material highlightMaterial;
	[SerializeField] private Color highlightColor = new Color(1f, 0.2f, 0.2f, 0.8f);
	
	[Header("Audio & Feedback")]
	[SerializeField] private AudioClip activationSound;
	[SerializeField] private AudioClip deactivationSound;
	[SerializeField] private AudioSource audioSource;

    private MutantStamina staminaComponent;

    // Internal variables
    GameObject[] players;
	GameObject mutant_radar_fill;
	private float cooldown = 0;
	private bool isRevealing = false;
	
	// Store original materials for revealed players (backup method)
	private System.Collections.Generic.Dictionary<GameObject, MaterialInfo[]> originalMaterials = 
		new System.Collections.Generic.Dictionary<GameObject, MaterialInfo[]>();

	// Predator Vision Controller reference
	private SimplePredatorVisionController predatorVisionController;

	[System.Serializable]
	private struct MaterialInfo
	{
		public Renderer renderer;
		public Material[] materials;
	}

	void Start()
	{
		if (staminaComponent == null)
		{
			staminaComponent = GetComponent<MutantStamina>();
        }
		
		mutant_radar_fill = GameObject.FindWithTag("mutant_radar_fill");
		
		// Get or create SimplePredatorVisionController
		predatorVisionController = GetComponent<SimplePredatorVisionController>();
		if (predatorVisionController == null && useNewPredatorVision)
		{
			predatorVisionController = gameObject.AddComponent<SimplePredatorVisionController>();
		}
		
		// Setup audio
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.playOnAwake = false;
				audioSource.spatialBlend = 0f;
			}
		}
		
		// Create highlight material if not assigned and not using new system
		if (!useNewPredatorVision && highlightMaterial == null)
		{
			highlightMaterial = CreateHighlightMaterial();
		}
	}

	void Update()
	{
		cooldown -= Time.deltaTime;		

		// Handle input for reveal ability
		if(cooldown <= 0 && !isRevealing && Input.GetKeyDown(KeyCode.Q) && GetCurrentStamina() > staminaCost)
		{
			StartReveal();
		}

        // Update radar UI
        if (mutant_radar_fill != null)
			mutant_radar_fill.transform.localScale = new Vector3(1, Mathf.Clamp(cooldownTime - cooldown, 0, cooldownTime) / cooldownTime, 1);
	}

	private float GetCurrentStamina()
	{
		if (staminaComponent == null) return 0f;
		
		// Try to call get_stamina method
		var method = staminaComponent.GetType().GetMethod("get_stamina");
		if (method != null)
		{
			return (float)method.Invoke(staminaComponent, null);
		}
		
		return 0f;
	}

	private void ReduceStamina(float amount)
	{
		if (staminaComponent == null) return;
		
		// Try to call reduce_stamina method
		var method = staminaComponent.GetType().GetMethod("reduce_stamina");
		if (method != null)
		{
			method.Invoke(staminaComponent, new object[] { amount });
		}
	}

	private void ForceUnhidePlayer(GameObject player)
	{
		if (player == null) return;
		
		// Try to get PlayerStealth component
		Component stealthComponent = player.GetComponent("PlayerStealth");
		if (stealthComponent != null)
		{
			// Try to call force_unhide method
			var method = stealthComponent.GetType().GetMethod("force_unhide");
			if (method != null)
			{
				method.Invoke(stealthComponent, null);
			}
		}
	}

	private void StartReveal()
	{
		ReduceStamina(staminaCost);
		cooldown = cooldownTime;
		isRevealing = true;

		Debug.Log("EnhancedMutantRevealPlayer: Starting reveal ability");

		// Find all players in range and force unhide them
		players = GameObject.FindGameObjectsWithTag("Player");
		
		foreach(GameObject player in players)
		{
			if(Vector3.Distance(player.transform.position, transform.position) < revealRange)
			{
				// Force unhide using existing stealth system
				ForceUnhidePlayer(player);
			}
		}

		// Use the new predator vision system or fall back to material method
		if (useNewPredatorVision && predatorVisionController != null)
		{
			// Use the new predator vision system with fade effects
			predatorVisionController.ActivatePredatorVision(revealDuration);
			PlaySound(activationSound);
			StartCoroutine(WaitForPredatorVisionEnd());
		}
		else
		{
			// Fall back to original material method
			StartOriginalReveal();
		}
	}

	private IEnumerator WaitForPredatorVisionEnd()
	{
		yield return new WaitForSeconds(revealDuration);
		PlaySound(deactivationSound);
		isRevealing = false;
		Debug.Log("EnhancedMutantRevealPlayer: Predator vision reveal completed");
	}

	private void StartOriginalReveal()
	{
		foreach(GameObject player in players)
		{
			if(Vector3.Distance(player.transform.position, transform.position) < revealRange)
			{
				ApplyHighlightToPlayer(player);
			}
		}
		StartCoroutine(EndRevealAfterDelay());
	}

	private void ApplyHighlightToPlayer(GameObject player)
	{
		Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
		MaterialInfo[] materialInfos = new MaterialInfo[renderers.Length];

		for (int i = 0; i < renderers.Length; i++)
		{
			if (renderers[i] != null)
			{
				materialInfos[i] = new MaterialInfo
				{
					renderer = renderers[i],
					materials = renderers[i].materials
				};

				Material[] newMaterials = new Material[renderers[i].materials.Length + 1];
				for (int j = 0; j < renderers[i].materials.Length; j++)
				{
					newMaterials[j] = renderers[i].materials[j];
				}
				newMaterials[newMaterials.Length - 1] = highlightMaterial;
				
				renderers[i].materials = newMaterials;
			}
		}

		if (!originalMaterials.ContainsKey(player))
		{
			originalMaterials.Add(player, materialInfos);
		}
	}

	private void RemoveHighlightFromPlayer(GameObject player)
	{
		if (originalMaterials.ContainsKey(player))
		{
			MaterialInfo[] materialInfos = originalMaterials[player];
			
			foreach (MaterialInfo info in materialInfos)
			{
				if (info.renderer != null)
				{
					info.renderer.materials = info.materials;
				}
			}
			
			originalMaterials.Remove(player);
		}
	}

	private IEnumerator EndRevealAfterDelay()
	{
		yield return new WaitForSeconds(revealDuration);
		
		foreach (var kvp in originalMaterials)
		{
			RemoveHighlightFromPlayer(kvp.Key);
		}
		
		originalMaterials.Clear();
		isRevealing = false;
		PlaySound(deactivationSound);
	}

	private void PlaySound(AudioClip clip)
	{
		if (audioSource != null && clip != null)
		{
			audioSource.clip = clip;
			audioSource.Play();
		}
	}

	private Material CreateHighlightMaterial()
	{
		Shader highlightShader = Shader.Find("Custom/PlayerRevealHighlight");
		
		if (highlightShader == null)
		{
			highlightShader = Shader.Find("Unlit/Transparent");
			Debug.LogWarning("Custom highlight shader not found, using fallback");
		}

		Material mat = new Material(highlightShader);
		
		if (highlightShader.name.Contains("PlayerRevealHighlight"))
		{
			mat.SetColor("_HighlightColor", highlightColor);
			mat.SetFloat("_PulseSpeed", 3f);
			mat.SetFloat("_IntensityMin", 0.4f);
			mat.SetFloat("_IntensityMax", 1f);
			mat.SetFloat("_EffectIntensity", 1f);
		}
		else
		{
			mat.color = highlightColor;
		}

		return mat;
	}

	// Public methods
	public bool IsRevealing => isRevealing;
	
	public float RevealTimeRemaining 
	{ 
		get 
		{ 
			if (useNewPredatorVision && predatorVisionController != null)
			{
				return predatorVisionController.IsRevealActive() ? 1f : 0f;
			}
			return isRevealing ? revealDuration : 0f; 
		} 
	}

	public void ForceEndReveal()
	{
		if (isRevealing)
		{
			if (useNewPredatorVision && predatorVisionController != null)
			{
				predatorVisionController.DeactivatePredatorVision();
			}
			else
			{
				StopAllCoroutines();
				
				foreach (var kvp in originalMaterials)
				{
					RemoveHighlightFromPlayer(kvp.Key);
				}
				
				originalMaterials.Clear();
			}
			
			isRevealing = false;
		}
	}

	public void SetRevealDuration(float duration)
	{
		revealDuration = Mathf.Max(0.1f, duration);
	}

	public void SetRevealRange(float range)
	{
		revealRange = Mathf.Max(1f, range);
	}

	public void TogglePredatorVisionMode(bool useNewSystem)
	{
		this.useNewPredatorVision = useNewSystem;
		
		if (useNewSystem && predatorVisionController == null)
		{
			predatorVisionController = gameObject.AddComponent<SimplePredatorVisionController>();
		}
	}

	void OnDestroy()
	{
		if (highlightMaterial != null)
		{
			Destroy(highlightMaterial);
		}
		
		ForceEndReveal();
	}

	[ContextMenu("Test Reveal")]
	public void TestReveal()
	{
		if (GetCurrentStamina() >= staminaCost)
		{
			StartReveal();
		}
		else
		{
			Debug.Log("Not enough stamina for reveal test");
		}
	}

	[ContextMenu("Force End Reveal")]
	public void TestForceEnd()
	{
		ForceEndReveal();
	}

	[ContextMenu("Toggle Predator Vision Mode")]
	public void TestToggleMode()
	{
		TogglePredatorVisionMode(!useNewPredatorVision);
		Debug.Log($"Switched to {(useNewPredatorVision ? "New Predator Vision" : "Original")} mode");
	}
}