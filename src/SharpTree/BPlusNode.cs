using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
[assembly: InternalsVisibleTo("tests")]

namespace SharpTree.BPlusTree
{
    public abstract class Node<C> where C : IComparable
    {
        /// <value>A tree where the node belongs.</value>
        protected BPlusTree<C> tree;

        /// <value>An index referencing to next position where new node is inserted.</value>
        internal protected int idx;

        /// <summary>
        /// Initializes the node.
        /// </summary>
        /// <param name="tree">A tree where the node belongs.</param>
        public Node(BPlusTree<C> tree)
        {
            this.tree = tree;
            this.idx = 0;
            this.Count = 0;
        }

        /// <summary>
        /// Gets the number of elements contained in the tree.<T>
        /// </summary>
        /// <return>The number of elements contained in the tree.</return>
        internal protected virtual int Count
        {
            get; protected set;
        }

        /// <summary>
        /// Gets the minimum item among the all items under the node.<T>
        /// </summary>
        /// <return>The minimum item under the node.</return>
        internal abstract C Min { get; }

        /// <summary>
        /// Adds an item to the node.
        /// </summary>
        /// <param name="item">The object to add to the node.<param>
        /// <return>The node split from the orginal node, or null if split does not happen.</return>
        internal abstract Node<C>? Add(C item);

        /// <summary>
        /// Returns true if the tree contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the tree.</param>
        /// <return>true if item is found in the tree; otherwise, false.</return>
        internal abstract bool Contains(C item);

        /// Removes the first occurrence of a specific object from the tree.
        /// </summary>
        /// <param name="value">The object to remove from the tree.</param>
        /// <return>true if item was successfully removed from the tree; otherwise, false. Also returns false if item is not found.</return>
        public abstract bool Remove(C item);

