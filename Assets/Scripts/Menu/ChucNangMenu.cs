using UnityEngine;
using UnityEngine.SceneManagement;
public class ChucNangMenu : MonoBehaviour
{
    public void ChoiGame()
    {
        GameObject menuTransition = Instantiate(Resources.Load<GameObject>("UI/SceneTransition"));

        menuTransition.GetComponent<MenuTransition>().switchScene(1);
    }
    public void ThoatRaMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void ThoatGame()
    {
        Application.Quit();
        //Debug.Log("Thoát Game");
    }
    public void HuongDan()
    {
        //SceneManager.LoadScene("HuongDan");
    }


}

    