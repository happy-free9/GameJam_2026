using UnityEngine;

public class PS1Resolution : MonoBehaviour
{
    public int width = 640;
    public int height = 360;
    public bool fullscreen = true;

    void Start()
    {
        Screen.SetResolution(width, height, fullscreen);
    }
}