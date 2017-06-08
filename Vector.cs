using System;
using System.Collections.Generic;
using UnityEngine;

public class Vector : IComparable<Vector> {
    public float x;
    public float y;

    public Vector(float x = 0.0f, float y = 0.0f) {
        this.x = x;
        this.y = y;
    }

    public Vector(Vector v) {
        this.x = v.x;
        this.y = v.y;
    }

    public int CompareTo(Vector v) {
        if (this.y < v.y) {
            return -1;
        } else if (Mathf.Approximately(this.y, v.y)) {
            if (Mathf.Approximately(this.x, v.x)) {
                return 0;
            } else if (this.x > v.x) {
                return 1;
            } else {
                return -1;
            }
        } else {
            return 1;
        }
    }

    public float Magnitude {
        get {
            return Mathf.Sqrt(this.x * this.x + this.y * this.y);
        }
    }
}

public class VectorYComparer : IComparer<Vector> {
    public int Compare(Vector v1, Vector v2) {
        if (v1.y < v2.y) {
            return -1;
        } else if (Mathf.Approximately(v1.y, v2.y)) {
            if (Mathf.Approximately(v1.x, v2.x)) {
                return 0;
            } else if (v1.x > v2.x) {
                return 1;
            } else {
                return -1;
            }
        } else {
            return 1;
        }
    }
}

public class VectorXComparer : IComparer<Vector> {
    public int Compare(Vector v1, Vector v2) {
        if (v1.x < v2.x) {
            return -1;
        } else if (Mathf.Approximately(v1.x, v2.x)) {
            if (v1.y > v2.y) {
                return 1;
            } else if (Mathf.Approximately(v1.y, v2.y)) {
                return 0;
            } else {
                return -1;
            }
        } else {
            return 1;
        }
    }
}