using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int heal = 15;
    public int diamond = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddHeal(int points)
    {
        heal += points;
    }

    public void AddDiamond(int points)
    {
        diamond += points;
    }
}
