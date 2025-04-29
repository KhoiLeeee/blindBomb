using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float fuseTime = 3f;
    private float remainingTime;
    private int explosionRadius = 1;

    private void Start()
    {
        remainingTime = fuseTime;
    }

    // Update is called once per frame
    private void Update()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime < 0)
        {
            remainingTime = 0;
        }
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }

    public void SetExplosionRadius(int explosionRadius)
    {
        this.explosionRadius = explosionRadius;
    }
    public int GetExplosionRadius()
    {
        return explosionRadius;
    }
}
