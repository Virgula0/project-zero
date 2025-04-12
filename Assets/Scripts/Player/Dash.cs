using UnityEngine;
using System.Collections;

public class StandardDash : MonoBehaviour, IDash
{
    private Rigidbody2D body;
    private float elapsedTimeFromLatestDash = float.PositiveInfinity;
    private float dashCooldownInSeconds = 3f;
    private float dashSpeed;
    private bool isDashing = false;

    [SerializeField] private float dashDuration = 0.3f; // Total time for the dash
    [SerializeField] private float dashDistance = 7f;

    public bool IsDashing => isDashing; // property 

    public IDash New(Rigidbody2D body)
    {
        this.body = body;
        this.dashSpeed = dashDistance * 2 / dashDuration;
        return this;
    }

    public bool Dash(Vector2 direction, bool dashRequested)
    {
        elapsedTimeFromLatestDash += Time.fixedDeltaTime;

        if (elapsedTimeFromLatestDash < dashCooldownInSeconds)
            return false;

        if (dashRequested && !isDashing)
        {
            elapsedTimeFromLatestDash = 0;
            StartCoroutine(DoDash(direction));
            return true;
        }
        
        return false;
    }

    private IEnumerator DoDash(Vector2 direction)
    {
        isDashing = true;
        // Apply the calculated dash speed in the desired direction
        body.linearVelocity = direction.normalized * dashSpeed;

        // Sustain the dash for the full duration
        yield return new WaitForSeconds(dashDuration);

        body.linearVelocity = Vector2.zero;
        isDashing = false;
    }
}