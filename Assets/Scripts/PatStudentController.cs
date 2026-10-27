using System.Collections;
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

    [Header("Game State")]
    public GameManager gameManager;

    public ParticleSystem deathParticles;
    public AudioClip deathSound;
    public AnimationClip playerDeathAnimationClip;

    private bool isDying;
    private Vector3 startingPosition;
    private int score;
    private bool isLerping;
    private Vector3Int currentCell;
    private string lastInput = "";
    private string currentInput = "";
    private bool blocked = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        startingPosition = transform.position;

        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(currentCell);

        animator.SetInteger("Direction", 3);

        SetMovingEffects(false);
        animator.speed = 0f;

        bool hasPellet = levelGenerator.IsPelletCell(currentCell);
        AudioClip clip = hasPellet ? pelletMovementAudio : normalMovementAudio;
        movementAudio.clip = clip;
        movementAudio.Play();
        int itemID = levelGenerator.CollectItem(currentCell);

        if (itemID == 5)
        {
            gameManager.AddScore(10);
        }

        else if (itemID == 6)
        {
            gameManager.AddScore(50);
            gameManager.ActivatePowerPill();
        }
    }
    
    void Update()
    {
        if (isDying)
        {
            return;
        }

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
            int itemID = levelGenerator.CollectItem(currentCell);

            if (itemID == 5)
            {
                gameManager.AddScore(10);
            }

            else if (itemID == 6)
            {
                gameManager.AddScore(50);
                gameManager.ActivatePowerPill();
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

    
    private IEnumerator PlayerDeathRoutine()
    {
        if (isDying)
        {
            yield break;
        }

        isDying = true;

        lastInput = "";
        currentInput = "";

        if (movementAudio != null)
        {
            movementAudio.Stop();
        }

        if (cherryAudioSource != null)
        {
            cherryAudioSource.Stop();
        }
        if (wallAudioSource != null)
        {
            wallAudioSource.Stop();
        }

        SetMovingEffects(false);
        gameManager.FreezeGhosts();
        gameManager.LoseLife();

        if (deathParticles != null)
        {
            deathParticles.transform.position = transform.position;
            deathParticles.Play();
        }

        if (movementAudio != null && deathSound != null)
        {
            movementAudio.PlayOneShot(deathSound);
        }

        if (animator != null && playerDeathAnimationClip != null)
        {
            animator.Play(playerDeathAnimationClip.name, 0, 0f);

            yield return new WaitForSeconds(playerDeathAnimationClip.length);
        }

        else
        {
            yield return new WaitForSeconds(1f);
        }

        transform.position = startingPosition;
        currentCell = grid.WorldToCell(startingPosition);
        transform.position = grid.GetCellCenterWorld(currentCell);

        gameManager.ResetGhosts();

        if (animator != null)
        {
            animator.SetInteger("Direction", 3);
            animator.speed = 0f;
        }

        isLerping = false;
        blocked = false;

        lastInput = "";
        currentInput = "";

        if (gameManager.Lives > 0)
        {
            isDying = false;
            gameManager.ResumeGhosts();
        }

        else
        {
            Debug.Log("Game Over");
            // Game Over sequence to be done later
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying || gameManager == null)
            return;

        if (other.CompareTag("Cherry"))
        {
            if (movementAudio != null && cherryEatAudio != null)
                movementAudio.PlayOneShot(cherryEatAudio);

            gameManager.AddScore(100);
            Destroy(other.gameObject);
            return;
        }

        GhostController ghost = other.GetComponent<GhostController>();

        if (ghost == null)
        {
            return;
        }

        if (ghost.CurrentState == GhostState.Dead)
        {
            return;
        }

        if (ghost.CurrentState == GhostState.Scared || ghost.CurrentState == GhostState.Recovering)
        {
            gameManager.EatGhost(ghost);
            return;
        }

        if (ghost.CurrentState == GhostState.Normal)
        {
            StartCoroutine(PlayerDeathRoutine());
        }
    }
}
