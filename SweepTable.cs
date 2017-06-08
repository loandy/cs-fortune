using System.Collections.Generic;
using UnityEngine;

using BinaryTree;

public class Arc {
    public Vector Site;
    public VoronoiEdge Edge;
    public Event CircleEvent;

    public Arc(Vector site = null, VoronoiEdge edge = null, Event circle_event = null) {
        this.Site = site;
        this.Edge = edge;
        this.CircleEvent = circle_event;
    }
}

public class SweepTable : BinaryTree<Arc> {
    private float getIntersection(BinaryTreeNode<Arc> edge_node, float sweep_line) {
        Vector p1 = edge_node.FindPrevLeaf().Value.Site;
        Vector p2 = edge_node.FindNextLeaf().Value.Site;
        float breakpoint;

        if (p1.y == p2.y) {
            breakpoint = (p1.x + p2.x) / 2;
        } else if (p1.y == sweep_line) {
            // If *only* one of the sites is being swept by the sweep line,
            // then that site forms a vertical line segment straight down.
            breakpoint = p1.x;
        } else if (p2.y == sweep_line) {
            // Same as previous case, only with the other site being swept.
            breakpoint = p2.x;
        } else {
            breakpoint = (p1.y * p2.x - Mathf.Sqrt(p1.y * p2.y * ((p1.y - p2.y) * (p1.y - p2.y) + p2.x * p2.x))) /
                (p1.y - p2.y);
        }

        return breakpoint;
    }

    private float findIntersection(BinaryTreeNode<Arc> node, float k) {
        Vector s1 = node.FindPrevLeaf().Value.Site;
        Vector s2 = node.FindNextLeaf().Value.Site;
        float breakpoint;

        float a = 0.0f;
        float b = 0.0f;
        float c = 0.0f;

        if (Mathf.Approximately(s1.y, s2.y)) {
            breakpoint = (s1.x + s2.x) / 2;
        } else if (s1.y == k) {
            // If *only* one of the sites is being swept by the sweep line,
            // then that site forms a vertical line segment straight down.
            breakpoint = s1.x;
            a = 1 / (2 * (s2.y - k));
            b = -2.0f * a * s2.x;
            c = (s2.x * s2.x * a) + (1 / (2 * (s2.y + k)));
        } else if (s2.y == k) {
            // Same as previous case, only with the other site being swept.
            breakpoint = s2.x;
            a = 1 / (2 * (s1.y - k));
            b = -2.0f * a * s1.x;
            c = (s1.x * s1.x * a) + (1 / (2 * (s1.y + k)));
        } else {
            // Calculate the standard form coefficients for each parabola.
            float s1a = 1 / (2 * (s1.y - k));
            float s2a = 1 / (2 * (s2.y - k));
            float s1b = -2.0f * s1a * s1.x;
            float s2b = -2.0f * s2a * s2.x;
            float s1c = (s1.x * s1.x * s1a) + (1 / (2 * (s1.y + k)));
            float s2c = (s2.x * s2.x * s2a) + (1 / (2 * (s2.y + k)));

            // Equalize the two quadratic equations.
            a = s1a - s2a;
            b = s1b - s2b;
            c = s1c - s2c;

            float discriminant = b * b - 4 * a * c;
            float r1, r2;
            if (Mathf.Approximately(discriminant, 0)) {
                // If the discriminant is 0, both roots should be equal.
                breakpoint = -b / 2 * a;
            } else if (discriminant > 0) {
                // If the discriminant is positive, two real roots exist.
                r1 = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
                r2 = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

                if (s1.y < s2.y) {
                    breakpoint = Mathf.Min(r1, r2);
                } else {
                    breakpoint = Mathf.Max(r1, r2);
                }
            } else {
                // If the discriminant is negative, no real roots exist.
                breakpoint = float.NegativeInfinity;
            }
        }

        return breakpoint;
    }

