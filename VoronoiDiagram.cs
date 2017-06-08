using System.Collections.Generic;
using UnityEngine;

using BinaryTree;

public class HalfEdge {
    public Vector Head;
    public Vector Tail;
    public Vector Region;

    public HalfEdge(Vector site) {
        this.Head = null;
        this.Tail = null;
        this.Region = site;
    }
}

public class VoronoiEdge {
    public BinaryTreeNode<Arc> Left;
    public BinaryTreeNode<Arc> Right;
    public VoronoiVertex Start;
    public VoronoiVertex Stop;

    public VoronoiEdge(BinaryTreeNode<Arc> left, BinaryTreeNode<Arc> right,
        VoronoiVertex start = null, VoronoiVertex stop = null) {
        this.Left = left;
        this.Right = right;
        this.Start = start;
        this.Stop = stop;
    }

    public float Slope {
        get {
            float m = (this.Right.Value.Site.y - this.Left.Value.Site.y) / (this.Right.Value.Site.x -
                this.Left.Value.Site.x);
            return -1 / m;
        }
    }
}

public class VoronoiVertex : Vector {
    public VoronoiVertex(float x, float y) : base(x, y) { }
}

public class VoronoiDiagram {
    private List<Vector> sites;
    //private List<Cell> cells;
    //private List<HalfEdge> half_edges;
    private List<VoronoiEdge> edge_list;
    private List<VoronoiVertex> vertex_list;
    private EventQueue event_queue;
    private SweepTable sweep_table;
    
    private void processSiteEvent(Event site_event) {
        BinaryTreeNode<Arc> inserted_node = this.sweep_table.Insert(site_event.Position);

        if (inserted_node != null && !inserted_node.IsLeaf) {
            BinaryTreeNode<Arc> central_arc = inserted_node.Left.Right;
            BinaryTreeNode<Arc> left_arc = inserted_node.Left.Left;
            BinaryTreeNode<Arc> right_arc = inserted_node.Right;

            if (left_arc.Value.Site.CompareTo(right_arc.Value.Site) == 0) {
                VoronoiEdge new_edge = new VoronoiEdge(left_arc, central_arc);
                left_arc.Parent.Value.Edge = right_arc.Parent.Value.Edge = new_edge;
                this.edge_list.Add(new_edge);
            } else {
                // New site lies directly above a breakpoint, creating a vertex event that ends an
                // edge and creates two more.
                Event circle_event = this.event_queue.CreateCircleEvent(central_arc);
                
                VoronoiVertex v = new VoronoiVertex(circle_event.Center.x, circle_event.Center.y);
                this.vertex_list.Add(v);

                // Finish edge.
                VoronoiEdge old_edge = right_arc.Value.Edge;
                old_edge.Start = v;

                // Create new edges formed from the new vertex.
                VoronoiEdge left_edge = new VoronoiEdge(left_arc, central_arc, null, v);
                left_arc.Parent.Value.Edge = left_edge;
                this.edge_list.Add(left_edge);

                VoronoiEdge right_edge = new VoronoiEdge(central_arc, right_arc, null, v);
                right_arc.Parent.Value.Edge = right_edge;
                this.edge_list.Add(right_edge);
            }

            // Remove old circle event.
            //this.event_queue.RemoveEvent(central_arc.Value.CircleEvent);

            // Check the new left and right sections for circle events.
            Debug.Log("Inserting circle events into event queue.");
            this.event_queue.InsertCircleEvent(left_arc);
            this.event_queue.InsertCircleEvent(right_arc);
        }
    }

    private void processCircleEvent(Event circle_event) {
        if (circle_event.Arc == null) {
            Debug.Log("No valid circle event found.");
            return;
        }

        BinaryTreeNode<Arc> center_arc = circle_event.Arc;
        BinaryTreeNode<Arc> left_arc = circle_event.Arc.FindPrevLeaf();
        BinaryTreeNode<Arc> right_arc = circle_event.Arc.FindNextLeaf();

        VoronoiVertex v = new VoronoiVertex(circle_event.Center.x, circle_event.Center.y);
        this.vertex_list.Add(v);

        left_arc.Parent.Value.Edge.Start = v;
        right_arc.Parent.Value.Edge.Start = v;

        VoronoiEdge new_edge = new VoronoiEdge(left_arc, right_arc, null, v);
        left_arc.Parent.Value.Edge = new_edge;
        right_arc.Parent.Value.Edge = new_edge;
        this.edge_list.Add(new_edge);

        // Check the new left and right sections for circle events.
        this.event_queue.InsertCircleEvent(left_arc);
        this.event_queue.InsertCircleEvent(right_arc);
    }

    private List<Vector> generateSites(int number_of_sites) {
        List<Vector> sites = new List<Vector>();

        for (int i = 0; i < number_of_sites; i++) {
            sites.Add(new Vector(Random.Range(0, 512), Random.Range(0, 512)));
        }

        return sites;
    }

    private void init() {
        // This initialization should be shared by all the constructors.
        this.edge_list = new List<VoronoiEdge>();
        this.vertex_list = new List<VoronoiVertex>();
        this.event_queue = new EventQueue();
        this.sweep_table = new SweepTable();
    }

    public VoronoiDiagram() {
        this.init();
    }

    public VoronoiDiagram(int number_of_sites) {
        this.init();
        this.GenerateVoronoiDiagram(number_of_sites);
    }

    public List<Vector> Sites {
        get {
            return this.sites;
        }
    }

    /*public List<HalfEdge> Cells {
        get {
            return this.cells;
        }
    }*/

    /*public List<HalfEdge> HalfEdges {
        get {
            return this.half_edges;
        }
    }*/

    public List<VoronoiEdge> Edges {
        get {
            return this.edge_list;
        }
    }
    public List<VoronoiVertex> Vertices {
        get {
            return this.vertex_list;
        }
    }

    public void GenerateVoronoiDiagram(int number_of_sites) {
        // Generate an ordered list of sites.
        this.sites = this.generateSites(number_of_sites);

        // Initialize event queue using generated sites.
        foreach (Vector site in sites) {
            this.event_queue.Insert(new Event(site));
        }

        // Process the event queue.
        int site_count = 0, circle_count = 0;
        while (event_queue.Any()) {
            Debug.Log("Retrieving next event from queue (currently " + event_queue.Count + ").");
            Event e = event_queue.ExtractMin();
            Debug.Log("Event queue has " + event_queue.Count + " events remaining.");

            if (!e.IsCircleEvent) {
                Debug.Log("Handling site event at <" + e.Position.x + ", " + e.Position.y + ">");
                this.processSiteEvent(e);
                site_count++;
            } else {
                Debug.Log("Handling circle event at <" + e.Position.x + ", " + e.Position.y + ">");
                //this.processCircleEvent(e);
                circle_count++;
            }
        }

        Debug.Log("FINISHED EXECUTION\n--------------------\n" +
            "Site events: " + site_count + "\nCircle events: " + circle_count);
    }
}