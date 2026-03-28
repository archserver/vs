using TMPro;
using UnityEngine;

public class DamageNumbers : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    private float moveSpeed;

    // move at a random speed for asthetics
    void Start()
    {
        moveSpeed = Random.Range(0.1f, 2f);
        Destroy(gameObject, 1);  
    }

    // move the number
    void Update()
    {
       transform.position += Vector3.up * Time.deltaTime * moveSpeed;
    }

    // set the text to the value coming in
    public void SetText(int value)
    {
        damageText.text = value.ToString();
    }
}
