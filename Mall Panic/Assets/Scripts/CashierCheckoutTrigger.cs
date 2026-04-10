using UnityEngine;

public class CashierCheckoutTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        HandleCheckout(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCheckout(collision.gameObject);
    }

    private void HandleCheckout(GameObject otherObject)
    {
        if (otherObject == null)
        {
            return;
        }

        AIShopperController aiShopper = otherObject.GetComponent<AIShopperController>();
        if (aiShopper != null)
        {
            aiShopper.NotifyReachedCashier();
            return;
        }

        if (!otherObject.CompareTag(playerTag))
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.State == GameManager.GameState.Playing)
        {
            GameManager.Instance.CheckoutPlayer();
        }
    }
}