        /// <summary>
        /// Returns a string that represents the node.
        /// </summary>
        /// <return>A string that represents the node.</return>
        public override string ToString()
        {
            var sb = new StringBuilder();
            this.KeyToString(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Verify the node.
        /// </summary>
        /// <param name="isRoot">Suppose that the node is a root node if it is true.</param>
        /// <return>True if the node satisfy conditions of nodes; otherwise, false.</return>
        internal abstract bool Verify(bool isRoot);

        internal abstract void Append(Node<C> node);

        internal abstract void FetchLeftMostFrom(Node<C> node);

        internal abstract void FetchRightMostFrom(Node<C> node);

        internal abstract void KeyToString(StringBuilder sb);
    }

    public class LeafNode<C> : Node<C> where C : IComparable
    {
        /// <value>A collection containing the keys of the node.</value>
        internal protected C[] keys;

        /// <summary>
        /// Initializes the leaf node.
        /// </summary>
        /// <param name="tree">A tree where the node belongs.</param>
        public LeafNode(BPlusTree<C> tree) : base(tree)
        {
            this.keys = new C[base.tree.Order + 2]; // with room for shifting
        }

        /// <summary>
        /// Initializes the leaf node with the given keys.
        /// </summary>
        /// <param name="tree">A tree where the node belongs.</param>
        /// <param name="keys">An array of keys.</param>
        internal LeafNode(BPlusTree<C> tree, C[] keys) : this(tree)
        {
            Array.Copy(keys, this.keys, keys.Length);
            base.idx = keys.Length;
        }

        /// <summary>
        /// Gets the number of elements contained in the tree<T>.
        /// </summary>
        /// <return>The number of elements contained in the tree.</return>
        protected internal override int Count
        {
            get => this.idx;
        }

        internal override C Min
        {
            get
            {
                return this.keys[0];
            }
        }

        /// <summary>
        /// Adds an item to the node.
        /// </summary>
        /// <param name="item">The object to add to the node.<param>
        /// <return>The node split from the orginal node, or null if split does not happen.</return>
        internal override Node<C>? Add(C item)
        {
            var index = Array.BinarySearch(this.keys, 0, base.idx, item);
            if (index < 0)
            {
                index = ~index;
            }
            else // means the key is already there at the index
            {
                return null;
            }

            Array.Copy(this.keys, index, this.keys, index + 1, base.idx - index);
            this.keys[index] = item;
            ++base.idx;

            if (base.idx < base.tree.Order)
            {
                return null;
            }

            var rightLeaf = new LeafNode<C>(base.tree);
            Array.Copy(this.keys, base.tree.Order - base.tree.minc, rightLeaf.keys, 0, base.tree.minc);
            rightLeaf.idx = base.tree.minc;

            base.idx = base.tree.Order - base.tree.minc;

            return rightLeaf;
        }

        /// <summary>
        /// Returns true if the tree contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the tree.</param>
        /// <return>true if item is found in the tree; otherwise, false.</return>
        internal override bool Contains(C item)
        {
            var index = Array.BinarySearch(this.keys, 0, this.idx, item);
            return 0 <= index;
        }

        /// Removes the first occurrence of a specific object from the tree.
        /// </summary>
        /// <param name="value">The object to remove from the tree.</param>
        /// <return>true if item was successfully removed from the tree; otherwise, false. Also returns false if item is not found.</return>
        public override bool Remove(C item)
        {
            var index = Array.BinarySearch(this.keys, 0, base.idx, item);
            if (index < 0)
            {
                return false;
            }

            Array.Copy(this.keys, index + 1, this.keys, index, base.idx - index);
            --base.idx;
            return true;
        }

        /// <summary>
        /// Verify the node.
        /// </summary>
        /// <param name="asRoot">Suppose that the node is a root node if it is true.</param>
        /// <return>True if the node satisfy conditions of nodes; otherwise, false.</return>
        internal override bool Verify(bool asRoot)
        {
            if (asRoot && base.idx < 0) { return false; }
            if (!asRoot && base.idx + 1 < base.tree.minc) { return false; }
            if (base.tree.Order <= base.idx) { return false; }
            for (var i = 0; i < base.idx - 1; ++i)
            {
                if (this.keys[i + 1].CompareTo(this.keys[i]) <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        internal override void Append(Node<C> node)
        {
            if (node is BranchNode<C>)
            {
                throw new NotSupportedException();
            }

            var lnode = (LeafNode<C>)node;
            Array.Copy(lnode.keys, 0, this.keys, base.idx, lnode.idx);
            base.idx += lnode.idx;
        }

        internal override void FetchLeftMostFrom(Node<C> node)
        {
            Debug.Assert(base.idx + 1 < base.tree.Order);
            Debug.Assert(node is LeafNode<C>);

            if (node.idx == 0)
            {
                return;
            }

            var fromNode = (LeafNode<C>)node;
            this.keys[base.idx] = fromNode.keys[0];
            Array.Copy(fromNode.keys, 1, fromNode.keys, 0, fromNode.idx - 1);
            base.idx += 1;
            --node.idx;
        }

        internal override void FetchRightMostFrom(Node<C> node)
        {
            Debug.Assert(base.idx + 1 < base.tree.Order);
            Debug.Assert(node is LeafNode<C>);

            if (node.idx == 0)
            {
                return;
            }
    
            var fromNode = (LeafNode<C>)node;
            Array.Copy(this.keys, 0, this.keys, 1, base.idx);
            this.keys[0] = fromNode.keys[fromNode.idx - 1];
            base.idx += 1;
            --node.idx;
        }

        internal override void KeyToString(StringBuilder sb)
        {
            sb.Append("L(");
            sb.Append(this.keys[0]);
            for (var i = 1; i < this.idx; ++i)
            {
                sb.Append(" ");
                sb.Append(this.keys[i]);
            }
            sb.Append(")");
        }
    }

    public class BranchNode<C> : Node<C> where C : IComparable
    {
        /// <value>A collection containing the child nodes.</value>
        protected internal Node<C>[] childNodes;

        protected static readonly IComparer<Node<C>> cmp = (IComparer<Node<C>>)new BranchNodeComparer();

        /// <summary>
        /// Initializes the branch node with the given nodes.
        /// </summary>
        /// <param name="tree">A tree where the node belongs.</param>
        /// <param name="nodes">An array of nodes.</param>
        internal BranchNode(BPlusTree<C> tree, Node<C>[] nodes) : base(tree)
        {
            if (nodes == null || nodes.Length == 0)
            {
                throw new ArgumentNullException("nodes");
            }

            this.childNodes = new Node<C>[base.tree.Order + 2];

            Array.Copy(nodes, 0, this.childNodes, 0, nodes.Length);
            base.idx = nodes.Length - 1;

            base.Count = this.childNodes.Take(base.idx + 1).Sum(n => n.Count);
        }

        internal override C Min
        {
            get
            {
                return this.childNodes[0].Min;
            }
        }

        /// <summary>
        /// Adds an item to the node.
        /// </summary>
        /// <param name="item">The object to add to the node.<param>
        /// <return>The node split from the orginal node, or null if split does not happen.</return>
        internal override Node<C>? Add(C item)
        {
            BranchNode<C>? rightBranch = null;

            var index = this.FindIndex(item);
            var newNode = this.childNodes[index].Add(item);
            if (newNode != null)
            {
                Array.Copy(this.childNodes, index + 1, this.childNodes, index + 2, base.idx - index);
                this.childNodes[index + 1] = newNode;
                ++base.idx;

                if (this.tree.Order <= base.idx)
                {
                    var childNodes = new Node<C>[this.tree.minc];
                    Array.Copy(this.childNodes, base.idx - base.tree.minc + 1, childNodes, 0, childNodes.Length);
                    rightBranch = new BranchNode<C>(this.tree, childNodes);

                    base.idx = this.tree.minc - 1;
                }
            }

            base.Count = this.childNodes.Take(base.idx + 1).Sum(n => n.Count);

            return rightBranch;
        }

        /// <summary>
        /// Returns true if the tree contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the tree.</param>
        /// <return>true if item is found in the tree; otherwise, false.</return>
        internal override bool Contains(C item)
        {
            return this.childNodes[this.FindIndex(item)].Contains(item);
        }

        /// Removes the first occurrence of a specific object from the tree.
        /// </summary>
        /// <param name="value">The object to remove from the tree.</param>
        /// <return>true if item was successfully removed from the tree; otherwise, false. Also returns false if item is not found.</return>
        public override bool Remove(C item)
        {
            var index = this.FindIndex(item);
            var removed = this.childNodes[index].Remove(item);
            if (!removed)
            {
                return false;
            }

            if (base.tree.minc - 1 <= this.childNodes[index].idx)
            {
                return true;
            }

            if (0 < index && base.tree.minc - 1 < this.childNodes[index - 1].idx)
            {
                this.childNodes[index].FetchRightMostFrom(this.childNodes[index - 1]);
                return true;
            }

            if (index < base.idx && base.tree.minc - 1 < this.childNodes[index + 1].idx)
            {
                this.childNodes[index].FetchLeftMostFrom(this.childNodes[index + 1]);
                return true;
            }

            if (0 < index)
            {
                this.childNodes[index - 1].Append(this.childNodes[index]);
                Array.Copy(this.childNodes, index + 1, this.childNodes, index, base.idx - index);
                --base.idx;
                return true;
            }

            if (index < base.idx)
            {
                this.childNodes[index].Append(this.childNodes[index + 1]);
                Array.Copy(this.childNodes, index + 2, this.childNodes, index + 1, base.idx - index - 1);
                --base.idx;
                return true;
            }

            Debug.Assert(index == 0);
            Debug.Assert(index == base.idx);
            Debug.Assert(base.idx == 0);
            return true;
        }

        /// <summary>
        /// Very the node.
        /// </summary>
        /// <param name="isRoot">Suppose that the node is a root node if it is true.</param>
        /// <return>True if the node satisfy conditions of nodes; otherwise, false.</return>
        internal override bool Verify(bool isRoot)
        {
            if (isRoot && base.idx < 0) { return false; }
            if (!isRoot && base.idx + 1 < base.tree.minc) { return false; }
            if (base.tree.Order <= base.idx) { return false; }
            for (var i = 0; i < base.idx; ++i)
            {
                if (!this.childNodes[i].Verify(false)) { return false; }
                if (this.childNodes[i + 1].Min.CompareTo(this.childNodes[i].Min) < 0)
                {
                    return false;
                }
            }
            if (!this.childNodes[base.idx].Verify(false)) { return false; }
            return true;
        }

        internal override void Append(Node<C> node)
        {
            var bnode = (BranchNode<C>)node;
            Array.Copy(bnode.childNodes, 0, this.childNodes, base.idx + 1, bnode.idx + 1);
            base.idx += bnode.idx + 1;
        }

        internal override void FetchLeftMostFrom(Node<C> node)
        {
            Debug.Assert(base.idx < base.tree.Order);
            Debug.Assert(node is BranchNode<C>);

            if (node.idx == 0)
            {
                return;
            }

            var fromNode = (BranchNode<C>)node;
            this.childNodes[base.idx + 1] = fromNode.childNodes[0];
            Array.Copy(fromNode.childNodes, 1, fromNode.childNodes, 0, fromNode.idx);
            base.idx += 1;
            --node.idx;
        }

        internal override void FetchRightMostFrom(Node<C> node)
        {
            Debug.Assert(base.idx + 1 < base.tree.Order);
            Debug.Assert(node is BranchNode<C>);

            if (node.idx == 0)
            {
                return;
            }

            var fromNode = (BranchNode<C>)node;
            Array.Copy(this.childNodes, 0, this.childNodes, 1, base.idx);
            this.childNodes[0] = fromNode.childNodes[fromNode.idx];
            base.idx += 1;
            --node.idx;
        }

        protected int FindIndex(C item)
        {
            var searchKey = new LeafNode<C>(base.tree, new C[] { item });
            var index = Array.BinarySearch<Node<C>>(this.childNodes, 1, base.idx, searchKey, cmp);
            if (index < 0)
            {
                index = ~index - 1;
            }
            Debug.Assert(0 <= index);
            Debug.Assert(index < this.childNodes.Length);
            return index;
        }

        internal override void KeyToString(StringBuilder sb)
        {
            sb.Append("B(");
            for (var i = 1; i <= this.idx; ++i)
            {
                sb.Append(" ");
                sb.Append(this.childNodes[i].ToString());
            }
            sb.Append(")");
        }

        public class BranchNodeComparer : IComparer<Node<C>>
        {
            public int Compare(Node<C>? x, Node<C>? y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        return 0;
                    }
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                return x.Min.CompareTo(y.Min);
            }
        }
    }

    public class NodeEnumerator<C> : IEnumerator<C> where C : IComparable
    {
        protected Node<C> initNode;

        protected Stack<Node<C>> nodeStack;

        protected int sidx;

        /// <summary>
        /// Initializes the enumerator.
        /// </summary>
        /// <param name="node">The enumerator will enumerate all nodes under the node.</param>
        public NodeEnumerator(Node<C> node)
        {
            this.initNode = node;
            this.Reset();
        }

        /// <summary>
        /// Gets the element in the tree at the current position of the enumerator.
        /// <summary>
        /// <return>The element in the tree at the current position of the enumerator.</return>
        public C Current
        {
            get
            {
                var lnode = (LeafNode<C>)this.nodeStack.Peek();
                return lnode.keys[this.sidx];
            }
        }

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the tree.
        /// </summary>
        /// <return>true if the enumerator was successfully advanced to the next element;
        /// false if the enumerator has passed the end of the tree.</return>
        public bool MoveNext()
        {
            while (true)
            {
                if (this.nodeStack.Count == 0)
                {
                    return false;
                }

                if (this.nodeStack.Peek() is LeafNode<C>)
                {
                    var lnode = (LeafNode<C>)this.nodeStack.Peek();
                    ++this.sidx;
                    if (this.sidx < lnode.idx)
                    {
                        return true;
                    }

                    this.nodeStack.Pop();
                    if (this.nodeStack.Count == 0)
                    {
                        return false;
                    }
                    this.sidx = -1;
                    continue;
                }

                var bnode = (BranchNode<C>)this.nodeStack.Pop();
                for (var i = bnode.idx; 0 <= i; --i)
                {
                    this.nodeStack.Push(bnode.childNodes[i]);
                }
            }
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the tree.
        /// </summary>
        [MemberNotNull(nameof(nodeStack))]
        public void Reset()
        {
            this.nodeStack = new Stack<Node<C>>(new Node<C>[] { this.initNode });
            this.sidx = -1;
        }
    }
}
