using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class piping : MonoBehaviour
{

    public Material material;
    public int size;

    public float segmentLength = 1;
    public float pipeRadius = 1;
    public int polyCountPerSeg = 8;
    public int cornerSegCount = 4;

    public Vector3 cornerRot;
    public Vector3 cornerPos;

    public Vector3 pipeRot;
    public Vector3 pipePos;

    // Start is called before the first frame update
    Mesh pipeMesh;
    Mesh cornerMesh;
    HashSet<Vector3> pipeLocs;
    [HideInInspector]
    public List<PipeSeg> pipeSegs;
    List<Material> materials;
    public float updateTime;

    float currentTime;

    Pipe currentPipe;

    class Pipe
    {
        Vector3 pos;

        int steps;
        PipeDir dir;

        List<Vector3> allDirs;

        int size;

        float segLength;

        Material material;
        HashSet<Vector3> pipeHash;

        Hashtable cornerDirTable = new Hashtable ( )
        { { new PipeDir (new Vector3 (0, 1, 0), new Vector3 (0, 0, 1)), new Vector3 (0, 0, 0) }, //
            { new PipeDir (new Vector3 (0, 1, 0), new Vector3 (1, 0, 0)), new Vector3 (0, 90, 0) }, //
            { new PipeDir (new Vector3 (0, 1, 0), new Vector3 (0, 0, -1)), new Vector3 (0, 180, 0) }, //
            { new PipeDir (new Vector3 (0, 1, 0), new Vector3 (-1, 0, 0)), new Vector3 (0, 270, 0) }, //
            { new PipeDir (new Vector3 (0, -1, 0), new Vector3 (0, 0, -1)), new Vector3 (180, 0, 0) }, //
            { new PipeDir (new Vector3 (0, -1, 0), new Vector3 (1, 0, 0)), new Vector3 (180, 270, 0) }, //
            { new PipeDir (new Vector3 (0, -1, 0), new Vector3 (0, 0, 1)), new Vector3 (180, 180, 0) }, //
            { new PipeDir (new Vector3 (0, -1, 0), new Vector3 (-1, 0, 0)), new Vector3 (180, 90, 0) }, //
            { new PipeDir (new Vector3 (1, 0, 0), new Vector3 (0, -1, 0)), new Vector3 (0, 270, 0) }, //
            { new PipeDir (new Vector3 (1, 0, 0), new Vector3 (0, 0, 1)), new Vector3 (0, 270, 90) }, //
            { new PipeDir (new Vector3 (1, 0, 0), new Vector3 (0, 1, 0)), new Vector3 (0, 270, 180) }, //
            { new PipeDir (new Vector3 (1, 0, 0), new Vector3 (0, 0, -1)), new Vector3 (0, 270, 270) }, //
            { new PipeDir (new Vector3 (0, 0, 1), new Vector3 (0, -1, 0)), new Vector3 (0, 180, 0) }, //
            { new PipeDir (new Vector3 (0, 0, 1), new Vector3 (-1, 0, 0)), new Vector3 (0, 180, 90) }, //
            { new PipeDir (new Vector3 (0, 0, 1), new Vector3 (0, 1, 0)), new Vector3 (0, 180, 180) }, //
            { new PipeDir (new Vector3 (0, 0, 1), new Vector3 (1, 0, 0)), new Vector3 (0, 180, 270) }, //
            { new PipeDir (new Vector3 (0, 0, -1), new Vector3 (0, -1, 0)), new Vector3 (0, 0, 0) }, //
            { new PipeDir (new Vector3 (0, 0, -1), new Vector3 (1, 0, 0)), new Vector3 (0, 0, 90) }, //
            { new PipeDir (new Vector3 (0, 0, -1), new Vector3 (0, 1, 0)), new Vector3 (0, 0, 180) }, //
            { new PipeDir (new Vector3 (0, 0, -1), new Vector3 (-1, 0, 0)), new Vector3 (0, 0, 270) }, //
            { new PipeDir (new Vector3 (-1, 0, 0), new Vector3 (0, -1, 0)), new Vector3 (0, 90, 0) }, //
            { new PipeDir (new Vector3 (-1, 0, 0), new Vector3 (0, 0, -1)), new Vector3 (0, 90, 90) }, //
            { new PipeDir (new Vector3 (-1, 0, 0), new Vector3 (0, 1, 0)), new Vector3 (0, 90, 180) }, //
            { new PipeDir (new Vector3 (-1, 0, 0), new Vector3 (0, 0, 1)), new Vector3 (0, 90, 270) }
        };
        public Pipe (int size, float segLength, Material material, HashSet<Vector3> pipeHash)
        {

            allDirs = new List<Vector3> ( );
            allDirs.Add (Vector3.up);
            allDirs.Add (Vector3.down);
            allDirs.Add (Vector3.left);
            allDirs.Add (Vector3.right);
            allDirs.Add (Vector3.forward);
            allDirs.Add (Vector3.back);

            this.size = size;
            this.segLength = segLength;
            this.material = material;
            this.pipeHash = pipeHash;
            //picking a random starting location
            for (int i = 0; i < 100; i++)
            {
                pos = new Vector3 (Random.Range (3, size - 3), Random.Range (3, size - 3), Random.Range (3, size - 3));
                if (validPos (pos)) break;

            }
            Vector3 startingDir = allDirs[Random.Range (0, allDirs.Count)];
            dir = new PipeDir (startingDir, startingDir);
            steps = Random.Range (0, (int) (size / 2));
        }

        //pos return will be -1 -1 -1 if no valid pipeSeg can be made
        public PipeSeg getNext ( )
        {

            if (steps > 0)
            {

                //if going straight will run into boundry or another pipe
                if (!validPos (pos + dir.nextDir))
                {
                    steps = 0;
                }
                else
                {
                    steps -= 1;
                    dir.currentDir = dir.nextDir;
                    pos += dir.currentDir;
                    pipeHash.Add (pos);
                    return new PipeSeg (false, pos * segLength, Quaternion.FromToRotation (Vector3.up, dir.nextDir), material);
                }
            }
            if (steps == 0)
            {
                List<Vector3> tempDirs = new List<Vector3> (allDirs);
                //cant make pipe go back it self
                tempDirs.Remove (dir.currentDir);
                tempDirs.Remove (-1 * dir.currentDir);
                //using lamda expression :v
                //removing any dirs such that following that dir will go immediately out of bound or colides with a existing pipe
                tempDirs.RemoveAll (vec => !validPos (vec + pos));
                if (tempDirs.Count == 0)
                {

                    return new PipeSeg (false, new Vector3 (-1, -1, -1), Quaternion.identity, null);
                }

                dir.nextDir = tempDirs[Random.Range (0, tempDirs.Count)];
                steps = Random.Range (1, (int) (size / 2));

                Quaternion rot = Quaternion.Euler ((Vector3) cornerDirTable[dir]);
                pos += dir.currentDir;
                pipeHash.Add (pos);
                return new PipeSeg (true, pos * segLength, rot, material);

            }

            return new PipeSeg (false, new Vector3 (-1, -1, -1), Quaternion.identity, null);
        }

        bool validPos (Vector3 vec)
        {
            return vec.x >= 0 && vec.y >= 0 && vec.z >= 0 && vec.x < size && vec.y < size && vec.z < size && !pipeHash.Contains (vec);

        }
    }

    void Start ( )
    {
        initialize ( );
    }

    void initialize ( )
    {
        pipeLocs = new HashSet<Vector3> ( );
        pipeSegs = new List<PipeSeg> ( );
        materials = new List<Material> ( );
        makePipeMesh ( );
        makeCornerMesh ( );
    }

    void makePipeMesh ( )
    {
        pipeMesh = new Mesh ( );
        List<Vector3> vertices = new List<Vector3> ( );
        List<int> tris = new List<int> ( );
        float PIfrac = Mathf.PI * 2 / polyCountPerSeg;

        //upper ring of vertices
        for (int i = 0; i < polyCountPerSeg; i++)
        {
            vertices.Add (new Vector3 (Mathf.Cos (PIfrac * i) * pipeRadius, segmentLength / 2, Mathf.Sin (PIfrac * i) * pipeRadius));
        }
        //lower ring
        for (int i = 0; i < polyCountPerSeg; i++)
        {
            vertices.Add (new Vector3 (Mathf.Cos (PIfrac * i) * pipeRadius, -segmentLength / 2, Mathf.Sin (PIfrac * i) * pipeRadius));
        }

        //forming triangles with the verices.
        for (int i = 0; i < polyCountPerSeg; i++)
        {
            //backwards triangle
            tris.Add (mod ((i - 1), polyCountPerSeg));
            tris.Add (i);
            tris.Add (i + polyCountPerSeg);
            //forward triangle

            tris.Add (i);
            tris.Add (mod (i + 1, polyCountPerSeg) + polyCountPerSeg);
            tris.Add (i + polyCountPerSeg);

        }
        pipeMesh.vertices = vertices.ToArray ( );
        pipeMesh.triangles = tris.ToArray ( );
        pipeMesh.RecalculateNormals ( );
        pipeMesh.RecalculateTangents ( );

    }
    void makeCornerMesh ( )
    {
        cornerMesh = new Mesh ( );
        float PIfrac = Mathf.PI * 2 / polyCountPerSeg;
        float acrFrac = Mathf.PI / (2 * (cornerSegCount + 1)); //quater turn
        Vector3 pivot = new Vector3 (0, -segmentLength / 2, (segmentLength) / 2);

        List<Vector3> vertices = new List<Vector3> ( );
        List<int> tris = new List<int> ( );

        for (int i = 0; i < cornerSegCount + 2; i++)
        {
            for (int j = 0; j < polyCountPerSeg; j++)
            {
                Vector3 tempVec = new Vector3 (Mathf.Cos (PIfrac * j) * pipeRadius, -segmentLength / 2, Mathf.Sin (PIfrac * j) * pipeRadius);

                vertices.Add (rotateAboutPoint (tempVec, pivot, Quaternion.Euler (Mathf.Rad2Deg * i * acrFrac, 0, 0)));
            }
        }

        for (int i = 0; i < cornerSegCount + 1; i++)
        {
            int offset = i * polyCountPerSeg;
            for (int j = 0; j < polyCountPerSeg; j++)
            {
                tris.Add (mod (j - 1, polyCountPerSeg) + offset);
                tris.Add (j + offset);
                tris.Add (j + polyCountPerSeg + offset);

                tris.Add (j + offset);
                tris.Add (mod (j + 1, polyCountPerSeg) + polyCountPerSeg + offset);
                tris.Add (j + polyCountPerSeg + offset);
            }
        }
        tris.Reverse ( );
        cornerMesh.vertices = vertices.ToArray ( );
        cornerMesh.triangles = tris.ToArray ( );
        cornerMesh.RecalculateNormals ( );
    }
    // Update is called once per frame

    int counter = 0;
    int target = 10;
    void Update ( )
    {

        if (currentPipe == null)
        {
            print ("starting again");
            makeCornerMesh ( );
            makePipeMesh ( );
            Material m = new Material (Shader.Find ("Standard"));
            m.color = new Color (Random.Range (0.1f, 0.9f), Random.Range (0.1f, 0.9f), Random.Range (0.1f, 0.9f), 1);
            materials.Add (m);
            currentPipe = new Pipe (size, segmentLength, materials[materials.Count - 1], pipeLocs);
        }

        // if (drawCorner)
        //     Graphics.DrawMesh (cornerMesh, cornerPos, Quaternion.Euler (cornerRot), material, 0, Camera.main, 0, null, true, true, false);
        // if (drawPipe)
        //     Graphics.DrawMesh (pipeMesh, pipePos, Quaternion.Euler (pipeRot), material, 0, Camera.main, 0, null, true, true, false);

        // if (Input.GetKeyUp (KeyCode.Q))
        //     drawCorner = !drawCorner;

        //     drawPipe = !drawPipe;

        foreach (PipeSeg seg in pipeSegs)
        {
            Graphics.DrawMesh (seg.corner?cornerMesh : pipeMesh, seg.location, seg.rotation, seg.material, 0, Camera.main);
        }
        if (currentTime == 0)
        {
            PipeSeg temp = currentPipe.getNext ( );

            if (temp.location == -1 * Vector3.one)
            {
                currentPipe = null;
                counter += 1;

                if (counter > target)
                {
                    pipeSegs.Clear ( );
                    pipeLocs.Clear ( );
                    counter = 0;
                    target = Random.Range (8, 20);
                }
            }
            else
            {
                pipeSegs.Add (temp);
            }
        }
        currentTime += Time.deltaTime;
        if (currentTime > updateTime)
        {
            currentTime = 0;
        }

    }

    //c#'s modulos is questionable
    int mod (int x, int m)
    {
        return (x % m + m) % m;
    }

    Vector3 rotateAboutPoint (Vector3 vector, Vector3 pivot, Quaternion rotation)
    {

        return (rotation * (vector - pivot)) + pivot;
    }

    public struct PipeSeg
    {
        public bool corner;
        public Vector3 location;
        public Quaternion rotation;

        public Material material;

        public PipeSeg (bool corner, Vector3 loc, Quaternion rot, Material mat)
        {
            this.corner = corner;
            location = loc;
            rotation = rot;
            material = mat;
        }
    }

    struct PipeDir
    {
        public Vector3 currentDir;
        public Vector3 nextDir;

        public PipeDir (Vector3 cur, Vector3 next)
        {
            currentDir = cur;
            nextDir = next;
        }

        public override string ToString ( )
        {
            return currentDir.ToString ( ) + " " + nextDir.ToString ( );
        }

        public override bool Equals (object other)
        {
            if (!(other is PipeDir))
            {
                return false;
            }

            PipeDir temp = (PipeDir) other;
            return currentDir == temp.currentDir && nextDir == temp.nextDir;
        }

        public override int GetHashCode ( )
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + currentDir.GetHashCode ( );
                hash = hash * 29 + nextDir.GetHashCode ( );
                return hash;
            }
        }
    }
}