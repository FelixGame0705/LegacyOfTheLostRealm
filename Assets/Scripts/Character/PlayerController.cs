using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float healAmount = 10f;
    [SerializeField] private float knockbackForce = 5f; // Lực đẩy ngược lại khi bị sát thương
    [SerializeField] private float knockbackDuration = 0.2f; // Thời gian chịu lực đẩy

    [SerializeField] private float currentHealth;
    private int currentLevel;

    private Vector2 moveInput;
    private Rigidbody2D myRigidbody;
    private Animator myAnimator;
    private Collider2D playerCollider;

    private bool isOnGround;
    private bool isLaddering;
    private bool isLadderingStop;
    private bool isPushing;
    private bool isTakingDamage;
    private bool isDead = false;

    private const string ENEMY_TAG = "Enemy";
    private const string HEAL_TAG = "Heal";

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = playerStats.maxHealth;
        currentLevel = 1;

        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTakingDamage) // Chỉ cho phép di chuyển khi không bị tấn công
        {
            CheckGroundCollision();
            HandleMovement();
            CheckPush();
        }
        UpdateAnimationState();
    }

    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
        Debug.Log($"Move Input: {moveInput}");
    }
    private void HandleMovement()
    {
        Run();
        if (!isOnGround)
        {
            Climb();
        }
        FlipSprite();
    }


    private void Climb()
    {
        if (Mathf.Abs(moveInput.y) > Mathf.Epsilon || Mathf.Abs(moveInput.x) > Mathf.Epsilon)
        {
            if (isDead) return;
            isLaddering = true;
            isLadderingStop = false;

            Vector2 climbVelocity = new Vector2(moveInput.x * playerStats.maxSpeed, moveInput.y * playerStats.maxSpeed);
            myRigidbody.linearVelocity = climbVelocity;
        }
        else if (!isLadderingStop)
        {
            if (isDead) return;
            isLaddering = false;
            isLadderingStop = true;

            myRigidbody.linearVelocity = new Vector2(myRigidbody.linearVelocity.x, 0);
        }
    }

    private void Run()
    {
        Vector2 playerlinearVelocity = new Vector2(moveInput.x * playerStats.maxSpeed, moveInput.y * playerStats.maxSpeed);
        myRigidbody.linearVelocity = playerlinearVelocity;
        isLaddering = false;
        isLadderingStop = false;
    }

    private void CheckGroundCollision()
    {
        isOnGround = playerCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    private void CheckPush()
    {
        Vector2 rayDirection = new Vector2(Mathf.Sign(transform.localScale.x), 0);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, 0.5f, playerStats.pushableLayer);

        if (hit.collider != null && Mathf.Abs(moveInput.x) > Mathf.Epsilon)
        {
            isPushing = true;
        }
        else
        {
            isPushing = false;
        }

        Debug.DrawRay(transform.position, rayDirection * 0.5f, Color.red);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == HEAL_TAG)
        {
            HealPlayer();
            Destroy(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(ENEMY_TAG))
        {
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            TakeDamage(knockbackDirection);
        }
    }

    private void HealPlayer()
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, playerStats.maxHealth);
        Debug.Log($"Player healed! Current Health: {currentHealth}/{playerStats.maxHealth}");
    }

    private void TakeDamage(Vector2 knockbackDirection)
    {
        if (isTakingDamage) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        isTakingDamage = true;
        myAnimator.SetBool("isTakingDamage", true);
        Debug.Log("Player is taking damage!");

        StartCoroutine(ApplyKnockback(knockbackDirection));
    }


    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        float knockbackTime = 0.1f;
        Vector2 knockbackForce = direction * 10f;

        myRigidbody.linearVelocity = Vector2.zero;
        myRigidbody.AddForce(knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackTime);

        myRigidbody.linearVelocity = Vector2.zero;
        RecoverFromDamage();
    }

    private void RecoverFromDamage()
    {
        isTakingDamage = false;
        Debug.Log("Player recovered from damage!");

        myAnimator.SetBool("isTakingDamage", false);
    }

    private void Die()
    {
        Debug.Log("Player died!");
        myAnimator.SetBool("isDead", true);
        isDead = true;
        GetComponent<PlayerController>().enabled = false;
        Destroy(gameObject, 2f);
    }


    private void FlipSprite()
    {
        if (Mathf.Abs(moveInput.x) > Mathf.Epsilon)
        {
            float newRotationY = moveInput.x > 0 ? 0 : 180;
            if (transform.localEulerAngles.y != newRotationY)
            {
                transform.localEulerAngles = new Vector3(0, newRotationY, 0);
            }
        }
    }

    private void UpdateAnimationState()
    {
        if(isDead) return;
        if (isTakingDamage) return; // Không cập nhật trạng thái khác khi đang bị tấn công

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.linearVelocity.x) > Mathf.Epsilon && isOnGround;
        myAnimator.SetBool("isWalking", playerHasHorizontalSpeed);
        myAnimator.SetBool("isLadderingStop", isLadderingStop);
        myAnimator.SetBool("isLaddering", isLaddering);
        myAnimator.SetBool("isPushing", isPushing);
        //myAnimator.SetBool("isTakingDamage", isTakingDamage);
    }

}
