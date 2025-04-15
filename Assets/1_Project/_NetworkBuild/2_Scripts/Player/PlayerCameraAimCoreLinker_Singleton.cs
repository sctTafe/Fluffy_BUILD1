using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerAimCoreLinker_Singleton : Singleton<PlayerAimCoreLinker_Singleton> {

    public PlayerCameraAimController aimController;
    public PositionConstraint posConstraint;

    private void Awake() {
        //aimController = GetComponent<PlayerCameraAimController>();
    }

    public void AssignAimCoreTarget(AnimalCharacter character) {
        

        if (aimController == null) return;
        {
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = character.transform;
            source.weight = 1f;
            posConstraint.SetSource(0, source);

            aimController.m_Controller = character;
            aimController.enabled = true;
        }
    }
}

