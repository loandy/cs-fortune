using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using BinaryTree;

public class Event {
    public Vector Position;
    public Vector Center;
    public BinaryTreeNode<Arc> Arc;

    public Event(Vector event_position) {
        this.Position = new Vector(event_position.x, event_position.y);
        this.Center = null;
        this.Arc = null;
    }

    public Event(Vector event_position, float center_y, BinaryTreeNode<Arc> arc) {
        this.Position = new Vector(event_position.x, event_position.y);
        this.Center = new Vector(this.Position.x, center_y);
        this.Arc = arc;
    }

    public bool IsCircleEvent {
        get {
            return (this.Arc != null);
        }
    }

    public float Radius {
        get {
            return Mathf.Abs(this.Position.y - this.Center.y);
        }
    }
}

public class EventQueue : Collection<Event> {
    private int findParentIndex(int index) {
        return (index - 1) / 2;
    }

    private int findLeftChildIndex(int index) {
        return index * 2 + 1;
    }

    private int findRightChildIndex(int index) {
        return index * 2 + 2;
    }

    private void sink(int i) {
        int least = i;
        int left, right;

        do {
            i = least;
            left = this.findLeftChildIndex(i);
            right = this.findRightChildIndex(i);

            if (left < base.Count && this.Items[left].Position.CompareTo(this.Items[i].Position) < 0) {
                least = left;
            } else {
                least = i;
            }

            if (right < base.Count && this.Items[right].Position.CompareTo(this.Items[least].Position) < 0) {
                least = right;
            }

            if (i != least) {
                Event e = base.Items[i];
                base.Items[i] = base.Items[least];
                base.Items[least] = e;
            }
        } while (i != least);

        return;
    }

    private void swim(int i) {
        int parent_index = this.findParentIndex(i);

        while(i > 0 && base.Items[i].Position.CompareTo(base.Items[parent_index].Position) < 0) {
            Event e = base.Items[i];
            base.Items[i] = base.Items[parent_index];
            base.Items[parent_index] = e;
            i = parent_index;
            parent_index = this.findParentIndex(i);
            guard++;
        }

        return;
    }

