using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Put on a UI Slider. The player calls SetMaxHealth once and SetHealth on every change.
/// The fill colour goes from green (full) to red (almost empty).
/// </summary>
[RequireComponent(typeof(Slider))]
public class HealthBarBehaviour : MonoBehaviour
{
    public Image fillImage;                 // Slider > Fill Area > Fill
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;

    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.interactable = false;        // display only, the player can't drag it
    }

    public void SetMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
        SetHealth(maxHealth);
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(lowColor, fullColor, slider.normalizedValue);
        }
    }
}
