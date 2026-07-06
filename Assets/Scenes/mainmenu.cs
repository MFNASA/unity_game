using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Panels")]
    [Tooltip("Masukkan GameObject Panel Main Menu ke sini")]
    public GameObject mainMenuPanel;
    
    [Tooltip("Masukkan GameObject Panel Setingan ke sini")]
    public GameObject settingsPanel;

    private void Start()
    {
        // Memastikan saat awal mulai, Main Menu aktif dan Settings non-aktif
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // Fungsi untuk tombol "PLAY CUY"
    public void PlayGame()
    {
        // Memuat scene selanjutnya berdasarkan urutan di Build Settings
        // Pastikan scene game kamu berada di urutan setelah Main Menu
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
        // CATATAN: Jika kamu ingin menggunakan nama scene secara spesifik, 
        // gunakan kode di bawah ini dan beri tanda komentar pada kode di atas:
        SceneManager.LoadScene("GameScene");
    }

    // Fungsi untuk tombol "SETINGAN"
    public void OpenSettings()
    {
        // Menyembunyikan menu utama dan menampilkan menu setingan
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // Fungsi untuk tombol "Kembali" / "Back" di dalam menu setingan
    public void CloseSettings()
    {
        // Menyembunyikan menu setingan dan menampilkan menu utama kembali
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // Fungsi untuk tombol "KELUAR"
    public void QuitGame()
    {
        // Menampilkan pesan di console (hanya terlihat saat di Unity Editor)
        Debug.Log("Game ditutup!");
        
        // Menutup aplikasi (hanya berfungsi jika game sudah di-build)
        Application.Quit();
    }
}