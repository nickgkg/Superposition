using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperPosition : MonoBehaviour
{
    // Start is called before the first frame update
    public string key;
    public int pool;
    public bool spawning;
    public bool dying = false;
    public static Dictionary<string, List<SuperPosition>> superPositions = new Dictionary<string, List<SuperPosition>>();
    public int aliveness = 0;

    IEnumerator AnimateCollapse()
    {
        Vector3 startScale = this.transform.localScale;
        int length = 100;
        for (int i = length - 1; i >= 0; i--)
        {
            this.transform.localScale = startScale * ((float)i / length);
            yield return null;
        }
        Spawner.cleanup(this);
    }

    public void CollapseSelf()
    {
        if (dying)
        {
            return;
        }
        dying = true;
        if (this != null && this.gameObject != null)
        {
            this.StartCoroutine(this.AnimateCollapse());
        }
        else
        {
            //Debug.Log("Trying to collapse missing gameobject");
        }
    }

    public void CollapseOthers()
    {
        foreach (SuperPosition p in superPositions[key])
        {
            if (p != this)
            {
                p.CollapseSelf();
            }
        }
        superPositions[key].Clear();
        superPositions[key].Add(this);
    }
    public void register() {
        if (!superPositions.ContainsKey(key))
        {
            superPositions.Add(key, new List<SuperPosition>());
        }
        superPositions[key].Add(this);
    }


    void Update()
    {
        /*if (spawning && !dying && Random.Range(0, 500 * superPositions[key].Count) == 0) {
            foreach (float offset in new float[]{ -2.5f, 2.5f}) {
                GameObject n = GameObject.Instantiate(gameObject, this.transform.position + new Vector3(0f + offset, 15f, 0f), this.transform.rotation);
                //n.transform.localScale = new Vector3(1, 1, 1);
                n.name = gameObject.name;
            }
            //TODO diverge super positions
        }*/
    }
    private void OnCollisionEnter(Collision other)
    {
        if (dying)
        {
            return;
        }

        if (other.rigidbody)
        {
            SuperPosition sup = other.rigidbody.gameObject.GetComponent<SuperPosition>();
            if (sup.dying)
            {
                return;
            }
            if (sup.key == key)
            {
                if (sup.GetHashCode() < this.GetHashCode())
                {
                    CollapseSelf();
                }
                else
                {
                    sup.CollapseSelf();
                }
            }
        }

        if (other.rigidbody)
        {
            Player otherPlayer = other.rigidbody.gameObject.GetComponent<Player>();
            if (otherPlayer)
            {
                Player ourPlayer = gameObject.GetComponent<Player>();
                if (ourPlayer)
                {
                    if (ourPlayer.GetHashCode() < otherPlayer.GetHashCode())
                    {
                        CollapseOthers();
                    }
                }
                else
                {
                    CollapseOthers();
                }
            }
        }
        if (other.rigidbody && other.rigidbody.gameObject.GetComponent<Bullet>())
        {
            CollapseSelf();
        }
    }
}
