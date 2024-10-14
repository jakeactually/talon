using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Talon : MonoBehaviour
{
    public GameObject knifePrefab;
    public Collider plane;
    Animator animator;
    List<Knife> knives = new List<Knife>();
    Vector3 mouse;
    int qFrames = 0;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("q"))
        {
            qFrames = 200;
            mouse = transform.position + Util.ToCartesian(transform.rotation.eulerAngles.y, 5);
            animator.SetBool("IsKilling", true);
        }

        if (qFrames == 0)
        {
            animator.SetBool("IsKilling", false);
        }
        else
        {
            qFrames--;
        }

        if (Input.GetKeyDown("w"))
        {
            TalonW();
        }

        if (Input.GetKeyUp("e"))
        {
            animator.SetBool("IsJumping", false);
        }

        if (Input.GetKeyDown("e"))
        {
            animator.SetBool("IsJumping", true);
        }

        if (Input.GetKeyDown("r"))
        {
            TalonR();
        }

        Move();
        Knives();
    }
    
    void Move()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (plane.Raycast(ray, out hit, 100.0f))
            {
                mouse = hit.point;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, mouse, Time.deltaTime * 10);

        if (Vector3.Distance(transform.position, mouse) > 1 && qFrames == 0)
        {
            animator.SetBool("IsRunning", true);
            transform.LookAt(mouse);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }
    }

    void Knives()
    {
        knives.ForEach(k => {
            k.Move();

            if (!k.valid)
            {
                Destroy(k.knife);
            }
        });

        knives = knives.Where(k => k.valid).ToList();
    }

    void TalonW()
    {
        knives.Add(new Knife(this));
        knives.Add(new Knife(this, 30));
        knives.Add(new Knife(this, -30));
    }

    void TalonR()
    {
        for (int i = 0; i < 10; i++)
        {
            knives.Add(new Knife(this, i * 36));
        }
    }
}

static class Util
{
    public static Vector3 ToCartesian(float angle, int magnitude)
    {
        return new Vector3(
            (float)Math.Sin(angle / 180 * Math.PI) * magnitude,
            0,
            (float)Math.Cos(angle / 180 * Math.PI) * magnitude
        );
    }
}

class Knife
{
    public bool valid = true;
    public GameObject knife;
    Talon parent;
    float time = 0;
    Vector3 startPosition;
    Vector3 endPosition;

    public Knife(Talon parent)
    {
        Init(parent, 0);
    }

    public Knife(Talon parent, float angleOffset)
    {
        Init(parent, angleOffset);
    }

    void Init(Talon parent, float angleOffset)
    {
        this.parent = parent;
        startPosition = parent.transform.position + new Vector3(0, 1.5f, 0);
        endPosition = startPosition + Util.ToCartesian(parent.transform.rotation.eulerAngles.y + angleOffset, 7);
        knife = UnityEngine.Object.Instantiate(parent.knifePrefab, startPosition, Quaternion.identity);
    }

    public void Move()
    {
        if (time < 0.5)
        {
            Forward();
        }
        else if (time > 1.5 && time < 2)
        {
            Back();
        }
        else if (time > 2)
        {
            valid = false;
        }

        time += Time.deltaTime;
    }

    public Vector3 ParentPosition()
    {
        Vector3 parentPosition = parent.transform.position;
        return new Vector3(parentPosition.x, 1.5f, parentPosition.z);
    }

    public void Forward()
    {
        float offset = time * 2;
        knife.transform.localPosition = Vector3.Lerp(startPosition, endPosition, offset);
    }


    public void Back()
    {
        float offset = (float)(time - 1.5) * 2;
        knife.transform.localPosition = Vector3.Lerp(endPosition, ParentPosition(), offset);
    }
}
