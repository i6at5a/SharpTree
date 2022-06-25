using System.Collections;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("tests")]

namespace SharpTree.BPlusTree
{
    public class BPlusTree<C> : ICollection<C> where C : IComparable
    {
        /// <value>The constant value representing a minimum number of child nodes.</value>
        internal protected readonly int minc;

        /// <value>The root node of the tree.</value>
        protected Node<C> root;

        /// <summary>
        /// Initializes a new instance of the B+ tree class.
        /// </summary>
        /// <param name="order">capacity of nodes.</param>
        public BPlusTree(int order)
        {
            if (order < 2)
            {
                throw new ArgumentOutOfRangeException("order", "order should be greater than 1");
            }

            this.Order = order;
            this.minc = (int)(Math.Ceiling(this.Order / 2.0));
            this.root = new LeafNode<C>(this);
        }

        /// <value>
        /// Gets the capacity of nodes.
        /// </value>
        public int Order
        {
            get;
        }

        /// <summary>
        /// Gets the number of elements contained in the tree.<T>.
        /// </summary>
        /// <return>The number of elements contained in the tree.</return>
        public int Count => this.root.Count;

        /// <summary>
        /// Gets a value indicating whether the tree is read-only.
        /// </summary>
        /// <return>Returns always false.</return>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Adds an item to the tree.
        /// </summary>
        /// <param name="item">The object to add to the tree.</param>
        public void Add(C item)
        {
            var promoted = this.root.Add(item);
            if (promoted != null)
            {
                this.root = new BranchNode<C>(this, new Node<C>[] { this.root, promoted });
            }
        }

        /// <summary>
        /// Removes all values from the tree.
        /// </summary>
        public void Clear()
        {
            this.root = new LeafNode<C>(this);
        }

        /// <summary>
        /// Determines whether the tree contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the tree.</param>
        /// <return>true if item is found in the tree; otherwise, false.</return>
        public bool Contains(C item)
        {
            return this.root.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the tree to an Array, starting at a particular Array index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from the tree.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(C[] array, int arrayIndex)
        {
            var s = new Stack<Node<C>>();
            s.Push(this.root);
            var cur = 0;
            while (0 < s.Count)
            {
                var n = s.Pop();
                if (n is BranchNode<C>)
                {
                    var bnode = (BranchNode<C>)n;
                    for (var i = bnode.idx; 0 <= i; --i)
                    {
                        s.Push(bnode.childNodes[i]);
                    }
                    continue;
                }
                else
                {
                    var lnode = (LeafNode<C>)n;
                    if (lnode.idx < arrayIndex)
                    {
                        arrayIndex -= lnode.idx;
                        continue;
                    }

                    Array.Copy(lnode.keys, arrayIndex, array, cur, lnode.idx - arrayIndex);
                    cur += (lnode.idx - arrayIndex);
                    arrayIndex = 0;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the tree.
        /// </summary>
        /// <return>The Enumerator object that can be used to iterate through the tree.</return>
        public IEnumerator<C> GetEnumerator()
        {
            return new NodeEnumerator<C>(this.root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the tree.
        /// </summary>
        /// <param name="item">The object to remove from the tree.</param>
        /// <return>true if item was successfully removed from the tree; otherwise, false. Also returns false if item is not found.</return>
        public bool Remove(C item)
        {
            var removed = this.root.Remove(item);
            if (this.root is BranchNode<C> && this.root.idx == 0)
            {
                this.root = ((BranchNode<C>)this.root).childNodes[0];
            }
            return removed;
        }
    }
}
