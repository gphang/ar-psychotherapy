using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomepageController : MonoBehaviour
{
    public void Begin()
    {
        SceneManager.LoadScene("AvatarSelection");
    }
}
