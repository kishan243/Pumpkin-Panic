using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("UI Elements")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image damageFlashImage;
    [SerializeField] private float flashDuration = 0.15f;

    public static PlayerHealth Instance { get; private set; }

    private void Awake()
    {
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
            damageFlashImage.color = new Color(1f, 0f, 0f, 0f);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

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

        damageFlashImage.color = new Color(1f, 0f, 0f, 0.4f);
        yield return new WaitForSeconds(flashDuration);

        float fadeTime = flashDuration * 2;
        float timer = 0;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0.4f, 0f, timer / fadeTime);
            damageFlashImage.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }

        damageFlashImage.color = new Color(1f, 0f, 0f, 0f);
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        GameManager.Instance.TriggerLose();
    }
}