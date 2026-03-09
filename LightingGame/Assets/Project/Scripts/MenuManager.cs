using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入这个命名空间才能管理场景

public class MenuManager : MonoBehaviour
{
    // 这个方法会被按钮调用
    public void StartGame()
    {
        // 括号里的名字必须和你截图里的场景名字一模一样
        SceneManager.LoadScene("level_1"); 
    }
}