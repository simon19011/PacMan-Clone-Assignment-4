using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Ghosts")]
    public GhostController[] ghosts;

    [Header("Player")]
    public PacStudentController player;

    [Header("UI")]
    public TMP_Text scoreText;
    public Image[] lifeImages;
    public TMP_Text ghostTimerHeading;
    public TMP_Text ghostTimerText;

    [Header("Lives")]
    public int startingLives = 3;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip introMusic;
    public AudioClip normalMusic;
    public AudioClip scaredMusic;
    public AudioClip ghostDeadMusic;
    public float introDuration = 3f;

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioClip ghostEatSound;

    public int Score { get; private set; }
    public int Lives { get; private set; }

    public float ScaredTimeRemaining { get; private set; }

    public bool PowerActive => ScaredTimeRemaining > 0f;

    private Coroutine powerRoutine;

    private void Start()
    {
        Lives = Mathf.Min(startingLives, lifeImages != null ? lifeImages.Length : startingLives);

        UpdateLivesUI();


        if (ghostTimerHeading != null)
        {
            ghostTimerHeading.gameObject.SetActive(false);
        }

        if (ghostTimerText != null)
        {
            ghostTimerHeading.gameObject.SetActive(false);
            ghostTimerText.gameObject.SetActive(false);
        }

        UpdateUI();

        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        if (introMusic != null && musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = introMusic;
            musicSource.loop = false;
            musicSource.Play();

            yield return new WaitForSeconds(introDuration);
        }

        PlayMusic(normalMusic);
    }

    public void AddScore(int points)
    {
        Score += points;
        UpdateUI();
    }

    public void ActivatePowerPill()
    {
        if (powerRoutine != null)
        {
            StopCoroutine(powerRoutine);
        }

        powerRoutine = StartCoroutine(PowerPillRoutine());
    }

    private IEnumerator PowerPillRoutine()
    {
        ScaredTimeRemaining = 10f;

        if (ghostTimerHeading != null)
        {
            ghostTimerHeading.gameObject.SetActive(true);
        }

        if (ghostTimerText != null)
        {
            ghostTimerText.gameObject.SetActive(true);
        }

        foreach (GhostController ghost in ghosts)
        {
            if (ghost != null && ghost.CurrentState != GhostState.Dead)
            {
                ghost.SetState(GhostState.Scared);
            }
        }

        PlayMusic(scaredMusic);

        while (ScaredTimeRemaining > 0f)
        {
            if (ghostTimerText != null)
            {
                ghostTimerText.text = Mathf.CeilToInt(ScaredTimeRemaining).ToString();
            }

            if (ScaredTimeRemaining <= 3f)
            {
                foreach (GhostController ghost in ghosts)
                {
                    if (ghost != null && ghost.CurrentState == GhostState.Scared)
                    {
                        ghost.SetState(GhostState.Recovering);
                    }
                }
            }

            yield return null;
            ScaredTimeRemaining = Mathf.Max(0f, ScaredTimeRemaining - Time.deltaTime);
        }

        foreach (GhostController ghost in ghosts)
        {
            if (ghost != null && ghost.CurrentState != GhostState.Dead)
            {
                ghost.SetState(GhostState.Normal);
            }
        }

        if (ghostTimerHeading != null)
        {
            ghostTimerHeading.gameObject.SetActive(false);
        }

        if (ghostTimerText != null)
        {
            ghostTimerText.gameObject.SetActive(false);
        }

        PlayMusic(normalMusic);
        powerRoutine = null;
    }

    public void EatGhost(GhostController ghost)
    {
        if (ghost == null || ghost.CurrentState == GhostState.Dead)
        {
            return;
        }

        ghost.SetMovementAllowed(false);
        ghost.SetState(GhostState.Dead);

        AddScore(300);

        if (sfxSource != null && ghostEatSound != null)
        {
            sfxSource.PlayOneShot(ghostEatSound);
        }
        
        PlayMusic(ghostDeadMusic);

        StartCoroutine(GhostRespawnRoutine(ghost));
    }

    private IEnumerator GhostRespawnRoutine(GhostController ghost)
    {
        yield return new WaitForSeconds(3f);

        if (ghost == null)
        {
            yield break;
        }

        ghost.transform.position = ghost.StartingPosition;

        if (!PowerActive)
        {
            ghost.SetState(GhostState.Normal);
        }

        else if (ScaredTimeRemaining <= 3f)
        {
            ghost.SetState(GhostState.Recovering);
        }

        else
        {
            ghost.SetState(GhostState.Scared);
        }

        ghost.SetMovementAllowed(true);

        PlayMusic(PowerActive ? scaredMusic : normalMusic);
    }

    public void LoseLife()
    {
        if (Lives <= 0)
            return;

        Lives--;
        UpdateLivesUI();
    }

    public void FreezeGhosts()
    {
        foreach (GhostController ghost in ghosts)
        {
            if (ghost != null)
                ghost.SetMovementAllowed(false);
        }
    }

    public void ResetGhosts()
    {
        foreach (GhostController ghost in ghosts)
        {
            if (ghost != null)
                ghost.ResetGhost();
        }
    }

    public void ResumeGhosts()
    {
        foreach (GhostController ghost in ghosts)
        {
            if (ghost != null &&
                ghost.CurrentState != GhostState.Dead)
            {
                ghost.SetMovementAllowed(true);
            }
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = Score.ToString("D6");
    }

    private void UpdateLivesUI()
    {
        if (lifeImages == null)
            return;

        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].gameObject.SetActive(i < Lives);
            }
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }
}
