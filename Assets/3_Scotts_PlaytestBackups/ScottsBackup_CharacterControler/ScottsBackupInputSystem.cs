using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

//namespace StarterAssets
//{
public class ScottsBackupInputSystem : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public bool jump;
	public bool sprint;
	public bool attack;

	[Header("Character Interaction Input Values")]
	public bool interaction1;
	public bool interaction2;
	public bool interaction3;
	public bool interaction4;

	[Header("Movement Settings")]
	public bool analogMovement;

	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
	public void OnMove(InputValue value)
	{
		MoveInput(value.Get<Vector2>());
	}
	public void OnLook(InputValue value)
	{
		if (cursorInputForLook)
		{
			LookInput(value.Get<Vector2>());
		}
	}
	public void OnJump(InputValue value)
	{
		JumpInput(value.isPressed);
	}
	public void OnSprint(InputValue value)
	{
		SprintInput(value.isPressed);
	}

	public void OnInteract1(InputValue value)
	{
		Interact1(value.isPressed);
	}
	public void OnInteract2(InputValue value)
	{
		Interact2(value.isPressed);
	}
	public void OnInteract3(InputValue value)
	{
		Interact3(value.isPressed);
	}
	public void OnInteract4(InputValue value)
	{
		Interact4(value.isPressed);
	}
	public void OnAttack(InputValue value)
	{
		Attack(value.isPressed);
	}
#endif

	public void MoveInput(Vector2 newMoveDirection)
	{
		move = newMoveDirection;
	}
	public void LookInput(Vector2 newLookDirection)
	{
		look = newLookDirection;
	}
	public void JumpInput(bool newJumpState)
	{
		jump = newJumpState;
	}
	public void SprintInput(bool newSprintState)
	{
		sprint = newSprintState;
	}
	public void Interact1(bool i)
	{
		interaction1 = i;
	}
	public void Interact2(bool i)
	{
		interaction2 = i;
	}
	public void Interact3(bool i)
	{
		interaction3 = i;
	}
	public void Interact4(bool i)
	{
		interaction4 = i;
	}
	public void Attack(bool i)
	{
		attack = i;
	}
	/*
    private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	
	*/
}
//}