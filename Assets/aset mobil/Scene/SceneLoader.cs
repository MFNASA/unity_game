using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadLevel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene 1");
    }

    public void ExitGame() // untuk tombol "keluar" kalau perlu
    {
        Application.Quit();
    }
}