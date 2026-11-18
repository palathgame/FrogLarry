using UnityEngine;

public class EatScript : MonoBehaviour
{

    private GameObject flyToAttach; // Reference na objekt Fly, který je navázán na jazyk

    private void Update()
    {
        if (flyToAttach != null)
        {
            Vector3 tonguePosition = transform.position; // Pøedpokládáme, že máte referenci na transform jazyka zaby
            flyToAttach.transform.position = tonguePosition;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var gridManagerScript = GameObject.Find("GridManager").GetComponent<GridManagerScript>();

        if (collision.CompareTag("Fly"))
        {
            flyToAttach = collision.gameObject;
            flyToAttach.GetComponent<FlyScript>().Api("Die");
            gridManagerScript.addCatchFly();
            var effects = GetComponentInParent<FrogEffectsScript>(); // "+1" over Larry
            if (effects != null) effects.FlyEaten();
        }
    }
}
