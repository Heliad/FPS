using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [SerializeField] float speed = 3.5F;
    [SerializeField] float angularSpeed = 120;
    [SerializeField] float fov = 150F;
    [SerializeField] float maxDistanseView = 25F;
    [SerializeField] float maxCommunicateDistanse = 40F;
    [SerializeField] float maxFireRange = 10F;
    [SerializeField] float health = 100.0F;
    [SerializeField] float fireRate = 0.5F;
    [SerializeField] float weaponDamage = 5F;
    [SerializeField] float weaponRecoil = 5F;
    [SerializeField] float reloadTime = 1.833F;
    [SerializeField] int ammoClip = 50;
    [SerializeField] int clipAmmount = 5;
    [SerializeField] GameObject muzzleEndGameObject;
    [SerializeField] GameObject shellPortGameObject;
    [SerializeField] GameObject destinationTarget;

    [Header("Decals")]
    [SerializeField] GameObject decal0;

    [Header("IK Controller")]
    [SerializeField] float weight = 1;
    [SerializeField] float bodyWeight = 1;
    [SerializeField] float headWeight = 1;
    [SerializeField] Transform weaponDirection;
    [SerializeField] Transform bodyDirection;

    [Header("Patrol Points")]
    [SerializeField] Transform beginPoint;
    [SerializeField] Transform endPoint;

    private NavMeshAgent agent;
    private GameObject player;
    private GameObject target;

    Animator animator;

    float communicateTime;
    float searchSpeed = 3.5F;
    float lastSeenTargetDirectionSearchTime = 5F;
    float lastSeenTargetDirectionSearchTimeTemp = 5F;
    float hasPathTime = 0.5F;
    float timeToFire = 0;
    float distanseToTarget = 0;
    float weaponOffset = 0;
    float fovTemp;

    bool isLostEnemy = false;
    bool isSearching = false;
    bool isFiring = false;
    bool isHiding = false;
    bool isHideDestinationSet = false;
    bool isHalfRotation = false;
    bool isRotating = false;
    bool isReloading = false;
    bool direction = true;
    bool hasLookedAround = false;

    int layerMask;
    int curAmmoClip;

    string enemyTag = "Player";

    public delegate void state();
    state CurrentState;
    state PreviousState;

    List<GameObject> allys = new List<GameObject>();

    Vector3 forward;
    Vector3 newPoint;
    Vector3 searchTarget;
    Vector3 lastSeenTarget;
    Vector3 lastSeenTargetDirection;

    Ray fireRay;
    Ray trgRay;
    Ray escapeRay;

    RaycastHit trgHit;
    RaycastHit fireHit;
    RaycastHit escapeHit;

    ParticleSystem muzzleEnd;
    ParticleSystem shellPort;

    NavMeshPath currentPath;

    LineRenderer ln;

    List<SearchedPoints> points = new List<SearchedPoints>();

    public class SearchedPoints
    {
        public Vector3 searchedArea;
        public float validTime;

        public SearchedPoints(Vector3 s, int v)
        {
            searchedArea = s;
            validTime = v;
        }

        public void timeDecrease()
        {
            validTime -= Time.deltaTime;
        }
    }

    void Start ()
    {
        if (LevelParameters.difficulty == 0)
        {
            weaponDamage = 2;
            fov = 120;
            maxDistanseView = 20;
            maxFireRange = 20;
            weaponRecoil = 6.5F;
            health = 40;
            maxCommunicateDistanse = 40;
            communicateTime = 4;
        }
        else if (LevelParameters.difficulty == 1)
        {
            weaponDamage = 3;
            fov = 140;
            maxDistanseView = 25;
            maxFireRange = 20;
            weaponRecoil = 5;
            health = 50;
            maxCommunicateDistanse = 40;
            communicateTime = 3;
        }
        else if (LevelParameters.difficulty == 2)
        {
            weaponDamage = 3.5F;
            fov = 170;
            maxDistanseView = 35;
            maxFireRange = 30;
            weaponRecoil = 3.5F;
            health = 60;
            maxCommunicateDistanse = 60;
            communicateTime = 2;
        }

        GameObject[] temp = GameObject.FindGameObjectsWithTag(gameObject.tag);

        for (int i = 0; i < temp.Length; i++)
        {
            if (temp[i].GetComponent<AIController>() != null)
            {
                allys.Add(temp[i]);
            }
        }

        timeToFire = fireRate;

        curAmmoClip = ammoClip;

        layerMask = 1 << 9;
        layerMask = ~layerMask;

        fovTemp = fov;

        ln = gameObject.AddComponent<LineRenderer>();
        ln.startWidth = 0.1F;
        ln.endWidth = 0.1F;

        currentPath = new NavMeshPath();

        animator = GetComponentInChildren<Animator>();

        agent = GetComponent<NavMeshAgent>();

        muzzleEnd = muzzleEndGameObject.GetComponent<ParticleSystem>();
        shellPort = shellPortGameObject.GetComponent<ParticleSystem>();

        player = GameObject.FindGameObjectWithTag(enemyTag);

        fireRay = new Ray();
        trgRay = new Ray();
        escapeRay = new Ray();

        if (beginPoint != null || endPoint != null)
        {
            CurrentState = Patrol;
        }
        else
        {
            CurrentState = Idle;
        }

        agent.speed = speed;

        StartCoroutine(MapArea());
        StartCoroutine(FindAllys());
    }

    void ReceiveDamage(float dmg)
    {
        health -= dmg;
        if (health <= 0)
            CurrentState = Dead;
        if (target == null)
            target = player;
    }

    void TargetLocation(GameObject t)
    {
        if (target == null)
        {
            target = t;
        }
    }

    void Dead()
    {
        StopAllCoroutines();
        agent.velocity = Vector3.zero;
        agent.enabled = false;
        animator.SetBool("is_dead", true);
        animator.enabled = false;
    }

    void Idle()
    {
        Scan();
    }

    void Patrol()
    {
        if ((transform.position - endPoint.position).magnitude < 0.6F && !direction)
        {
            if (!hasLookedAround)
            {
                CurrentState = LookAround;
                PreviousState = Patrol;
                hasLookedAround = true;
                return;
            }

            currentPath = new NavMeshPath();
            agent.CalculatePath(beginPoint.position, currentPath);
            direction = true;
            hasLookedAround = false;
        }
        else if ((transform.position - beginPoint.position).magnitude < 0.6F && direction)
        {
            if (!hasLookedAround)
            {
                CurrentState = LookAround;
                PreviousState = Patrol;
                hasLookedAround = true;
                return;
            }

            currentPath = new NavMeshPath();
            agent.CalculatePath(endPoint.position, currentPath);
            direction = false;
            hasLookedAround = false;
        }

        agent.SetPath(currentPath);

        Scan();
    }

    void FollowTarget()
    {
        agent.stoppingDistance = maxFireRange;
        agent.speed = speed;

        agent.SetDestination(target.transform.position);
        trgRay.origin = transform.position + transform.up * 1.5f + transform.forward * 0.3F;
        trgRay.direction = (target.transform.position - (transform.position + transform.up * 1.5f + transform.forward * 0.3F)).normalized;

        if (Physics.Raycast(trgRay, out trgHit, maxDistanseView))
        {
            if (!trgHit.collider.gameObject.CompareTag(enemyTag))
            {
                CurrentState = Search;
                lastSeenTargetDirectionSearchTimeTemp = lastSeenTargetDirectionSearchTime;
                target = null;
            }
        }
        else
        {
            CurrentState = Search;
            lastSeenTargetDirectionSearchTimeTemp = lastSeenTargetDirectionSearchTime;
            target = null;
        }

        if (Vector3.Angle(transform.forward, trgRay.direction) >= fov)
        {
            CurrentState = Search;
            lastSeenTargetDirectionSearchTimeTemp = lastSeenTargetDirectionSearchTime;
            target = null;
        }

        if (target != null)
        {
            if ((target.transform.position - transform.position).magnitude <= maxFireRange)
            {
                try
                {
                    agent.path = null;
                }
                catch
                {

                }
                CurrentState = Attack;
            }
        }
    }

    void  RotateTorwards()
    {
        if (Vector3.Angle(transform.forward, (target.transform.position - transform.position).normalized) >= fov / 2)
        {
            isRotating = true;
        }
        else if (Vector3.Angle(transform.forward, (target.transform.position - transform.position).normalized) < 30)
        {
            isRotating = false;
        }

        if (isRotating)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.transform.position - (transform.position + transform.up * 1.5f + transform.forward * 0.3F), transform.up), 0.1F);
        }
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(reloadTime);
        curAmmoClip = ammoClip;
        clipAmmount--;
        isReloading = false;
    }

    void Attack()
    {
        if (weaponOffset > 0.01)
        {
            weaponOffset = Mathf.Lerp(weaponOffset, 0, 0.1F);
        }

        timeToFire += Time.deltaTime;

        fireRay.direction = (target.transform.position - muzzleEndGameObject.transform.position).normalized + new Vector3((UnityEngine.Random.value - 0.5F) * 2, (UnityEngine.Random.value - 0.5F) * 2, 0) * Mathf.Abs(weaponOffset);
        fireRay.origin = muzzleEndGameObject.transform.position;

        //ln.SetPosition(0, fireRay.origin);
        //ln.SetPosition(1, fireRay.GetPoint(maxFireRange));

        if (timeToFire >= fireRate && weaponOffset < weaponRecoil / (target.transform.position - transform.position).magnitude / 5 && curAmmoClip > 0)
        {
            timeToFire = 0;
            isFiring = true;
            weaponOffset += weaponRecoil / 35;
            curAmmoClip--;
            muzzleEnd.Play();

            if (Physics.Raycast(fireRay, out fireHit, maxFireRange, layerMask))
            {
                try
                {
                    GameObject gm = fireHit.collider.gameObject;
                    if (gm.tag == enemyTag)
                    {
                        Health h = fireHit.collider.GetComponent<Health>();
                        h.SendMessage("ReceiveDamage", weaponDamage);
                    }
                    else
                    {
                        GameObject d = Instantiate(decal0);
                        d.transform.position = fireHit.point;
                        d.transform.forward = fireHit.normal;
                    }
                }
                catch
                {

                }
            }
        }
        else if (curAmmoClip == 0 && !isReloading && clipAmmount > 0)
        {
            isReloading = true;
            StartCoroutine(Reload());
        }

        //if (isReloading) FindHidingPoint();

        trgRay.origin = transform.position + transform.up * 1.5f + transform.forward * 0.3F;
        trgRay.direction = (target.transform.position - (transform.position + transform.up * 1.5f + transform.forward * 0.3F)).normalized;

        if (Physics.Raycast(trgRay, out trgHit, maxDistanseView, layerMask))
        {
            if (!trgHit.collider.gameObject.CompareTag(enemyTag))
            {
                isLostEnemy = true;
            }
        }

        if ((target.transform.position - transform.position).magnitude > maxDistanseView)
        {
            isLostEnemy = true;
        }

        if (isLostEnemy)
        {
            CurrentState = Search;
            lastSeenTargetDirectionSearchTimeTemp = lastSeenTargetDirectionSearchTime;
            target = null;
        }
    }

    void Search()
    {
        lastSeenTargetDirectionSearchTimeTemp -= Time.deltaTime;
        int pointCount = 0;

        if (lastSeenTargetDirectionSearchTimeTemp <= 0 || (lastSeenTarget - transform.position).magnitude < 0.5F)
        {
            lastSeenTarget = Vector3.zero;
        }

        if (agent.velocity.magnitude < 0.1F)
        {
            while (pointCount <= 10)
            {
                bool is_valid = true;
                if (lastSeenTarget == Vector3.zero)
                {
                    newPoint = FindPoint();
                    foreach (var item in points)
                    {
                        if ((item.searchedArea - newPoint).magnitude < maxDistanseView)
                        {
                            is_valid = false;
                        }
                    }
                }
                else
                    newPoint = lastSeenTarget;
                pointCount++;

                if (is_valid)
                {
                    searchTarget = newPoint;
                    CurrentState = FollowSearchTarget;
                    agent.SetDestination(searchTarget);
                    break;
                }
            }
        }

        Scan();
    }

    void FindHidingPoint()
    {
        if (!isHideDestinationSet)
        {
            escapeRay.origin = FindPoint(true);
            currentPath = new NavMeshPath();
            agent.CalculatePath(escapeRay.origin, currentPath);
        }

        if (currentPath != null)
        {
            if (currentPath.status == NavMeshPathStatus.PathPartial || currentPath.status == NavMeshPathStatus.PathInvalid)
            {
                currentPath = null;
            }
            else
            {
                escapeRay.origin = currentPath.corners[currentPath.corners.Length - 1] + new Vector3(0, agent.height / 2);
            }
        }

        if (currentPath != null)
        {
            escapeRay.direction = (player.transform.position - escapeRay.origin).normalized;
            if (Physics.Raycast(escapeRay, out escapeHit, maxDistanseView, layerMask))
            {
                if (!escapeHit.collider.gameObject.CompareTag(enemyTag) && !isHideDestinationSet)
                {
                    float d = PathDistanse(currentPath);

                    if (d <= (transform.position - escapeRay.origin).magnitude * 1.4F && !IsPathCloseToTarget(currentPath.corners, player.transform.position))
                    {
                        CurrentState = Hide;
                        isHideDestinationSet = true;
                    }
                }
                else if (escapeHit.collider.gameObject.CompareTag(enemyTag))
                {
                    isHiding = false;
                    isHideDestinationSet = false;
                    currentPath = null;
                }
            }
        }
    }

    void Hide()
    {
        agent.stoppingDistance = 0;
        agent.angularSpeed = 120;

        isFiring = false;

        agent.SetPath(currentPath);
        //GameObject gm = Instantiate(destinationTarget);
        //gm.transform.position = currentPath.corners[currentPath.corners.Length - 1];
        //ln.SetPosition(0, transform.position);
        //ln.SetPosition(1, gm.transform.position);

        if ((currentPath.corners[currentPath.corners.Length - 1] - transform.position).magnitude < 0.1F)
        {
            isHiding = true;
            escapeRay.origin = transform.position;
            escapeRay.direction = (player.transform.position - transform.position).normalized;

            if (Physics.Raycast(escapeRay, out escapeHit, maxDistanseView, layerMask))
            {
                if (escapeHit.collider.gameObject.CompareTag(enemyTag))
                {
                    target = escapeHit.collider.gameObject;
                    isHiding = false;
                    isHideDestinationSet = false;
                    CurrentState = Attack;
                }
            }
        }
    }

    bool IsPathCloseToTarget(Vector3[] p, Vector3 t, float m = 3.5F)
    {
        foreach (Vector3 i in p)
        {
            if ((i - t).magnitude < m)
            {
                return true;
            }
        }
        return false;
    }

    Vector3 FindPoint(bool escape = false)
    {
        Vector3 p;
        if (!escape)
        {
            if (lastSeenTargetDirectionSearchTimeTemp > 0)
                p = transform.position + lastSeenTargetDirection * UnityEngine.Random.Range(5, maxDistanseView) + new Vector3(((UnityEngine.Random.value - 0.5F) * 2), 0, 0);
            else
                p = transform.position + (new Vector3((UnityEngine.Random.value - 0.5F) * 2, UnityEngine.Random.value - 0.5F, (UnityEngine.Random.value - 0.5F) * 2) * UnityEngine.Random.Range(5, maxDistanseView));
        }
        else
        {
            float d;
            if (distanseToTarget > maxDistanseView)
            {
                d = maxDistanseView;
            }
            else
            {
                d = distanseToTarget;
            }
            p = transform.position + (new Vector3((UnityEngine.Random.value - 0.5F) * 2, (UnityEngine.Random.value - 0.5F) / 2, (UnityEngine.Random.value - 0.5F) * 2) * UnityEngine.Random.Range(0.5F, d));
        }
        return p;
    }

    float PathDistanse(NavMeshPath path)
    {
        float distanse = 0;
        for (int i = 1; i < path.corners.Length; i++)
        {
            distanse += (path.corners[i] - path.corners[i - 1]).magnitude;
        }
        return distanse;
    }

    private IEnumerator MapArea()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5F);

            Vector3 p = transform.position + transform.forward * maxDistanseView / 2;
            bool is_valid = true;
            foreach (var item in points)
            {
                if ((item.searchedArea - p).magnitude < maxDistanseView)
                {
                    is_valid = false;
                }
            }
            if (is_valid)
                points.Add(new SearchedPoints(transform.position + transform.forward * maxDistanseView / 2, 30));
        }
    }

    void FollowSearchTarget()
    {
        hasPathTime -= Time.deltaTime;

        lastSeenTargetDirectionSearchTimeTemp -= Time.deltaTime;
        isSearching = true;
        agent.speed = searchSpeed;
        if ((searchTarget - transform.position).magnitude < 1)
        {
            CurrentState = LookAround;
            PreviousState = Search;
            isSearching = false;
            return;
        }
        if ((agent.hasPath && agent.remainingDistance != Mathf.Infinity && agent.remainingDistance > (searchTarget - transform.position).magnitude * 2) || (agent.velocity.magnitude < 0.1F && hasPathTime <= 0))
        {
            CurrentState = Search;
            hasPathTime = 0.5F;
            agent.SetDestination(transform.position);
        }
        Scan();
    }

    void LookAround()
    {
        if (forward == Vector3.zero)
            forward = transform.forward;
        transform.forward = transform.forward + transform.right * 0.05F;
        if (Vector3.Angle(forward, transform.forward) > 170)
            isHalfRotation = true;
        if (Vector3.Angle(forward, transform.forward) < 10 && isHalfRotation)
        {
            if (PreviousState == Search)
            {
                CurrentState = Search;
            }
            else if (PreviousState == Patrol)
            {
                CurrentState = Patrol;
            }
            forward = Vector3.zero;
            isHalfRotation = false;
        }
        Scan();
    }

    void Scan()
    {
        float distanse = (player.transform.position - transform.position).magnitude;

        if (Vector3.Angle(transform.forward, player.transform.position - transform.position) <= fov / 2 && distanse <= maxDistanseView)
        {
            trgRay.origin = transform.position + transform.up * 1.5f + transform.forward * 0.3F;
            trgRay.direction = (player.transform.position - (transform.position + transform.up * 1.5f + transform.forward * 0.3F)).normalized;
            if (Physics.Raycast(trgRay, out trgHit, maxDistanseView, layerMask))
            {
                if (trgHit.collider.gameObject.tag == enemyTag)
                {
                    if (distanse <= maxFireRange)
                    {
                        CurrentState = Attack;
                    }
                    else
                    {
                        CurrentState = FollowTarget;
                    }
                    lastSeenTargetDirectionSearchTimeTemp = lastSeenTargetDirectionSearchTime;
                    target = player;
                    isSearching = false;
                }
            }
        }
    }

    List<SearchedPoints> DeleteInvalidPoints(List<SearchedPoints> p)
    {
        var p_ = new List<SearchedPoints>(p);
        foreach (var item in p)
        {
            item.timeDecrease();
            if (item.validTime < 0)
                p_.Remove(item);
        }
        return new List<SearchedPoints>(p_);
    }

    public string CurrentStateLog()
    {
        if (CurrentState == Search)
        {
            return "Search";
        }
        else if (CurrentState == Idle)
        {
            return "Idle";
        }
        else if (CurrentState == Patrol)
        {
            return "Patrol";
        }
        else if (CurrentState == FollowTarget)
        {
            return "FollowTarget";
        }
        else if (CurrentState == FollowSearchTarget)
        {
            return "FollowSearchTarget";
        }
        else if (CurrentState == LookAround)
        {
            return "LookAround";
        }
        else if (CurrentState == Dead)
        {
            return "Dead";
        }
        else if (CurrentState == Attack)
        {
            return "Attack";
        }
        else if (CurrentState == Hide)
        {
            return "Hide";
        }
        else
        {
            return "Unknown";
        }
    }

    void HealthCheck()
    {
        if (health <= 10 && CurrentState != Dead && CurrentState != Hide && (curAmmoClip > 0 || clipAmmount > 0))
        {
            FindHidingPoint();
        }
    }

    void AnimationVariableSet()
    {
        animator.SetFloat("run_speed", agent.velocity.magnitude / agent.speed);
        animator.SetBool("reload", isReloading);

        if (CurrentState == Attack)
        {
            animator.SetBool("is_firing", true);
        }
        else
            animator.SetBool("is_firing", false);

        if (CurrentState == Dead)
        {
            animator.SetBool("is_dead", true);
        }
    }

    IEnumerator FindAllys()
    {
        while (true)
        {
            yield return new WaitForSeconds(communicateTime);

            if (target != null)
            {
                foreach (GameObject gm in allys)
                {
                    if ((gm.transform.position - transform.position).magnitude <= maxCommunicateDistanse)
                    {
                        gm.GetComponent<AIController>().BroadcastMessage("TargetLocation", target);
                    }
                }
            }
        }
    }

    void ManageSecondaryTasks()
    {
        if (CurrentState != Dead)
        {
            HealthCheck();

            points = DeleteInvalidPoints(points);

            AnimationVariableSet();

            if (!isReloading) StopCoroutine("Reload");

            if (target != null)
            {
                distanseToTarget = (target.transform.position - transform.position).magnitude;

                if (CurrentState == Search || CurrentState == FollowSearchTarget || CurrentState == Idle || CurrentState == Patrol)
                {
                    CurrentState = FollowTarget;
                }
            }
            else
            {
                distanseToTarget = 0;
            }

            if (CurrentState == Hide)
            {
                fov = 360;
            }
            else
            {
                fov = fovTemp;
            }
            if (CurrentState == Search)
            {
                agent.stoppingDistance = 0;
            }
            if (CurrentState == Attack)
            {
                isLostEnemy = false;
                weight = 1;
                bodyWeight = 1;
                agent.angularSpeed = 0;
                agent.SetPath(new NavMeshPath());
                RotateTorwards();
            }
            else
            {
                isFiring = false;
                agent.angularSpeed = angularSpeed;
                weight = 0;
                bodyWeight = 0;
            }

            if (curAmmoClip == 0 && clipAmmount == 0) FindHidingPoint();

            if (target != null)
            {
                lastSeenTarget = target.transform.position;
                lastSeenTargetDirection = target.GetComponent<CharacterController>().velocity.normalized;
            }
            //Debug.Log(CurrentStateLog());

            //Debug.Log(agent.velocity);

            //Debug.Log(distanseToTarget);

        }
    }

    void FixedUpdate()
    {
        ManageSecondaryTasks();
        CurrentState();

        //Debug.Log(gameObject.name + ": " + CurrentStateLog());
    }

    void OnAnimatorIK()
    {
        animator.SetLookAtWeight(weight, bodyWeight, headWeight);
        if (target != null)
        {
            Vector3 v = target.transform.position - new Vector3(bodyDirection.position.x, weaponDirection.position.y, bodyDirection.position.z);
            Vector3 p = weaponDirection.forward * v.magnitude;
            animator.SetLookAtPosition(target.transform.position - (p - v));
        }
    }
}
