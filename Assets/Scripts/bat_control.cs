using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class bat_control : MonoBehaviour {
    //BAT ITSELF
    public GameObject batty;
    public Rigidbody2D bat_rb;
    private int level;

    //MOVEMENT
    private float horizontal_vel = 0.08f;
    private int vertical_push = 12;

    //ECHO AND ECHO PREFABS
    public float echo_radius = 40f;
    int echo_uses;
    public GameObject safe_flag;
    public GameObject hazard_flag;
    public GameObject destination_flag;
    public GameObject echo_sound;

    //CONTROL FLAGS AND BOUNCE
    private bool fly_flag = true;
    private bool echo_flag = true;
    private float fly_bounce = 0.6f;
    private float echo_bounce = 3.1f;

    //CAMERA
    public Camera main_camera;
    public bool cam_x_lock;
    public bool cam_y_lock;

    private Vector3 offset;
    private float cam_switch = 0.05f;
    private float horizontal_cam_switch = 0;
    private float vertical_cam_switch = 0;

    // Start is called before the first frame update
    void Start() {
        batty = GameObject.Find("batty");
        bat_rb = GetComponent<Rigidbody2D>();
        level = 1;
        echo_uses = 12;

        cam_x_lock = false;
        cam_y_lock = true;

        offset = Camera.main.transform.position - batty.transform.position;
    }

    // Update is called once per frame
    void Update() {
        checking_vertical_speed();

        //HORIZONTAL INPUT READING
        if (Input.GetAxis("Horizontal") == 0)
            move_horizontal(0f);
        else if (Input.GetAxis("Horizontal") < 0)
            move_horizontal(-horizontal_vel);
        else if (Input.GetAxis("Horizontal") > 0)
            move_horizontal(horizontal_vel);

        //VERTICAL INPUT READING
        if (Input.GetAxis("Vertical") > 0) {
            fly_up();
        }

        //ECHOLOCATION
        if (Input.GetAxis("Fire1") > 0f)
            use_echo();

        //UPDATING CAMERA
        float final_x = batty.transform.position.x + offset.x + horizontal_cam_switch;
        float final_y = batty.transform.position.y + offset.y;

        //CHECK IF X IS LOCKED
        if (cam_x_lock)
            final_x = -47.5f;

        //CHECK IF Y IS LOCKED
        if (cam_y_lock)
            final_y = 5;

        //Debug.Log("X lock:");
        //Debug.Log(cam_x_lock);
        //Debug.Log("Y lock:");
        //Debug.Log(cam_y_lock);

            Camera.main.transform.position = new Vector3(final_x, final_y, -8);
    }

    //HORIZONTAL MANAGER
    private void move_horizontal(float movement) {
        //MOVE THE BAT
        if (check_walls(movement))
             bat_rb.position = new Vector2 (bat_rb.position.x + movement, bat_rb.position.y);
        
        //SET SPRITE FLIP OF BAT AND CAMERA
        SpriteRenderer bat_renderer = batty.GetComponent<SpriteRenderer>();

        if (movement > 0f) {
            bat_renderer.flipX = false;
            horizontal_cam_switch = cam_switch;
        }
        else if (movement < 0f) {
            bat_renderer.flipX = true;
            horizontal_cam_switch = -cam_switch;
        } else
            horizontal_cam_switch = 0;
    }

    private bool check_walls(float movement_request) {
        //HAVE A RAYCAST IN THE DIRECTION OF THE HORIZONTAL MOVEMENT
        RaycastHit2D horiz_ray;

        if (movement_request < 0f)
            horiz_ray = Physics2D.Raycast(new Vector2(bat_rb.position.x - 0.55f, bat_rb.position.y), Vector2.left);
        else
            horiz_ray = Physics2D.Raycast(new Vector2(bat_rb.position.x + 0.55f, bat_rb.position.y), Vector2.right);

        if (horiz_ray.collider != null) {
            float distance = Mathf.Abs(horiz_ray.point.x - bat_rb.position.x);

            if (distance > 0.56f) {
                return true;
            } else {
                return false;
            }
        }
        
        return true;
    }

    //VERTICAL MANAGER
    private void checking_vertical_speed() {
        //THIS IS MAINLY FOR CAMERA WORK
        if (bat_rb.velocity.y > 1.5f)
            vertical_cam_switch = cam_switch;
        else if (bat_rb.velocity.y < -1.5f)
            vertical_cam_switch = -cam_switch;
        else
            vertical_cam_switch = 0;
    }

    private void fly_up() {
        //PUSH THE BAT UPWARD OR IF ON GROUND (IF HAVEN'T DONE SO RECENTLY)
        //Debug.Log(is_on_ground());
        //Debug.Log(bat_rb.velocity.y);
        //Debug.Log(fly_flag);

        if ((bat_rb.velocity.y <= -7f || is_on_ground()) && fly_flag) {
            fly_flag = false;
            bat_rb.velocity = new Vector2(0, vertical_push);
            StartCoroutine(fly_cooldown());
        } //else if (!(fly_flag))
            //Debug.Log("unable to fly");
    }

    private bool is_on_ground() {
        //RAYCAST BELOW THE BAT
        RaycastHit2D vert_ray = Physics2D.Raycast(new Vector2(bat_rb.position.x, bat_rb.position.y - 0.54f), Vector2.down);

        if (vert_ray != null) {
            if ((bat_rb.position.y - vert_ray.point.y) > 0.8f)
                return false;
            else
                return true;
        }

        return false;
    }

    IEnumerator fly_cooldown() {
        yield return new WaitForSeconds(fly_bounce);
        //Debug.Log("fly is now off cooldown");
        fly_flag = true;
    }

    //ECHO MANAGER
    private bool reduce_echo_uses() {
        //CHECK IF ECHO USE SWAG IS MORE THAN 0
        if (echo_uses > 0) {
            echo_uses--;
            return true;
        } else
            return false;
    }

    private void use_echo() {
        //CHECK IF ECHO FLAG IS UP
        if (echo_flag) {
            //CHECK IF ECHO USE IS VALID
            if (reduce_echo_uses()) {
                //Debug.Log("Using echolocation!");
                //Debug.Log(echo_uses);
                echo_flag = false;
                echolocation();
                StartCoroutine(echo_cooldown());
            }
        }
    }

    private void echolocation() {
        //USING PSEUDO ECHOLOCATION
        RaycastHit2D[] echo_rays = new RaycastHit2D[72];
        Vector2 echo_pos = new Vector2(0,0);
        Vector2 echo_angle = new Vector2(0,0);
        float proportion;

        //PLAYING SOUND EFFECT
        StartCoroutine(play_echo_sound());

        //SET A SERIES OF RAYCASTS AT INTERVALS OF 10 DEGREES AROUND THE BAT IN SOME SET DISTANCE
        for (int i = 0; i < 72; i++) {
            echo_pos = new Vector2(batty.transform.position.x + 0.6f * Mathf.Cos(i*5), batty.transform.position.y + 0.6f * Mathf.Sin(i * 5));
            echo_angle = new Vector2(1f * Mathf.Cos(i * 5), 1f * Mathf.Sin(i * 5));

            echo_rays[i] = Physics2D.Raycast(echo_pos, echo_angle, echo_radius);
            //Debug.Log(echo_rays[i].point);

            if (echo_rays[i].collider != null) {
                proportion = 1f - (echo_rays[i].distance / echo_radius);

                //SEE WHAT THE ECHO HAS HIT (I DON'T CARE IF I'M COMPARING TAGS)
                //Debug.Log("Echo ray has collided with something");

                if (echo_rays[i].collider.gameObject.tag == "safety") {
                    //SIGNIFY SPOT IS SAFE W/ SAFE FLAG PREFAB WHICH FADES
                    //Debug.Log("Placing new safe flags");
                    GameObject new_safe_flag = Instantiate(safe_flag);
                    new_safe_flag.transform.position = echo_rays[i].point;
                    new_safe_flag.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f * (proportion));
                    StartCoroutine(echo_clear(new_safe_flag));
                } else if (echo_rays[i].collider.gameObject.tag == "danger") {
                    //SIGNIFY SPOT IS NOT SAFE W/ HAZARD FLAG PREFAB WHICH FADES
                    //Debug.Log("Placing new danger flags");
                    GameObject new_danger_flag = Instantiate(hazard_flag);
                    new_danger_flag.transform.position = echo_rays[i].point;
                    new_danger_flag.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f * (proportion));
                    StartCoroutine(echo_clear(new_danger_flag));
                } else if (echo_rays[i].collider.gameObject.tag == "destination_block") {
                    //SIGNIFY DESTINATION
                    //Debug.Log("Placing new destination flags");
                    GameObject new_destination = Instantiate(destination_flag);
                    new_destination.transform.position = echo_rays[i].point;
                    new_destination.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f * (proportion));
                    StartCoroutine(echo_clear(new_destination));
                }
            }  else {
                //Debug.Log("Echo ray has not collided with something");
            }
        }
    }

    IEnumerator play_echo_sound() {
        GameObject sound_effect = Instantiate(echo_sound);
        sound_effect.transform.position = batty.transform.position;
        yield return new WaitForSeconds(0.2f);

        Destroy(sound_effect);
    }

    IEnumerator echo_clear(GameObject echo_flag) {
        SpriteRenderer echo_sprite = echo_flag.GetComponent<SpriteRenderer>();

        for (int i = 0; i < 10; i++) {
            yield return new WaitForSeconds(0.1f);
            echo_sprite.color = new Color(echo_sprite.color.r - 0.08f, echo_sprite.color.g - 0.1f, echo_sprite.color.b - 0.1f, echo_sprite.color.a);
        }

        Destroy(echo_flag);
    }

    IEnumerator echo_cooldown() {
        yield return new WaitForSeconds(echo_bounce);
        Debug.Log("echolocation is off cooldown");
        echo_flag = true;
    }

    //COLLISION AND COOLDOWN
    void OnCollisionEnter2D(Collision2D col) {
        //NORMALLY WOULD NOT COMPARE STRINGS, BUT NOT OPTIMIZING HERE -- DP

        //COMPARE TO SEE TAG OF COLLISION FOLLOWS A CERTAIN NAME
        //Debug.Log(col.gameObject.tag == "test_platform");

        //IF REACHING DESTINATION
        if (col.gameObject.tag == "destination_block") {
            //INCREASE A LEVEL
            level++;

            //MOVE CHECKPOINT AND CHANGE CAMERA BASED ON LEVEL AND RESET ECHO CHARGES
            if (level == 2) {
                cam_x_lock = true;
                cam_y_lock = false;
                batty.transform.position = new Vector2(-47.5f, 4.0f);
                echo_uses = 12;
            } else if (level > 2) {
                //LOAD GAME FINISHED SCREEN
                SceneManager.LoadScene("end_screen", LoadSceneMode.Single);
            }
            
        } else if (col.gameObject.tag == "danger") {
            //MOVE BACK TO CHECKPOINT AND RESET ECHO CHARGES
            Debug.Log("rip bat");

            if (level == 1)
                batty.transform.position = new Vector2(18.5f, 6.0f);
            else if (level == 2)
                batty.transform.position = new Vector2(-47.5f, 4.0f);

            echo_uses = 12;
        }
    }

    /*
    void OnDrawGizmos() {
        
        RaycastHit2D echo_hit;
        GameObject given_object;
        Vector2 echo_pos;
        Vector2 echo_angle;
        Vector2 echo_distance;
        float proportion = 0f;

        for (int i = 0; i < 360; i += 5) {
            echo_pos = new Vector2(0,3);
            echo_angle = new Vector2(1 * Mathf.Cos(i), 1 * Mathf.Sin(i));

            echo_hit = Physics2D.Raycast(echo_pos, echo_angle, echo_radius);

            echo_distance = echo_hit.point - echo_pos;

            proportion = 1f - (echo_distance.magnitude/ echo_radius);

            if (echo_hit.collider != null) {
                given_object = echo_hit.collider.gameObject;

                if (given_object.tag == "test_platform") {
                    Gizmos.color = new Color(0, 1f, 1f, 1f * (proportion));
                } else if (given_object.tag == "hazard") {
                    Gizmos.color = new Color(1f, 0, 1f, 1f * (proportion));
                } else {
                    Gizmos.color = new Color(0f, 0f, 0f, 0f);
                }
            }
            Gizmos.DrawCube(echo_hit.point, new Vector3(0.4f, 0.4f, 0.4f));
        }
    }
    */
}