    private BinaryTreeNode<Arc> insertNode(ref BinaryTreeNode<Arc> insert_node,
        BinaryTreeNode<Arc> new_node) {
        if (insert_node == null) {
            insert_node = new_node;
            insert_node.IsLeftThreaded = true;
            insert_node.IsRightThreaded = true;
        } else if (insert_node.IsLeaf) {
            insert_node.Value.Site.x + ", " + insert_node.Value.Site.y + ">.");

            BinaryTreeNode<Arc> central_arc = new_node;
            BinaryTreeNode<Arc> left_arc = new BinaryTreeNode<Arc>(insert_node.Value);
            BinaryTreeNode<Arc> right_arc = new BinaryTreeNode<Arc>(insert_node.Value);
            BinaryTreeNode<Arc> left_subtree = new BinaryTreeNode<Arc>(new Arc());

            left_arc.Parent = left_subtree;
            left_arc.Left = insert_node.Left;
            left_arc.Right = left_subtree;
            left_arc.IsLeftThreaded = true;
            left_arc.IsRightThreaded = true;

            central_arc.Parent = left_subtree;
            central_arc.Left = left_subtree;
            central_arc.Right = insert_node;
            central_arc.IsLeftThreaded = true;
            central_arc.IsRightThreaded = true;

            right_arc.Parent = insert_node;
            right_arc.Left = insert_node;
            right_arc.Right = insert_node.Right;
            right_arc.IsLeftThreaded = true;
            right_arc.IsRightThreaded = true;

            left_subtree.Parent = insert_node;
            left_subtree.Left = left_arc;
            left_subtree.Right = central_arc;

            // Invalidates connected circle event.
            if (insert_node.Value.CircleEvent != null) {
                insert_node.Value.CircleEvent.Arc = null;
            }

            insert_node.Value = new Arc();
            insert_node.Left = left_subtree;
            insert_node.Right = right_arc;
            insert_node.IsLeftThreaded = false;
            insert_node.IsRightThreaded = false;
        } else {
            BinaryTreeNode<Arc> left_subtree = new BinaryTreeNode<Arc>();
            left_subtree.Parent = insert_node;

            left_subtree.Left = insert_node.Left;
            left_subtree.Left.Parent = left_subtree;
            left_subtree.Left.Right = left_subtree;
            left_subtree.Left.IsLeftThreaded = true;
            left_subtree.Left.IsRightThreaded = true;

            left_subtree.Right = new_node;
            left_subtree.Right.Parent = left_subtree;
            left_subtree.Right.Left = left_subtree;
            left_subtree.Right.Right = insert_node;
            left_subtree.Right.IsLeftThreaded = true;
            left_subtree.Right.IsRightThreaded = true;

            insert_node.Left = left_subtree;
        }

        return insert_node;
    }

    private void detachNode(BinaryTreeNode<Arc> node) {
        BinaryTreeNode<Arc> prev, next;
        prev = node.Left;
        next = node.Right;

        if (prev.IsLeaf) {
            prev.Parent = node.Parent;
        } else if (next.IsLeaf) {
            next.Parent = node.Parent;
        }

        prev.Right = node.Right;
        next.Left = node.Left;
    }

    public SweepTable() : base() { }

    public BinaryTreeNode<Arc> Insert(Vector site) {
        BinaryTreeNode<Arc> new_node = new BinaryTreeNode<Arc>(new Arc(site));
        BinaryTreeNode<Arc> current_node = this.root;
        BinaryTreeNode<Arc> inserted_node = null;

        if (this.root == null) {
            inserted_node = this.insertNode(ref this.root, new_node);
        } else {
            while (inserted_node == null) {
                if (current_node.IsLeaf) {
                    inserted_node = this.insertNode(ref current_node, new_node);
                } else {
                    float breakpoint = this.findIntersection(current_node, site.y);

                    /*if (Mathf.Approximately(new_node.Value.Site.x, breakpoint)) {
                        Debug.Log("New site falls on breakpoint at " + breakpoint);
                        inserted_node = this.insertNode(ref current_node, new_node);
                    } else*/ if (new_node.Value.Site.x < breakpoint) {
                        current_node = current_node.Left;
                    } else {
                        current_node = current_node.Right;
                    }
                }
            }
        }

        return inserted_node;
    }

    public void Remove(BinaryTreeNode<Arc> site) {
        if (site == null || !site.IsLeaf) {
            throw new System.ArgumentException("Must provide a leaf node to remove.");
        }

        BinaryTreeNode<Arc> prev = site.FindPrevLeaf();
        BinaryTreeNode<Arc> next = site.FindNextLeaf();

        this.detachNode(site);
        this.detachNode(site.Parent);

        if (prev.Value.Site.CompareTo(next.Value.Site) == 0) {
            this.detachNode(prev);
            this.detachNode(prev.Parent);
        }
    }
}