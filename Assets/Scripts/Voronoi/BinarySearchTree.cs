using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace TyVoronoi {

    public class BinarySearchTreeNode<T> {
        public T data;
        public BinarySearchTreeNode<T> parent;
        public BinarySearchTreeNode<T> left;
        public BinarySearchTreeNode<T> right;

        public BinarySearchTreeNode(
            T data
        ) {
            this.data = data;
        }
    }

    public class BinarySearchTree<T> {
        protected IComparer comparer;
        protected BinarySearchTreeNode<T> root;

        public BinarySearchTree() : this((IComparer)null) { }
        public BinarySearchTree(
            IComparer comparer
        ) {
            this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
        }


        void _DumpNode(
            BinarySearchTreeNode<T> node,
            int depth,
            int shift,
            List<T[]> dumpTable
        ) {
            // expand dumpTable for depth (if required)
            if (dumpTable.Count <= depth) {
                dumpTable.Add(new T[1<<depth]);
            }

            // dump root
            dumpTable[depth][shift] = node.data;

            // left child
            if(node.left != null) {
                _DumpNode(node.left, depth+1, shift<<1, dumpTable);
            }
            // right child
            if (node.right != null) {
                _DumpNode(node.right, depth+1, (shift<<1)+1, dumpTable);
            }
        }

        public void Dump() {
            var dumpTable = new List<T[]>();
            if (root != null) {
                _DumpNode(root, 0, 0, dumpTable);
            }
            var rows = "";
            for (var depth=0; depth<dumpTable.Count; depth++) {
                var padMultiplier = dumpTable.Count-depth;
                rows += String.Join("", dumpTable[depth].Select(v=>String.Format("{0}", v).PadLeft(12*padMultiplier)).ToArray()) + "\n";
            }
            Debug.Log("rows:\n" + rows);
        }

        protected BinarySearchTreeNode<T> _InsertNode(
            BinarySearchTreeNode<T> parent,
            ref BinarySearchTreeNode<T> node,
            T data,
            IComparer comparer = null
        ) {
            if (comparer == null) comparer = this.comparer;
            if (node == null) {
                node = new BinarySearchTreeNode<T>(data);
                node.parent = parent;
                return node;
            } else if (comparer.Compare(data, node.data) < 0) {
                return _InsertNode(node, ref node.left, data, comparer);
            } else {
                return _InsertNode(node, ref node.right, data, comparer);
            }
        }

        public virtual object Insert(
            T data,
            IComparer comparer = null
        ) {
            return (object) _InsertNode(null, ref root, data, comparer);
        }

        protected static BinarySearchTreeNode<T> _FindMinNode(
            BinarySearchTreeNode<T> node
        ) {
            if (node == null) return null;
            while(node.left != null) {
                node = node.left;
            }
            return node;
        }
        static BinarySearchTreeNode<T> _FindMaxNode(
            BinarySearchTreeNode<T> node
        ) {
            if (node == null) return null;
            while(node.right != null) {
                node = node.right;
            }
            return node;
        }

        static void _ReplaceNode(
            BinarySearchTreeNode<T> node,
            BinarySearchTreeNode<T> replacement
        ) {
            if (node.parent != null) {
                if (node.parent.left == node) {
                    node.parent.left = replacement;
                } else {
                    node.parent.right = replacement;
                }
            }
            if (replacement != null) {
                replacement.parent = node.parent;
            }
        }

        protected void _DeleteNode(
            BinarySearchTreeNode<T> node
        ) {

            // two children, need to find successor
            if (node.left != null && node.right != null) {
                BinarySearchTreeNode<T> childToPromote;
                // randomize successor or predecessor to promote
                if (UnityEngine.Random.Range(0,2) == 0) {
                    childToPromote = _FindMinNode(node.right);
                } else {
                    childToPromote = _FindMaxNode(node.left);
                }

                // replace current node's data w/ that of child
                node.data = childToPromote.data;

                // remove child from tree
                _DeleteNode(childToPromote);

            // one child, promote child
            } else if (node.left != null) {
                _ReplaceNode(node, node.left);
                //node = node.left;
            } else if (node.right != null) {
                //node = node.right;
                _ReplaceNode(node, node.right);

            // no kids... just party
            } else {
                //node = null;
                _ReplaceNode(node, node.right);
            }
        }

        public virtual void Delete(
            T data,
            IComparer comparer = null
        ) {
            if (root != null) {
                var node = _Search(root, data, comparer);
                if (node != null) _DeleteNode(node);
            }
        }

        public virtual void DeleteNode(
            object node
        ) {
            var treeNode = (BinarySearchTreeNode<T>) node;
            if (node != null) _DeleteNode(treeNode);
        }

        public T GetNodeData(
            object node
        ) {
            var treeNode = (BinarySearchTreeNode<T>) node;
            if (node != null) {
                return treeNode.data;
            } else {
                return default(T);
            }
        }

        public object GetPredecessor(
            object node
        ) {
            if (node != null) {
                var treeNode = (BinarySearchTreeNode<T>) node;
                if (treeNode.left != null) {
                    treeNode = _FindMaxNode(treeNode.left);
                    return (object) treeNode;
                } else {
                    // traverse up the tree as long as the current node is the left child of parent
                    // if we find that current node is the right child of parent, parent becomes predecessor
                    while (treeNode.parent != null && treeNode.parent.left == treeNode) {
                        treeNode = treeNode.parent;
                    }
                    if (treeNode.parent != null && treeNode.parent.right == treeNode) {
                        return (object) treeNode.parent;
                    }
                    return null;
                }
            }
            return null;
        }

        public object GetSuccessor(
            object node
        ) {
            if (node != null) {
                var treeNode = (BinarySearchTreeNode<T>) node;
                if (treeNode.right != null) {
                    treeNode = _FindMinNode(treeNode.right);
                } else {
                    // traverse up the tree as long as the current node is the right child of parent
                    // if we find that current node is the left child of parent, parent becomes successor
                    while (treeNode.parent != null && treeNode.parent.right == treeNode) {
                        treeNode = treeNode.parent;
                    }
                    if (treeNode.parent != null && treeNode.parent.left == treeNode) {
                        return (object) treeNode.parent;
                    }
                    return null;
                }
                return (object) treeNode;
            } else {
                return null;
            }
        }

        protected virtual BinarySearchTreeNode<T> _Search(
            BinarySearchTreeNode<T> node,
            T data,
            IComparer comparer = null
        ) {
            if (comparer == null) comparer = this.comparer;
            while (node != null) {
                var compareResult = comparer.Compare(data, node.data);
                if (compareResult < 0) {
                    node = node.left;
                } else if (compareResult > 0){
                    node = node.right;
                } else {
                    break;
                }
            }
            return node;
        }

        public virtual object GetNode(
            T data,
            IComparer comparer = null
        ) {
            return (object) _Search(root, data, comparer);
        }


        public delegate void ProcessNodeDelegate(object node, object data);

        void _WalkInOrder(
            BinarySearchTreeNode<T> node,
            ProcessNodeDelegate processFunction,
            object data
        ) {
            if (node.left != null) {
                _WalkInOrder(node.left, processFunction, data);
            }
            processFunction((object) node, data);
            if (node.right != null) {
                _WalkInOrder(node.right, processFunction, data);
            }
        }

        public void WalkInOrder(
            ProcessNodeDelegate processFunction,
            object data
        ) {
            if (root != null) {
                _WalkInOrder(root, processFunction, data);
            }
        }

    }
}
