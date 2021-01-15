using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawGrid : MonoBehaviour {
    // Start is called before the first frame update
    public Material material;
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }
    void OnPostRender () {
        material.SetPass (0);
        GL.Begin (GL.LINES);
        GL.Color (Color.black);
        renderGrid ();
        GL.End ();
    }

    void renderGrid () {
        for (int x = -2; x <= 1; x++) {
            for (int z = -2; z <= 1; z++) {
                GL.Vertex (new Vector3 (x + 0.5f, -2 + 0.5f, z + 0.5f));
                GL.Vertex (new Vector3 (x + 0.5f, 1 + 0.5f, z + 0.5f));
            }
        }
        for (int y = -2; y <= 1; y++) {
            for (int z = -2; z <= 1; z++) {
                GL.Vertex (new Vector3 (1 + 0.5f, y + 0.5f, z + 0.5f));
                GL.Vertex (new Vector3 (-2 + 0.5f, y + 0.5f, z + 0.5f));
            }
        }

        for (int y = -2; y <= 1; y++) {
            for (int x = -2; x <= 1; x++) {
                GL.Vertex (new Vector3 (x + 0.5f, y + 0.5f, 1 + 0.5f));
                GL.Vertex (new Vector3 (x + 0.5f, y + 0.5f, -2 + 0.5f));
            }
        }

        GL.Color (Color.green);
        GL.Vertex (Vector3.zero);
        GL.Vertex (Vector3.up);

        GL.Color (Color.red);
        GL.Vertex (Vector3.zero);
        GL.Vertex (Vector3.right);

        GL.Color (Color.blue);
        GL.Vertex (Vector3.zero);
        GL.Vertex (Vector3.forward);
    }
}