using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Hapus baris ini jika tidak memakai TextMeshPro

/// <summary>
/// Script untuk menangani kondisi FINISH / GAME SELESAI.
/// Cara pakai:
/// 1. Buat GameObject kosong (misal "FinishLine") lalu tambahkan Collider (Is Trigger = true).
/// 2. Attach script ini ke GameObject tersebut.
/// 3. Pastikan objek Player memiliki tag "Player".
/// 4. Isi referensi UI (Panel Finish, Teks Waktu, dsb) lewat Inspector.
/// </summary>
public class FinishGame : MonoBehaviour
{
    [Header("Pengaturan Finish")]
    [Tooltip("Tag object yang dianggap sebagai pemain")]
    public string playerTag = "Player";

    [Tooltip("Jeda waktu (detik) sebelum aksi finish dijalankan")]
    public float delayBeforeFinish = 0.5f;

    [Header("UI Saat Finish")]
    public GameObject finishPanel;       // Panel UI yang muncul saat finish
    public TextMeshProUGUI resultText;   // Teks hasil (misal waktu / skor)
    // public Text resultText;           // Alternatif jika pakai UI Text biasa

    [Header("Pengaturan Waktu (Opsional)")]
    public bool useTimer = true;
    private float elapsedTime = 0f;
    private bool isFinished = false;
    private bool isTimerRunning = true;

    [Header("Audio (Opsional)")]
    public AudioSource finishSound;

    void Start()
    {
        if (finishPanel != null)
            finishPanel.SetActive(false);

        elapsedTime = 0f;
        isFinished = false;
        isTimerRunning = true;
    }

    void Update()
    {
        // Hitung waktu berjalan selama belum finish
        if (useTimer && isTimerRunning && !isFinished)
        {
            elapsedTime += Time.deltaTime;
        }
    }

    // Dipanggil otomatis saat ada object lain masuk ke area trigger ini
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isFinished)
        {
            HandleFinish();
        }
    }

    // Versi 2D — hapus fungsi ini jika game kamu 3D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !isFinished)
        {
            HandleFinish();
        }
    }

    private void HandleFinish()
    {
        isFinished = true;
        isTimerRunning = false;

        Debug.Log("Player mencapai garis FINISH!");

        if (finishSound != null)
            finishSound.Play();

        // Jalankan tampilan hasil setelah delay singkat
        Invoke(nameof(ShowFinishUI), delayBeforeFinish);
    }

    private void ShowFinishUI()
    {
        // Hentikan waktu permainan (opsional, hapus jika tidak perlu)
        Time.timeScale = 0f;

        if (finishPanel != null)
            finishPanel.SetActive(true);

        if (resultText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            resultText.text = $"SELESAI!\nWaktu: {minutes:00}:{seconds:00}";
        }
    }

    // ---------- FUNGSI TOMBOL UI (Hubungkan lewat OnClick di Inspector) ----------

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("Tidak ada level selanjutnya. Kembali ke Main Menu.");
            SceneManager.LoadScene(0); // asumsi scene index 0 = Main Menu
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // sesuaikan index scene menu utama
    }
}