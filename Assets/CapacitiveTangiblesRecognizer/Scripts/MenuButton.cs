using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour {

	public void changeScene (string scene){
        SceneManager.LoadScene(scene);
    }

    public void closeApplication()
    {
        Application.Quit();
    }
}
