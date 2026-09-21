using UnityEngine;

public class SurvivalEnemyActivator : MonoBehaviour
{
    [SerializeField] private float activationRange = 16f;

    private MonoBehaviour[] behaviours;
    private Rigidbody2D body;
    private Transform player;
    private bool activated;

    public void Configure(float range)
    {
        activationRange = Mathf.Max(1f, range);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        behaviours = GetComponents<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour != null
                && behaviour != this
                && !(behaviour is EnemyHealth2D)
                && !(behaviour is SurvivalEnemyActivator))
            {
                behaviour.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (activated)
        {
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject == null)
            {
                return;
            }

            player = playerObject.transform;
        }

        if (Vector2.Distance(transform.position, player.position) > activationRange)
        {
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }

            return;
        }

        activated = true;
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour != null && behaviour != this)
            {
                behaviour.enabled = true;
            }
        }
    }
}
