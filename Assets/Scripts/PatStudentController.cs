using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    public LevelGenerator levelGenerator;
    public Tweener tweener;
    public Animator animator;
    public ParticleSystem dustParticles;
    
    [Header("Audio")]
    public AudioSource movementAudio;
    public AudioClip pelletMovementAudio;
    public AudioClip normalMovementAudio;

    [Header("Movement")]
    public float speed = 5f;

    private bool isLerping;
    private Vector3Int currentCell;
    private string lastInput = "";
    private string currentInput = "";

    void Start()
    {
        animator = GetComponent<Animator>();

        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(currentCell);

        animator.SetInteger("Direction", 3);

        SetMovingEffects(false);
    }
    
    void Update()
    {
        ReadInputs();

        if (isLerping)
        {
            return;
        }

        if (TryMove(lastInput))
        {
            return;
        }

        if (TryMove(currentInput))
        {
            return;
        }

        SetMovingEffects(false);
    }
    private void ReadInputs()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            lastInput = "up";
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            lastInput = "left";
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = "down";
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = "right";
        }
    }

    private bool TryMove(string direction)
    {
        if (string.IsNullOrEmpty(direction))
        {
            return false;
        }

        Vector3Int offset;

        switch (direction)
        {
            case "up":
                offset = Vector3Int.up;
                break;
            case "down":
                offset = Vector3Int.down;
                break;
            case "left":
                offset = Vector3Int.left;
                break;
            case "right":
                offset = Vector3Int.right;
                break;
            default:
                return false;
        }

        Vector3Int nextCell = currentCell + offset;

        if (levelGenerator == null || !levelGenerator.IsWalkableCell(nextCell))
        {
            return false;
        }

        Vector3 startPosition = grid.GetCellCenterWorld(currentCell);
        Vector3 endPosition = grid.GetCellCenterWorld(nextCell);
        Vector3 directionVector = endPosition - startPosition;

        currentInput = direction;
        SetAnimationDirection(directionVector);
        UpdateMovementAudio(nextCell);

        if (dustParticles != null && !dustParticles.isPlaying)
        {
            dustParticles.Play();
        }

        float distance = Vector3.Distance(startPosition, endPosition);
        float duration = distance / Mathf.Max(speed, 0.01f);

        isLerping = true;

        bool started = tweener.AddTween(transform, startPosition, endPosition, duration, () => 
        {
            currentCell = nextCell;
            isLerping = false;
        }
        );

        if (!started)
        {
            isLerping = false;
            SetMovingEffects(false);
            return false;
        }

        return true;
    }

    private void SetAnimationDirection(Vector3 direction)
    {
        if (direction.y > 0)
        {
            animator.SetInteger("Direction", 0);
        }

        else if (direction.y < 0)
        {
            animator.SetInteger("Direction", 1);
        }

        else if (direction.x < 0)
        {
            animator.SetInteger("Direction", 2);
        }

        else if (direction.x > 0)
        {
            animator.SetInteger("Direction", 3);
        }
    }

    private void UpdateMovementAudio(Vector3Int nextCell)
    {
        movementAudio.Stop();
        if (movementAudio == null || levelGenerator.tileMap == null)
        {
            return;
        }

        bool hasPellet = levelGenerator.IsPelletCell(nextCell);
        AudioClip clip = hasPellet ? pelletMovementAudio : normalMovementAudio;

        if (clip == null)
        {
            movementAudio.Stop();
            return;
        }

        if (movementAudio.clip != clip || !movementAudio.isPlaying)
        {
            movementAudio.clip = clip;
            movementAudio.loop = false;
            movementAudio.Play();
        }
    }

    private void SetMovingEffects(bool moving)
    {
        if (dustParticles != null)
        {
            if (moving && !dustParticles.isPlaying)
            {
                dustParticles.Play();
            }
            else if (!moving && dustParticles.isPlaying)
            {
                dustParticles.Stop();
            }
        }

        if (!moving && movementAudio != null)
        {
            movementAudio.Stop();
        }
    }
}
