using UnityEngine;

public enum GhostState
{
    Normal,
    Scared,
    Recovering,
    Dead
}

public class GhostController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public MonoBehaviour movementScript;
    public GhostState CurrentState { get; private set; }
    public Vector3 StartingPosition { get; private set; }
    // 0 = Up, 1 = Down, 2 = Left, 3 = Right
    public int Direction { get; private set; } = 0;
    private bool movementAllowed = true;
    public bool MovementAllowed => movementAllowed;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        StartingPosition = transform.position;

        CurrentState = GhostState.Normal;
        PlayCurrentAnimation();
    }


    public void SetDirection(int direction)
    {
        Direction = Mathf.Clamp(direction, 0, 3);
        PlayCurrentAnimation();
    }

    public void SetState(GhostState newState)
    {
        CurrentState = newState;
        PlayCurrentAnimation();
    }

    private void PlayCurrentAnimation()
    {
        if (animator == null)
            return;

        string animationName;

        switch (CurrentState)
        {
            case GhostState.Normal:
                animationName = "Normal_" + DirectionName();
                break;

            case GhostState.Scared:
                animationName = "Scared_" + DirectionName();
                break;

            case GhostState.Recovering:
                animationName = "Recovering";
                break;

            case GhostState.Dead:
                animationName = "Dead";
                break;

            default:
                return;
        }

        animator.Play(animationName, 0, 0f);
    }

    private string DirectionName()
    {
        switch (Direction)
        {
            case 0: return "Up";
            case 1: return "Down";
            case 2: return "Left";
            case 3: return "Right";
            default: return "Up";
        }
    }

    public void SetMovementAllowed(bool allowed)
    {
        movementAllowed = allowed;

        if (movementScript != null)
            movementScript.enabled = allowed;
    }

    public void ResetGhost()
    {
        SetMovementAllowed(false);
        transform.position = StartingPosition;
        SetDirection(3);
        SetState(GhostState.Normal);
        SetMovementAllowed(true);
    }
}
