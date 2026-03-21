using UnityEngine;
using UnityEngine.SceneManagement; // 

public class SceneController : MonoBehaviour
{
    // 
    public void ReturnToMainMenu()
    {
        // 
        SceneManager.LoadScene("MainMenu"); 
    }
}