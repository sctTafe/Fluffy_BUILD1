using System.Linq.Expressions;
using UnityEngine;

/// <summary>
/// srcipt for variables that may be used between different script (better than having a bunch of get componts in between everything)
/// for example the mutant might not be allowed to use abilitys while grabbing a player so the is grabbing bool could be held here so other scripts can check here 
/// bad example is the grabbing functions target tag becuase why would other script need to know what the grab script can grab 
/// </summary>
public class MutantVariables : MonoBehaviour
{
    [Header("MutantEnergy")]
    int maxEnery = 100;
    int energy;

    [Header("Grab Variables")]
    public bool isGrabbing = false;
    //maybe for ui 
    //private bool isBiteOnCooldown = false;
    //private float biteCooldown;

    private void Awake()
    {
        energy = maxEnery;
    }




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void changeEnergy(int amount)
    {
        int newVal = energy + amount;
        if (newVal > maxEnery) energy = maxEnery;
        else if (newVal < 0) energy = 0; //tho im sure we just dont want to allow this (will have a function that checks this before change) 
        else energy = newVal;
    }
}
