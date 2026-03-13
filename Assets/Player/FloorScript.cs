using UnityEngine;

public class FloorScript : MonoBehaviour
{
    public float speed;

    [SerializeField]
    private Renderer bgRendered;

    // Update is called once per frame
    void Update()
    {
        bgRendered.material.mainTextureOffset += new Vector2(speed * Time.deltaTime, 0);
    }
}
