using TMPro;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [Header("References")]
    public Grid grid;
    public LevelGenerator levelGenerator;
    public Tweener tweener;
    public Animator animator;
    public ParticleSystem dustParticles;
    public ParticleSystem wallParticles;
    public TMP_Text scoreText;
    
    [Header("Audio")]
    public AudioSource movementAudio;
    public AudioClip pelletMovementAudio;
    public AudioClip normalMovementAudio;
    public AudioSource wallAudioSource;
    public AudioClip wallHitAudio;
    public AudioSource cherryAudioSource;
    public AudioClip cherryEatAudio;

    [Header("Movement")]
    public float speed = 5f;

    private int score;
    private bool isLerping;
    private Vector3Int currentCell;
    private string lastInput = "";
    private string currentInput = "";
    private bool blocked = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(currentCell);

        animator.SetInteger("Direction", 3);

        SetMovingEffects(false);
        animator.speed = 0f;

        bool hasPellet = levelGenerator.IsPelletCell(currentCell);
        AudioClip clip = hasPellet ? pelletMovementAudio : normalMovementAudio;
        movementAudio.clip = clip;
        movementAudio.Play();
        if (levelGenerator.CollectPellet(currentCell))
        {
            AddScore(10);
        }
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

        if (levelGenerator == null)
        {
            return false;
        }

        if (!levelGenerator.IsWalkableCell(nextCell))
        {
            if (levelGenerator.IsTeleporter(nextCell))
            {
                int oppositeColumn;

                if (nextCell.x < 0)
                {
                    oppositeColumn = levelGenerator.fullMap.GetLength(1) - 1;
                }

                else if (nextCell.x >= levelGenerator.fullMap.GetLength(1))
                {
                    oppositeColumn = 0;
                }

                else
                {
                    return false;
                }

                Vector3Int teleportCell = new Vector3Int(oppositeColumn, currentCell.y, currentCell.z);

                if (!levelGenerator.IsWalkableCell(teleportCell))
                {
                    return false;
                }

                currentCell = teleportCell;
                transform.position = grid.GetCellCenterWorld(currentCell);

                blocked = false;
                animator.speed = 1f;
                SetAnimationDirection(grid.GetCellCenterWorld(currentCell) - grid.GetCellCenterWorld(currentCell - offset));

                return true;
            }

            if (!blocked)
            {
                wallParticles.Play();
                wallAudioSource.PlayOneShot(wallHitAudio);
                blocked = true;
            }

            SetMovingEffects(false);
            animator.speed = 0f;

            return false;
        }

        blocked = false;
        animator.speed = 1f;

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
            if (levelGenerator.CollectPellet(currentCell))
            {
                AddScore(10);
            }
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
    }

    private void AddScore(int points)
    {
        score += points;

        if (scoreText != null)
        {
            scoreText.text = score.ToString("D6");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cherry"))
        {
            if (movementAudio != null && cherryEatAudio != null)
            {
                cherryAudioSource.PlayOneShot(cherryEatAudio);
            }

            AddScore(100);

            Destroy(other.gameObject);
        }
    }
}