    /**
     *
     * This function gets very mathy. It is based on a technique for calculating circumcircles
     * using coordinate transformations found in Mathematical Elements for Computer Graphics
     * (2nd Edition) by Rogers and Adams.
     *
     */
    public Event CreateCircleEvent(BinaryTreeNode<Arc> central_arc) {
        if (central_arc == null || !central_arc.IsLeaf) {
            throw new System.ArgumentException ("Must provide a leaf node to create circle event.");
        }

        BinaryTreeNode<Arc> left_arc = central_arc.FindPrevLeaf();
        BinaryTreeNode<Arc> right_arc = central_arc.FindNextLeaf();

        if (left_arc == null || right_arc == null) {
            Debug.Log("Invalid circle - cannot form triangle due to missing point.");
            return null;
        }

        if (left_arc.Value.Site.CompareTo(right_arc.Value.Site) == 0) {
            Debug.Log("Invalid circle - adjacent points are coincedent.");
            return null;
        }
            
        Vector center = new Vector();
        float radius;

        Vector a = left_arc.Value.Site;
        Vector b = central_arc.Value.Site;
        Vector c = right_arc.Value.Site;

        // Translate all points such that the left arc site lies at the origin.
        Vector b_prime = new Vector(b.x - a.x, b.y - a.y);
        Vector c_prime = new Vector(c.x - a.x, c.y - a.y);

        // Rotate all points about the origin such that one site lies on the positive x-axis (if necessary).
        Vector b_2prime = b_prime;
        Vector c_2prime = c_prime;
        float theta = Mathf.Atan2(b_prime.y, b_prime.x);
        bool rotate = !Mathf.Approximately(theta, 0) ? true : false;

        // For rotating the points back.
        float cos_theta = Mathf.Cos(theta);
        float sin_theta = Mathf.Sin(theta);

        if (rotate) {
            // For rotating the points into a new space.
            float cos_neg_theta = Mathf.Cos(-theta);
            float sin_neg_theta = Mathf.Sin(-theta);

            // Rotations! We don't need to rotate a_prime since it is at the point of rotation (origin).
            b_2prime = new Vector();
            b_2prime.x = b_prime.x * cos_neg_theta - b_prime.y * sin_neg_theta;
            b_2prime.y = b_prime.y * cos_neg_theta - b_prime.x * sin_neg_theta;
            c_2prime = new Vector();
            c_2prime.x = c_prime.x * cos_neg_theta - c_prime.y * sin_neg_theta;
            c_2prime.y = c_prime.y * cos_neg_theta - c_prime.x * sin_neg_theta;
        }

        if (Mathf.Approximately(c_2prime.y, 0)) {
            // If transformed point c lies on the x-axis, this means all points are collinear,
            // thereby no circumcircle can be formed.
            Debug.Log("Invalid circle - points are collinear.");
            return null;
        }

        float h_2prime = b_2prime.x / 2;
        float k_2prime = c_2prime.x * (c_2prime.x - b_2prime.x) / (2 * c_2prime.y) + (c_2prime.y / 2);
        radius = Mathf.Sqrt(h_2prime * h_2prime + k_2prime * k_2prime);

        // Convert results back to original coordinate space.
        float h_prime = h_2prime * cos_theta - k_2prime * sin_theta;
        float k_prime = k_2prime * cos_theta - h_2prime * sin_theta;
        center.x = h_prime + a.x;
        center.y = k_prime + a.y;

        if (center.x < 0 || center.x > 512 || center.y < 0 || center.y > 512) {
            Debug.Log("Invalid circle - Circumcircle center is out of bounds.");
            return null;
        }

        // A little cheaty. Peeks through the event queue (into the future) to determine whether
        // there are any sites inside the circumcircle that may invalidate it. If so, abandon the
        // circle event.
        foreach (Event e in this) {
            while (e.Position.CompareTo(center) < 0) {
                if (!e.IsCircleEvent) {
                    float hypotenuse = e.Position.x * e.Position.x + e.Position.y * e.Position.y;
                    if (hypotenuse < (radius * radius)) {
                        Debug.Log("Invalid circle - site within circumcircle <" + e.Position.x + ", " + e.Position.y + ">.");
                        return null;
                    } else {
                        break;
                    }
                }
            }
        }

        Vector circumcircle_top = new Vector(center.x, center.y + radius);
        Event circle_event = new Event(circumcircle_top, center.y, central_arc);
        central_arc.Value.CircleEvent = circle_event;

        return circle_event;
    }

    /*public void RemoveEvent(Event circle_event) {
        Debug.Log("RemoveEvent called.");
        for (int i = 0; i < base.Count - 1; i++) {
            if (circle_event.Position.CompareTo(base.Items[i].Position) == 0) {
                int last_index = base.Count - 1;
                base.Items[i] = base.Items[last_index];
                base.Items.RemoveAt(last_index);

                int parent_index = this.findParentIndex(i);
                if (base.Items[i].Position.CompareTo(base.Items[parent_index].Position) < 0) {
                    this.swim(i);
                } else if (base.Items[i].Position.CompareTo(base.Items[parent_index].Position) > 0) {
                    this.sink(i);
                }
            }
        }
    }*/

    public EventQueue() { }

    public bool Any() {
        return base.Items.Any();
    }

    public void Insert(Event e) {
        base.Items.Add(e);
        this.swim(base.Items.Count - 1);

        return;
    }

    public void InsertCircleEvent(BinaryTreeNode<Arc> arc) {
        Event e = this.CreateCircleEvent(arc);

        if (e != null) {
            this.Insert(e);
        }
    }

    public Event Peek()
    {
        if (base.Items.Count < 1) {
            return null;
        }

        return base.Items[0];
    }

    public Event ExtractMin()
    {
        if (base.Items.Count < 1) {
            return null;
        }

        Event min = base.Items[0];
        int last_index = base.Items.Count - 1;
        base.Items[0] = base.Items[last_index];
        base.Items.RemoveAt(last_index);
        this.sink(0);

        return min;
    }
}