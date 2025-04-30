using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float fuseTime = 3f;
    private float remainingTime;
    private int explosionRadius = 1;

    public void Initialize(float fuse, int radius)
    {
        fuseTime = fuse;
        remainingTime = fuse;
        explosionRadius = radius;
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
    public int GetExplosionRadius()
    {
        return explosionRadius;
    }
}
