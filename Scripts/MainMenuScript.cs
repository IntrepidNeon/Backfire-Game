using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{


	public void PlayGame() {
		SceneManager.LoadScene(1);
	}

	public void OpenShop() {
		SceneManager.LoadScene(2);
	}

	public void QuitGame() {
		Application.Quit();
	}
}
