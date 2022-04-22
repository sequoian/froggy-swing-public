using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("References")]
    public Game gameManager;
    public DeathAnimation body;
    public GameObject tongueTip;
    public AudioSource grappleAudio;
    public AudioSource deathAudio;

    [Header("Gravity")]
    public float gravity;
    public float yKillZone;

    [Header("Grapple Hook")]
	public float grappleAngle;
    public float grappleCastDistance;
    public LayerMask grappleLayerMask;

    [Header("Swinging")]
    public float swingAnglePerSecond;
    public float swingSpeed;

    // Constants
    Vector2 grappleDirection;
    
    // States
	const int numStates = 3;
	const int normalState = 0;
	const int swingState = 1;
    const int deathState = 2;

    // References
    LineRenderer lineRenderer;
    
    // Private
    Vector3 velocity;
    Vector3 positionPrev;
    Quaternion rotationPrev;
    Quaternion angularVelocity;
    StateMachine stateMachine;
    Vector3 spawnPoint;
    Vector2 hookPosition;
    float grappleLength;

    void Start()
    {
        // Calculate the direction of the grapple raycast.
        float angle = grappleAngle * Mathf.Deg2Rad;
        grappleDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        // Set references.
        lineRenderer = GetComponent<LineRenderer>();
        tongueTip.SetActive(false);

        // Init state machine.
        stateMachine = new StateMachine(numStates);
        stateMachine.AddState(normalState, NormalUpdate, null, null, null);
        stateMachine.AddState(swingState, SwingUpdate, null, null, null);
        stateMachine.AddState(deathState, DeathUpdate, null, null, null);
        stateMachine.SetState(normalState);

        // Store spawn point.
        spawnPoint = transform.position;
    }

    void Update()
    {
        stateMachine.Update();

        positionPrev = transform.position;
        rotationPrev = transform.rotation;

        if (transform.position.y <= yKillZone && stateMachine.GetState() != deathState)
        {
            // Kill the player if they fall below the camera.
            Die();
        }
    }

    void NormalUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            // Cast ray to grapple.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, grappleDirection,
                grappleCastDistance, grappleLayerMask);

            if (hit)
            {
                // If cast hit a wall, set variables needed for swinging and enter swing state.
                hookPosition = hit.point;
                grappleLength = (hookPosition - (Vector2)transform.position).magnitude;
                stateMachine.SetState(swingState);

                // Play the grapple sound.
                grappleAudio.PlayOneShot(grappleAudio.clip);
            }
        }

        // Apply gravity.
        velocity += new Vector3(0, -gravity * Time.deltaTime, 0);

        // Move player.
        transform.position = transform.position + velocity;

        // Spin player.
        transform.Rotate(angularVelocity.eulerAngles);

        // Hide rope.
        lineRenderer.enabled = false;
        tongueTip.SetActive(false);
    }

    void SwingUpdate()
    {
        // Rotate player. I don't know how this works.
        transform.RotateAround(hookPosition, Vector3.forward, 
            swingAnglePerSecond * 360 / grappleLength * Time.deltaTime);

        // Check the button state every frame. This prevents grappling from persisting if the
        // game is unpaused and the button is not being pressed.
        if (!Input.GetKey(KeyCode.Space) && !Input.GetMouseButton(0))
        {
            // Release grapple.
            stateMachine.SetState(normalState);

            // Set the swing speed to a constant value for consistency.
            Vector3 direction = (transform.position - positionPrev).normalized;
            velocity = direction * swingSpeed;

            // Set spin on player.
            angularVelocity = transform.rotation * Quaternion.Inverse(rotationPrev);
        }

        // Draw rope.
        lineRenderer.enabled = true;
        Vector3[] linePoints = new Vector3[] {transform.position, hookPosition};
        lineRenderer.SetPositions(linePoints);
        tongueTip.SetActive(true);
        tongueTip.transform.position = hookPosition;
        
    }

    void DeathUpdate()
    {
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Helpers.LayerInMask(collision.gameObject.layer, grappleLayerMask)
            && stateMachine.GetState() != deathState)
        {
            Die();
        }
    }

    void Die()
    {
            // Hide rope.
            lineRenderer.enabled = false;
            tongueTip.SetActive(false);

            // Play the death sound.
            deathAudio.Play();

            // Spawn body.
            body.Spawn(transform.position);

            // End game.
            gameManager.PlayerDied();
            stateMachine.SetState(deathState);

            // Hide player.
            GetComponent<SpriteRenderer>().enabled = false;
    }

    public void OnReset()
    {
        // Set position to spawn point.
        transform.position = positionPrev = spawnPoint;

        // Reset rotation.
        transform.rotation = rotationPrev = angularVelocity = Quaternion.identity;

        // Zero out velocity.
        velocity = Vector3.zero;

        // Unhide player.
        GetComponent<SpriteRenderer>().enabled = true;

        // Reset body.
        body.OnReset();
        
        // Set state to normal.
        stateMachine.SetState(normalState);
    }
}
