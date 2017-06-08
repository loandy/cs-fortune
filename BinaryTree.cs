using System.Collections.ObjectModel;
using UnityEngine;

namespace BinaryTree {
    public class Node<T> {
        private T data;
        private NodeList<T> children;

        public Node() { }

        public Node(T data) : this(data, null) { }

        public Node(T data, NodeList<T> children) {
            this.data = data;
            this.children = children;
        }

        public T Value {
            get {
                return this.data;
            }
            set {
                this.data = value;
            }
        }

        protected NodeList<T> Children {
            get {
                return this.children;
            }
            set {
                this.children = value;
            }
        }
    }

    public class NodeList<T> : Collection<Node<T>> {
        public NodeList() : base() { }

        public NodeList(int initial_size) {
            for (int i = 0; i < initial_size; i++) {
                base.Items.Add(default(Node<T>));
            }
        }

        public Node<T> FindByValue(T value) {
            foreach (Node<T> node in this.Items) {
                if (node.Value.Equals(value)) {
                    return node;
                }
            }

            return null;
        }
    }

    public class BinaryTreeNode<T> : Node<T> {
        private BinaryTreeNode<T> parent;
        private bool left_thread, right_thread;

        public BinaryTreeNode(BinaryTreeNode<T> parent = null, bool right_thread = false,
            bool left_thread = false) : base() {
            this.parent = parent;
            this.left_thread = left_thread;
            this.right_thread = right_thread;
        }

        public BinaryTreeNode(T data, BinaryTreeNode<T> parent = null, bool right_thread = false,
            bool left_thread = false) : base(data, null) {
            this.parent = parent;
            this.left_thread = left_thread;
            this.right_thread = right_thread;
        }

        public BinaryTreeNode(T data, BinaryTreeNode<T> left, BinaryTreeNode<T> right,
            BinaryTreeNode<T> parent = null, bool right_thread = false, bool left_thread = false) {
            this.parent = parent;
            this.left_thread = left_thread;
            this.right_thread = right_thread;

            base.Value = data;

            NodeList<T> children = new NodeList<T>(2);
            children[0] = left;
            children[1] = right;

            base.Children = children;
        }

        public bool IsLeftThreaded {
            get {
                return this.left_thread;
            }
            set {
                this.left_thread = value;
            }
        }

        public bool IsRightThreaded {
            get {
                return this.right_thread;
            }
            set {
                this.right_thread = value;
            }
        }

        public bool IsLeaf {
            get {
                if (base.Children == null
                    || (base.Children[0] == null && this.right_thread)
                    || (base.Children[1] == null && this.left_thread)
                    || (this.right_thread && this.left_thread)) {
                    return true;
                } else {
                    return false;
                }
            }
        }

        public BinaryTreeNode<T> Parent {
            get {
                return this.parent;
            }
            set {
                this.parent = value;
            }
        }

        public BinaryTreeNode<T> Left {
            get {
                if (base.Children == null) {
                    return null;
                } else {
                    return (BinaryTreeNode<T>)base.Children[0];
                }
            }
            set {
                if (base.Children == null) {
                    base.Children = new NodeList<T>(2);
                }

                base.Children[0] = value;
            }
        }

        public BinaryTreeNode<T> Right {
            get {
                if (base.Children == null) {
                    return null;
                } else {
                    return (BinaryTreeNode<T>)base.Children[1];
                }
            }
            set {
                if (base.Children == null) {
                    base.Children = new NodeList<T>(2);
                }

                base.Children[1] = value;
            }
        }

        public BinaryTreeNode<T> FindPrevLeaf() {
            BinaryTreeNode<T> current_node = this;

            if (current_node.IsLeftThreaded && current_node.IsLeaf) {
                current_node = current_node.Left;
            }

            if (current_node != null) {
                current_node = current_node.Left;
                while (!current_node.IsLeaf) {
                    current_node = current_node.Right;
                }
            }

            return current_node;
        }

        public BinaryTreeNode<T> FindNextLeaf() {
            BinaryTreeNode<T> current_node = this;

            if (current_node.IsRightThreaded && current_node.IsLeaf) {
                current_node = current_node.Right;
            }

            if (current_node != null) {
                current_node = current_node.Right;
                while (!current_node.IsLeaf) {
                    current_node = current_node.Left;
                }
            }

            return current_node;
        }
    }

    public class BinaryTree<T> {
        protected BinaryTreeNode<T> root;

        public BinaryTree() {
            this.root = null;
        }

        public virtual void Clear() {
            this.root = null;
        }

        public BinaryTreeNode<T> Root {
            get {
                return this.root;
            }
            set {
                this.root = value;
            }
        }
    }
}