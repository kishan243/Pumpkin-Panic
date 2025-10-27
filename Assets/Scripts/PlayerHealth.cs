using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Slider and Image

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("UI Elements")]
    [SerializeField] private Slider healthSlider; // Drag your Health Bar Slider here
    [SerializeField] private Image damageFlashImage; // Drag your red damage vignette Image here
    [SerializeField] private float flashDuration = 0.15f;

    // --- Singleton Pattern ---
    // This makes it easy for ghosts to find and damage the player
    public static PlayerHealth Instance { get; private set; }

    private void Awake()
    {
        // Set up the singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (damageFlashImage != null)
        {
            damageFlashImage.color = new Color(1f, 0f, 0f, 0f); // Start invisible
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damageAmount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthUI();
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private IEnumerator DamageFlash()
    {
        if (damageFlashImage == null) yield break;

        // Flash red
        damageFlashImage.color = new Color(1f, 0f, 0f, 0.4f); // 40% opacity red
        yield return new WaitForSeconds(flashDuration);

        // Fade out
        float fadeTime = flashDuration * 2;
        float timer = 0;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, timer / fadeTime);
            damageFlashImage.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }

        damageFlashImage.color = new Color(1f, 0f, 0f, 0f); // Ensure fully transparent
    }

    private void Die()
    {
        // TODO: Add your game over logic here
        Debug.Log("Player has died!");
        // For example, you could disable player movement and show a "Game Over" screen
        // this.GetComponent<PlayerPickupDrop>().enabled = false;
        // this.GetComponent<SimpleSampleCharacterControl>().enabled = false;
    }
}
