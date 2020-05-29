using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public Rigidbody2D theRB;
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    private Vector2 mouseInput;
    public float mouseSensitivity = 1f;
    public Camera viewCam;

    // prefab of bullet Impact fx
    public GameObject bulletImpact;
    public int currentAmmo;

    // animation
    public Animator gunAnim;
    public Animator playerAnim;

    // stats
    public int currentHealth;
    public int maxHealth = 100;
    public GameObject deadScreen;
    private bool hasDied;

    public Text healthText, ammoText;
    public GameObject healthBox, ammoBox;



    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthText.text = currentHealth.ToString() + "%";
        ammoText.text = currentAmmo.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasDied)
        {
            //player movement
            moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            Vector3 moveHorizontal = transform.up * -moveInput.x;
            Vector3 moveVertical = transform.right * moveInput.y;
            Vector3 movementAdded = moveHorizontal + moveVertical;
            movementAdded.Normalize();

            theRB.velocity = movementAdded * moveSpeed;

            //player view control
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z - mouseInput.x);

            viewCam.transform.localRotation = Quaternion.Euler(viewCam.transform.localRotation.eulerAngles + new Vector3(0f, mouseInput.y, 0f));

            //shooting
            if (Input.GetMouseButtonDown(0))
            {
                if(currentAmmo > 0)
                {
                    AudioController.instance.PlayGunShot();
                    Ray ray = viewCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    RaycastHit hit;
                    if(Physics.Raycast(ray, out hit))
                    {
                        //Debug.Log("I'm looking at " + hit.transform.name);
                        Instantiate(bulletImpact, hit.point, transform.rotation);

                        if(hit.transform.tag == "Enemy")
                        {
                            hit.transform.parent.GetComponent<EnemyController>().TakeDamage();
                        }

                    }
                    else
                    {
                        //Debug.Log("I'm looking at nothing");
                    }
                    currentAmmo--;
                    gunAnim.SetTrigger("Shoot");
                    UpdateAmmoUI();
                }
            }

            if(moveInput != Vector2.zero)
            {
                playerAnim.SetBool("isMoving", true);
            }
            else
            {
                playerAnim.SetBool("isMoving", false);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if(currentHealth <= 0)
        {
            GameManager.instance.UnlockCursor();
            healthBox.SetActive(false);
            ammoBox.SetActive(false);
            deadScreen.SetActive(true);
            hasDied = true;
            
        } else
        {
            AudioController.instance.PlayPlayerHurt();
        }

        healthText.text = currentHealth.ToString() + "%";
    }

    public void AddHealth(int healAmount)
    {
        currentHealth += healAmount;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthText.text = currentHealth.ToString() + "%";
    }

    public void UpdateAmmoUI()
    {
        ammoText.text = currentAmmo.ToString();
    }
}
    