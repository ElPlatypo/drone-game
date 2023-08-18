using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    public float moveSpeed = 5f;     // The speed at which the spaceships move
    public float rotationSpeed = 3f; // The rotation speed of the spaceships

    public GameObject opponent;

    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb.freezeRotation = true; // Freeze rotation to prevent unwanted tilting
    }

    // Update is called once per frame
    void Update()
    {
        // Get the direction to move based on the spaceship's position relative to the other spaceship
        Vector3 moveDirection = (transform.position - GetOpponentPosition()).normalized;
        moveDirection.y = 0f; // Set the y-component to zero to restrict movement to the horizontal plane

        // Move the spaceship
        rb.velocity = moveDirection * moveSpeed;

        // Rotate the spaceship towards the movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // This function returns the position of the opponent spaceship
    private Vector3 GetOpponentPosition()
    {
        if (opponent != null)
        {
            return opponent.transform.position;
        }

        return Vector3.zero; // If no opponent is found, return a default position (0,0,0)
    }
}