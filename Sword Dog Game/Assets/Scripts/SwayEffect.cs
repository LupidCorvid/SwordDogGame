using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class SwayEffect : MonoBehaviour
{
    public Sprite texture;
    Renderer rend;

    Mesh originalMesh;
    MeshFilter meshFilter;
    Mesh.MeshDataArray originalSnapshot;

    private float swayPosition = 1;
    private float swayVelocity = 0;

    public float tension = 1;
    public float dampening = 1;

    public float windStrength = 1;
    public float windSpeed = 1;
    //Lower wind volatility means objects near eachother have similar swaying motion
    public float windVolatility = 0.2f;

    //Maximum distance for deformation on the mesh
    public float limit = 1;

    public bool rotate = false;

    public Dictionary<Rigidbody2D, Vector2> objectsWithVelocity = new Dictionary<Rigidbody2D, Vector2>();

    public SoundPlayer soundPlayer;

    //Used for culling
    public static Transform player;

    //Used for sound capping
    public static int windSounds = 0;
    public static int windSoundCap = 400;

    // Start is called before the first frame update
    void Start()
    {
        player ??= GameObject.FindGameObjectWithTag("Player").transform;
        soundPlayer = GetComponent<SoundPlayer>();
        GetComponent<MeshRenderer>().material.mainTexture = texture.texture;
        rend = GetComponent<Renderer>();
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh;
        originalSnapshot = Mesh.AcquireReadOnlyMeshData(meshFilter.sharedMesh);
        Mesh.MeshDataArray tempData = Mesh.AcquireReadOnlyMeshData(meshFilter.sharedMesh);
        meshFilter.sharedMesh = new Mesh();
        Mesh.ApplyAndDisposeWritableMeshData(tempData, meshFilter.sharedMesh);
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.RecalculateNormals();
    }

    public void sway(float intensity)
    {
        NativeArray<Vector3> vertices = new NativeArray<Vector3>(originalSnapshot[0].vertexCount, Allocator.Temp);
        originalSnapshot[0].GetVertices(vertices);
        Vector3[] newVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float height = (originalMesh.vertices[i].y - originalMesh.bounds.min.y) / originalMesh.bounds.size.y;
            newVertices[i] = new Vector3((Mathf.Lerp(0, originalMesh.bounds.size.y, height) * intensity) + vertices[i].x, vertices[i].y, vertices[i].z);
        }
        meshFilter.sharedMesh.SetVertices(newVertices);
    }
    public void swayRotate(float intensity)
    {
        NativeArray<Vector3> vertices = new NativeArray<Vector3>(originalSnapshot[0].vertexCount, Allocator.Temp);
        originalSnapshot[0].GetVertices(vertices);
        Vector3[] newVertices = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            float height = (originalMesh.vertices[i].y - originalMesh.bounds.min.y) / originalMesh.bounds.size.y;
            newVertices[i] = new Vector3((Mathf.Lerp(0, originalMesh.bounds.size.y, height) * intensity) + vertices[i].x, vertices[i].y, vertices[i].z);
            float angle = Mathf.Atan2(newVertices[i].y, newVertices[i].x);
            newVertices[i].y = Mathf.Sin(angle);
        }

        meshFilter.sharedMesh.SetVertices(newVertices);
    }
    public void OnDestroy()
    {
        try
        {
            meshFilter.sharedMesh = originalMesh;
            originalSnapshot.Dispose();
        }
        catch
        {
            //Only here to prevent errors from stoping debug    
        }
    }

    public void OnApplicationQuit()
    {
        meshFilter.sharedMesh = originalMesh;
        meshFilter.sharedMesh.SetVertices(originalMesh.vertices);
        try
        {
            //Mesh.ApplyAndDisposeWritableMeshData(originalSnapshot, meshFilter.sharedMesh);
        }
        catch
        {
            //Only here to prevent having to stop debugging every time this recompiles when running
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (((int)Mathf.Pow(2, collision.gameObject.layer) & LayerMask.GetMask("TerrainFX", "Utility")) == 0)
        {
            float distanceModifier = 1;
            distanceModifier = Mathf.Abs((transform.position.x - collision.transform.position.x)/collision.bounds.extents.x);
            distanceModifier = Mathf.Clamp(distanceModifier, 0, 1);
            distanceModifier = 1 - distanceModifier;
            float physicsVelocity = ((collision.attachedRigidbody.velocity.x) * Time.deltaTime) * distanceModifier;
            
            if(collision.attachedRigidbody != null && objectsWithVelocity.ContainsKey(collision.attachedRigidbody))
            {
                //Affects additional force applied to grass behind the launching object
                const float PUSH_BACK_STRENGTH = 1.0f / 130;
                //Affects all grass around the launching or landing object
                const float PUSH_AWAY_STRENGTH = 1.0f / 115;

                distanceModifier = (transform.position.x - collision.transform.position.x)/collision.bounds.extents.x;
                distanceModifier = Mathf.Clamp(distanceModifier, -1, 1);
                distanceModifier += distanceModifier > 0 ? -1 : 1;
                distanceModifier *= -1;
                physicsVelocity += Mathf.Abs(collision.attachedRigidbody.velocity.y - objectsWithVelocity[collision.attachedRigidbody].y) * PUSH_AWAY_STRENGTH * distanceModifier;

                //For pushing grass behind something being launched
                if(collision.attachedRigidbody.velocity.y - objectsWithVelocity[collision.attachedRigidbody].y > .25f)
                {
                    if(transform.position.x - collision.transform.position.x < 0 && collision.attachedRigidbody.velocity.x > 0
                        || transform.position.x - collision.transform.position.x > 0 && collision.attachedRigidbody.velocity.x < 0)
                    {
                        physicsVelocity += Mathf.Abs(collision.attachedRigidbody.velocity.y - objectsWithVelocity[collision.attachedRigidbody].y) * PUSH_BACK_STRENGTH * distanceModifier 
                                        * Mathf.Abs(collision.attachedRigidbody.velocity.x);

                    }
                }
                
                objectsWithVelocity[collision.attachedRigidbody] = collision.attachedRigidbody.velocity;
            }
            swayVelocity += physicsVelocity;
            if(Mathf.Abs(physicsVelocity) > 0.01f)
            {
                soundPlayer.PlaySound("Ambience.WindyForest.RustleFX", Mathf.Abs(physicsVelocity / Time.fixedDeltaTime * 0.075f * 13f / 3 * .5f));
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.GetMask("TerrainFX") && collision.attachedRigidbody != null && !objectsWithVelocity.ContainsKey(collision.attachedRigidbody)) 
        {
            objectsWithVelocity.Add(collision.attachedRigidbody, collision.attachedRigidbody.velocity);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.GetMask("TerrainFX") && collision.attachedRigidbody != null && objectsWithVelocity.ContainsKey(collision.attachedRigidbody))
        {
            objectsWithVelocity.Remove(collision.attachedRigidbody);
        }
    }

    private void FixedUpdate()
    {
        //Culling
        if ((player.position - transform.position).x > 25 || (player.position - transform.position).y > 15)
            return;

        //Wind direction makes it so that wind rolls in the same direction as things are bending
        int windDirection = (windStrength > 0 ? -1 : 1);
        float lastVelocity = swayVelocity;
        //Changed Time.deltaTime to Time.fixedTime to reflect that this is in FixedUpdate
        float windEffect = Mathf.PerlinNoise(((Time.time * windSpeed * windDirection) + (transform.position.x)) * windVolatility, 0) * Time.fixedDeltaTime * windStrength;
        swayVelocity += windEffect;
        swayVelocity += tension * (-swayPosition) - swayVelocity * dampening;
        swayPosition += swayVelocity;

        if(Mathf.Abs(swayPosition) > limit)
        {
            if (swayPosition > 0)
                swayPosition = limit;
            else
                swayPosition = -limit;
            swayVelocity /= limit;
        }
        if (rend.isVisible)
        {
            if (rotate)
                swayRotate(swayPosition);
            else
                sway(swayPosition);
        }

        if(windEffect/Time.fixedDeltaTime  * 5/windStrength > 3 && windSounds < windSoundCap)
        {
            AudioClip rustleFX = AudioManager.instance?.Find("Ambience.WindyForest.RustleFX");
            if (rustleFX != null)
            {
                soundPlayer.PlaySound(rustleFX, windEffect / Time.fixedDeltaTime * 0.04f);
                windSounds++;
                Invoke("EndWindSound", rustleFX.length);
            }
        }
    }
    
    private void EndWindSound()
    {
        windSounds--;
    } 
}
