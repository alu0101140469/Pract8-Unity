using TMPro;
using UnityEngine;

/// <summary>
/// Mantiene la puntuación y la muestra en un texto de TextMeshPro.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    public int CurrentScore { get; private set; }

    private void Start()
    {
        RefreshUI();
    }

    public void AddPoints(int amount)
    {
        CurrentScore += amount;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (scoreText == null)
        {
            Debug.LogError("ScoreManager: asigna el texto TMP en el Inspector.", this);
            return;
        }

        scoreText.text = "Puntuación: " + CurrentScore;
    }
}
