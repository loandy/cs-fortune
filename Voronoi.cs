using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour {

    // Use this for initialization
    void Start() {
        VoronoiDiagram voronoi_diagram = new VoronoiDiagram(200);
        displayVoronoiDiagram(voronoi_diagram);
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    private void displayVoronoiDiagram(VoronoiDiagram diagram)
    {
        Texture2D screen = new Texture2D(512, 512);

        foreach (Vector s in diagram.Sites) {
            screen.SetPixel((int)s.x, (int)s.y, Color.red);
        }

        foreach (VoronoiVertex v in diagram.Vertices) {
            screen.SetPixel((int)v.x, (int)v.y, Color.black);
        }

        screen.Apply();

        this.GetComponent<Renderer>().material.mainTexture = screen;
    }

    // Bresenham line algorithm
    private void DrawLine(Vector p0, Vector p1, Texture2D tx, Color c, int offset = 0) {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            tx.SetPixel(x0 + offset, y0 + offset, c);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
