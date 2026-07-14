using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Diperlukan untuk pindah scene

public class GameTimer : MonoBehaviour
{
    [Header("Pengaturan Waktu")]
    public float sisaWaktu = 60f;
    public bool timerBerjalan = true;

    [Header("UI Reference")]
    public TextMeshProUGUI teksTimer;
    public GameObject panelGameOver; // Slot untuk Panel Game Over (opsional)

    void Start()
    {
        // Pastikan panel disembunyikan saat game dimulai
        if (panelGameOver != null) panelGameOver.SetActive(false);
    }

    void Update()
    {
        if (timerBerjalan)
        {
            if (sisaWaktu > 0)
            {
                sisaWaktu -= Time.deltaTime;
                UpdateUI();
            }
            else
            {
                sisaWaktu = 0;
                timerBerjalan = false;
                TriggerGameOver();
            }
        }
    }

    void UpdateUI()
    {
        int detik = Mathf.CeilToInt(sisaWaktu);
        teksTimer.text = "Waktu: " + detik.ToString();
    }

    public void TriggerGameOver()
    {
        timerBerjalan = false;
        Debug.Log("Game Over!");

        // Opsi 1: Mengaktifkan Panel Game Over di UI
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            Time.timeScale = 0f; // Menghentikan pergerakan game
        }

        // Opsi 2: Pindah ke Scene "GameOver"
        // SceneManager.LoadScene("GameOverScene"); 
    }
}