using UnityEngine;

public class DamageNumberController : MonoBehaviour
{
    public static DamageNumberController DNCInstance;
    public DamageNumbers damageNumbersPrefab;

    // Check for duplicate instances for additional levels and set DNCInstance pointer
    private void Awake()
    {
        if (DNCInstance != null && DNCInstance != this)
            Destroy(this);
        else
            DNCInstance = this;
    }

    // create a number for a creature whch takes damage
    public void CreateNumber( float value, Vector2 spot)
    {
        DamageNumbers damageNumber = Instantiate(damageNumbersPrefab, spot, transform.rotation, transform);
        damageNumber.SetText(Mathf.RoundToInt(value));
    }
}
