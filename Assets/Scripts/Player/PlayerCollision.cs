using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Apple"))
        {
            gameManager.AddHeal(5);
            collision.gameObject.SetActive(false);
        }

        else if(collision.CompareTag("Key"))
        {
            collision.gameObject.SetActive(false);
            Debug.Log("WIN");
        }

        else if(collision.CompareTag("Diamond"))
        {
            collision.gameObject.SetActive(false);
            gameManager.AddDiamond(1);
        }
    }
}
