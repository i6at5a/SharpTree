using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

using SharpTree.BPlusTree;

namespace tests;

public class BPlusNodeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Constructor_OfLeafNode()
    {
        var node = new LeafNode<int>(new TestTree<int>(5), new int[] { 1, 2 });

        Assert.That(node.idx, Is.EqualTo(2));
        Assert.That(node.keys, Is.EqualTo(new int[] { 1, 2, 0, 0, 0, 0 }));
    }

    [Test]
    public void Constructor_OfBranchNode()
    {
        var tree = new TestTree<int>(3);
        var node = new BranchNode<int>(tree, new Node<int>[] {
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 1, }),
                new LeafNode<int>(tree, new int[] { 2, 3 }),
            }),
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 11, 12 }),
                new LeafNode<int>(tree, new int[] { 13 }),
            })
        });
        Assert.That(node.Verify(true), Is.True);

        var bnode1 = (BranchNode<int>)node.childNodes[0];
        var bnode2 = (BranchNode<int>)node.childNodes[1];
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys, Is.EqualTo(new int[] { 1, 0, 0, 0 }));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys, Is.EqualTo(new int[] { 2, 3, 0, 0 }));
        Assert.That(bnode1.idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).keys, Is.EqualTo(new int[] { 11, 12, 0, 0 }));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).keys, Is.EqualTo(new int[] { 13, 0, 0, 0 }));
        Assert.That(bnode2.idx, Is.EqualTo(1));
        Assert.That(node.idx, Is.EqualTo(1));
    }

    [Test]
    public void Count_LeafNode()
    {
        var node = new LeafNode<int>(new TestTree<int>(10), new int[] {
            0, 1, 2, 3
        });
        Assert.That(node.Count, Is.EqualTo(4));
    }

    [Test]
    public void Count_BranchNode()
    {
        var tree = new TestTree<int>(4);
        var node = new BranchNode<int>(tree, new Node<int>[] {
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 1, 2 }),
                new LeafNode<int>(tree, new int[] { 3, 4, 5 }),
                new LeafNode<int>(tree, new int[] { 6, 7 })
            }),
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 11, 12 }),
                new LeafNode<int>(tree, new int[] { 13, 14, 15 }),
                new LeafNode<int>(tree, new int[] { 16, 17 }),
                new LeafNode<int>(tree, new int[] { 18, 19 })
            }),
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 21, 22 }),
                new LeafNode<int>(tree, new int[] { 23, 24, 25 }),
                new LeafNode<int>(tree, new int[] { 26, 27 })
            })
        });
        Assert.That(node.Verify(true), Is.True);
        Assert.That(node.Count, Is.EqualTo(23));
    }

    [Test]
    public void Add_ToLeafNodeOfOddOrder()
    {
        var treeOdd = new TestTree<int>(3);

        var lnode1 = new LeafNode<int>(treeOdd);
        Assert.That(lnode1.Add(3), Is.Null);
        Assert.That(lnode1.keys[0], Is.EqualTo(3));
        Assert.That(lnode1.idx, Is.EqualTo(1));

        Assert.That(lnode1.Add(1), Is.Null);
        Assert.That(lnode1.keys[0], Is.EqualTo(1));
        Assert.That(lnode1.keys[1], Is.EqualTo(3));
        Assert.That(lnode1.idx, Is.EqualTo(2));

        var lnode2 = (LeafNode<int>?)lnode1.Add(4);
        Assert.That(lnode2, Is.Not.Null);
        Debug.Assert(lnode2 != null);
        Assert.That(lnode1.keys[0], Is.EqualTo(1));
        Assert.That(lnode1.idx, Is.EqualTo(1));
        Assert.That(lnode2.keys[0], Is.EqualTo(3));
        Assert.That(lnode2.keys[1], Is.EqualTo(4));
        Assert.That(lnode2.idx, Is.EqualTo(2));

        Assert.That(lnode1.Add(2), Is.Null);
        Assert.That(lnode1.keys[0], Is.EqualTo(1));
        Assert.That(lnode1.keys[1], Is.EqualTo(2));
        Assert.That(lnode1.idx, Is.EqualTo(2));
        Assert.That(lnode2.keys[0], Is.EqualTo(3));
        Assert.That(lnode2.keys[1], Is.EqualTo(4));
        Assert.That(lnode2.idx, Is.EqualTo(2));
    }

    [Test]
    public void Add_ToLeafNodeOfEvenOrder()
    {
        var tree = new TestTree<int>(4);
        var lnode1 = new LeafNode<int>(tree);
        Assert.That(lnode1.Add(6), Is.Null);
        Assert.That(lnode1.keys[0], Is.EqualTo(6));
        Assert.That(lnode1.idx, Is.EqualTo(1));

        Assert.That(lnode1.Add(1), Is.Null);
        Assert.That(lnode1.keys[0], Is.EqualTo(1));
        Assert.That(lnode1.keys[1], Is.EqualTo(6));
        Assert.That(lnode1.idx, Is.EqualTo(2));

        Assert.That(lnode1.Add(5), Is.Null);
        Assert.That(lnode1.keys[0], Is.EqualTo(1));
        Assert.That(lnode1.keys[1], Is.EqualTo(5));
        Assert.That(lnode1.keys[2], Is.EqualTo(6));
        Assert.That(lnode1.idx, Is.EqualTo(3));

        var lnode2 = (LeafNode<int>?)lnode1.Add(2);
        Assert.That(lnode2, Is.Not.Null);
        Debug.Assert(lnode2 != null);
        Assert.That(lnode1.keys[0], Is.EqualTo(1));
        Assert.That(lnode1.keys[1], Is.EqualTo(2));
        Assert.That(lnode1.idx, Is.EqualTo(2));

        Assert.That(lnode2.keys[0], Is.EqualTo(5));
        Assert.That(lnode2.keys[1], Is.EqualTo(6));
        Assert.That(lnode2.idx, Is.EqualTo(2));

        Assert.That(lnode1.Add(3), Is.Null);
        Assert.That(lnode1.keys[0], Is.EqualTo(1));
        Assert.That(lnode1.keys[1], Is.EqualTo(2));
        Assert.That(lnode1.keys[2], Is.EqualTo(3));
        Assert.That(lnode1.idx, Is.EqualTo(3));
        Assert.That(lnode2.keys[0], Is.EqualTo(5));
        Assert.That(lnode2.keys[1], Is.EqualTo(6));
        Assert.That(lnode2.idx, Is.EqualTo(2));

        var lnode3 = (LeafNode<int>?)lnode1.Add(4);
        Assert.That(lnode3, Is.Not.Null);
        Debug.Assert(lnode3 != null);
        Assert.That(lnode1.keys[0], Is.EqualTo(1));
        Assert.That(lnode1.keys[1], Is.EqualTo(2));
        Assert.That(lnode1.idx, Is.EqualTo(2));
        Assert.That(lnode3.keys[0], Is.EqualTo(3));
        Assert.That(lnode3.keys[1], Is.EqualTo(4));
        Assert.That(lnode3.idx, Is.EqualTo(2));
        Assert.That(lnode2.keys[0], Is.EqualTo(5));
        Assert.That(lnode2.keys[1], Is.EqualTo(6));
        Assert.That(lnode2.idx, Is.EqualTo(2));
    }

    [Test]
    public void Add_ToBranchNode()
    {
        var tree = new TestTree<int>(3);
        var bnode1 = new BranchNode<int>(tree, new Node<int>[] {
            new LeafNode<int>(tree, new int[] { 11, 13 }),
            new LeafNode<int>(tree, new int[] { 21, 25 })
        });

        Assert.That(bnode1.Add(15), Is.Null);
        Assert.That(bnode1.Add(12), Is.Null);

        var bnode3 = (BranchNode<int>?)bnode1.Add(26);
        Assert.That(bnode3, Is.Not.Null);
        Debug.Assert(bnode3 != null);
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(11));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[1], Is.EqualTo(12));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(13));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[1], Is.EqualTo(15));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(2));
        Assert.That(bnode1.idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).keys[0], Is.EqualTo(21));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).keys[0], Is.EqualTo(25));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).keys[1], Is.EqualTo(26));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).idx, Is.EqualTo(2));
        Assert.That(bnode3.idx, Is.EqualTo(1));

        Assert.That(bnode1.Add(1), Is.Null);
        Assert.That(bnode1.Add(3), Is.Null);

        var bnode2 = (BranchNode<int>?)bnode1.Add(5);
        Assert.That(bnode2, Is.Not.Null);
        Debug.Assert(bnode2 != null);
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(3));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[1], Is.EqualTo(5));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(2));
        Assert.That(bnode1.idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).keys[0], Is.EqualTo(11));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).keys[1], Is.EqualTo(12));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).keys[0], Is.EqualTo(13));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).keys[1], Is.EqualTo(15));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).idx, Is.EqualTo(2));
        Assert.That(bnode2.idx, Is.EqualTo(1));

        Assert.That(bnode2.Add(16), Is.Null);
        Assert.That(bnode1.Add(6), Is.Null);
        Assert.That(bnode1.Add(2), Is.Null);
        Assert.That(bnode1.Add(4), Is.Null);
        Assert.That(bnode3.Add(24), Is.Null);
        Assert.That(bnode3.Add(23), Is.Null);
        Assert.That(bnode3.Add(22), Is.Null);
        Assert.That(bnode2.Add(14), Is.Null);

        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[1], Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(3));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[1], Is.EqualTo(4));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).keys[0], Is.EqualTo(5));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).keys[1], Is.EqualTo(6));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).idx, Is.EqualTo(2));
        Assert.That(bnode1.idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).keys[0], Is.EqualTo(11));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).keys[1], Is.EqualTo(12));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).keys[0], Is.EqualTo(13));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).keys[1], Is.EqualTo(14));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode2.childNodes[2]).keys[0], Is.EqualTo(15));
        Assert.That(((LeafNode<int>)bnode2.childNodes[2]).keys[1], Is.EqualTo(16));
        Assert.That(((LeafNode<int>)bnode2.childNodes[2]).idx, Is.EqualTo(2));
        Assert.That(bnode2.idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).keys[0], Is.EqualTo(21));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).keys[1], Is.EqualTo(22));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).keys[0], Is.EqualTo(23));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).keys[1], Is.EqualTo(24));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode3.childNodes[2]).keys[0], Is.EqualTo(25));
        Assert.That(((LeafNode<int>)bnode3.childNodes[2]).keys[1], Is.EqualTo(26));
        Assert.That(((LeafNode<int>)bnode3.childNodes[2]).idx, Is.EqualTo(2));
        Assert.That(bnode3.idx, Is.EqualTo(2));
    }

    [Test]
    public void Remove_FromLeafNode()
    {
        var node = new LeafNode<int>(new TestTree<int>(3), new int[] { 1, 2, 3 });

        Assert.That(node.Remove(0), Is.False);
        Assert.That(node.keys[0], Is.EqualTo(1));
        Assert.That(node.keys[1], Is.EqualTo(2));
        Assert.That(node.keys[2], Is.EqualTo(3));
        Assert.That(node.idx, Is.EqualTo(3));

        Assert.That(node.Remove(2), Is.True);
        Assert.That(node.keys[0], Is.EqualTo(1));
        Assert.That(node.keys[1], Is.EqualTo(3));
        Assert.That(node.idx, Is.EqualTo(2));

        Assert.That(node.Remove(1), Is.True);
        Assert.That(node.keys[0], Is.EqualTo(3));
        Assert.That(node.idx, Is.EqualTo(1));

        Assert.That(node.Remove(3), Is.True);
        Assert.That(node.idx, Is.EqualTo(0));
    }

    [Test]
    public void Remove_FromBranchNode()
    {
        var tree = new TestTree<int>(4);
        var bnode1 = new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 1 }),
                new LeafNode<int>(tree, new int[] { 2 }),
                new LeafNode<int>(tree, new int[] { 3 }),
                new LeafNode<int>(tree, new int[] { 4, 5 }),
            });
        var bnode2 = new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 12, 13 }),
                new LeafNode<int>(tree, new int[] { 14 })
            });
        var bnode3 = new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 21 }),
                new LeafNode<int>(tree, new int[] { 22, 23 }),
            });
        var bnode4 = new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 31 }),
                new LeafNode<int>(tree, new int[] { 32 }),
                new LeafNode<int>(tree, new int[] { 33 }),
            });
        var node = new BranchNode<int>(tree, new Node<int>[] { bnode1, bnode2, bnode3, bnode4 });
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(0), Is.False); // remove nothing

        Assert.That(node.Remove(5), Is.True); // remove simply
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).keys[0], Is.EqualTo(3));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[3]).keys[0], Is.EqualTo(4));
        Assert.That(((LeafNode<int>)bnode1.childNodes[3]).idx, Is.EqualTo(1));
        Assert.That(bnode1.idx, Is.EqualTo(3));
        Assert.That(node.idx, Is.EqualTo(3));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(14), Is.True);  // borrow from left leaf node
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).keys[0], Is.EqualTo(12));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).keys[0], Is.EqualTo(13));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode2.idx, Is.EqualTo(1));
        Assert.That(node.idx, Is.EqualTo(3));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(21), Is.True); // borrow from right leaf node
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).keys[0], Is.EqualTo(22));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).keys[0], Is.EqualTo(23));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode3.idx, Is.EqualTo(1));
        Assert.That(node.idx, Is.EqualTo(3));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(3), Is.True); // merge with left leaf node
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).keys[0], Is.EqualTo(4));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).idx, Is.EqualTo(1));
        Assert.That(bnode1.idx, Is.EqualTo(2));
        Assert.That(node.idx, Is.EqualTo(3));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(22), Is.True); // borrow from right branch node
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).keys[0], Is.EqualTo(23));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).keys[0], Is.EqualTo(31));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode3.idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode4.childNodes[0]).keys[0], Is.EqualTo(32));
        Assert.That(((LeafNode<int>)bnode4.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode4.childNodes[1]).keys[0], Is.EqualTo(33));
        Assert.That(((LeafNode<int>)bnode4.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode4.idx, Is.EqualTo(1));
        Assert.That(node.idx, Is.EqualTo(3));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(12), Is.True); // borrow from left branch node
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode1.idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).keys[0], Is.EqualTo(4));
        Assert.That(((LeafNode<int>)bnode2.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).keys[0], Is.EqualTo(13));
        Assert.That(((LeafNode<int>)bnode2.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode2.idx, Is.EqualTo(1));
        Assert.That(node.idx, Is.EqualTo(3));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(32), Is.True); // merge with left branch node
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).keys[0], Is.EqualTo(23));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).keys[0], Is.EqualTo(31));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode3.childNodes[2]).keys[0], Is.EqualTo(33));
        Assert.That(((LeafNode<int>)bnode3.childNodes[2]).idx, Is.EqualTo(1));
        Assert.That(bnode3.idx, Is.EqualTo(2));
        Assert.That(node.idx, Is.EqualTo(2));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(23), Is.True); // merge with right leaf node
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).keys[0], Is.EqualTo(31));
        Assert.That(((LeafNode<int>)bnode3.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).keys[0], Is.EqualTo(33));
        Assert.That(((LeafNode<int>)bnode3.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode3.idx, Is.EqualTo(1));
        Assert.That(node.idx, Is.EqualTo(2));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(4), Is.True); // merge with right branch node
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).keys[0], Is.EqualTo(13));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).idx, Is.EqualTo(1));
        Assert.That(bnode1.idx, Is.EqualTo(2));
        Assert.That(node.idx, Is.EqualTo(1));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(13), Is.True);
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode1.idx, Is.EqualTo(1));
        Assert.That(node.idx, Is.EqualTo(1));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(31), Is.True);
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).keys[0], Is.EqualTo(33));
        Assert.That(((LeafNode<int>)bnode1.childNodes[2]).idx, Is.EqualTo(1));
        Assert.That(bnode1.idx, Is.EqualTo(2));
        Assert.That(node.idx, Is.EqualTo(0));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(2), Is.True);
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).keys[0], Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).keys[0], Is.EqualTo(33));
        Assert.That(((LeafNode<int>)bnode1.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode1.idx, Is.EqualTo(1));
        Assert.That(node.idx, Is.EqualTo(0));
        Assert.That(node.Verify(true), Is.True);

        Assert.That(node.Remove(33), Is.True);
        Assert.That(bnode1.idx, Is.EqualTo(0));
        Assert.That(node.Remove(1), Is.True);
        Assert.That(node.idx, Is.EqualTo(0));
    }

    [Test]
    public void AddAndRemove_Nodes()
    {
        var tree = new TestTree<int>(3);
        tree.Add(1);
        tree.Add(9);
        tree.Add(2);
        tree.Add(8);
        tree.Add(3);
        tree.Add(7);
        tree.Add(4);
        tree.Add(6);
        tree.Add(5);
        for (var i = 1; i < 10; ++i)
        {
            Assert.That(tree.Contains(1), Is.True);
        }

        Assert.That(tree.Remove(4), Is.True);
        Assert.That(tree.Remove(5), Is.True);
        Assert.That(tree.Remove(6), Is.True);

        Assert.That(tree.RootNode.Verify(true), Is.True);
        Assert.That(tree.Contains(1), Is.True);
        Assert.That(tree.Contains(2), Is.True);
        Assert.That(tree.Contains(3), Is.True);
        Assert.That(tree.Contains(4), Is.False);
        Assert.That(tree.Contains(5), Is.False);
        Assert.That(tree.Contains(6), Is.False);
        Assert.That(tree.Contains(7), Is.True);
        Assert.That(tree.Contains(8), Is.True);
        Assert.That(tree.Contains(9), Is.True);

        tree.Add(4);
        tree.Add(6);
        tree.Add(5);

        Assert.That(tree.RootNode.Verify(true), Is.True);
        for (var i = 1; i < 10; ++i)
        {
            Assert.That(tree.Contains(1), Is.True);
        }

        Assert.That(tree.Remove(1), Is.True);
        Assert.That(tree.Remove(2), Is.True);
        Assert.That(tree.Remove(3), Is.True);
        Assert.That(tree.Remove(4), Is.True);

        Assert.That(tree.RootNode.Verify(true), Is.True);
        Assert.That(tree.Contains(1), Is.False);
        Assert.That(tree.Contains(2), Is.False);
        Assert.That(tree.Contains(3), Is.False);
        Assert.That(tree.Contains(4), Is.False);
        Assert.That(tree.Contains(5), Is.True);
        Assert.That(tree.Contains(6), Is.True);
        Assert.That(tree.Contains(7), Is.True);
        Assert.That(tree.Contains(8), Is.True);
        Assert.That(tree.Contains(9), Is.True);

        tree.Add(1);
        tree.Add(2);
        tree.Add(3);
        tree.Add(4);

        Assert.That(tree.RootNode.Verify(true), Is.True);
        for (var i = 1; i < 10; ++i)
        {
            Assert.That(tree.Contains(1), Is.True);
        }

        Assert.That(tree.Remove(6), Is.True);
        Assert.That(tree.Remove(7), Is.True);
        Assert.That(tree.Remove(8), Is.True);
        Assert.That(tree.Remove(9), Is.True);

        Assert.That(tree.RootNode.Verify(true), Is.True);
        Assert.That(tree.Contains(1), Is.True);
        Assert.That(tree.Contains(2), Is.True);
        Assert.That(tree.Contains(3), Is.True);
        Assert.That(tree.Contains(4), Is.True);
        Assert.That(tree.Contains(5), Is.True);
        Assert.That(tree.Contains(6), Is.False);
        Assert.That(tree.Contains(7), Is.False);
        Assert.That(tree.Contains(8), Is.False);
        Assert.That(tree.Contains(9), Is.False);

        tree.Add(6);
        tree.Add(7);
        tree.Add(8);
        tree.Add(9);

        Assert.That(tree.RootNode.Verify(true), Is.True);
        for (var i = 1; i < 10; ++i)
        {
            Assert.That(tree.Contains(1), Is.True);
        }
    }
}

public class BPlusTreeTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Constructor_OrderShouldBeMoreThan1()
    {
        Assert.That(() =>
        {
            new BPlusTree<int>(1);
        }, Throws.Exception.TypeOf<ArgumentOutOfRangeException>()
            .And.Property("ParamName").EqualTo("order"));

        Assert.That(() =>
        {
            new BPlusTree<int>(2);
        }, Throws.Nothing);
    }

    [Test]
    public void Constructor_InitialParameters()
    {
        var treeOdd = new TestTree<int>(3);
        Assert.That(treeOdd.Order, Is.EqualTo(3));
        Assert.That(treeOdd.minc, Is.EqualTo(2));
        Assert.That(treeOdd.Count, Is.EqualTo(0));

        var treeEven = new TestTree<int>(6);
        Assert.That(treeEven.Order, Is.EqualTo(6));
        Assert.That(treeEven.minc, Is.EqualTo(3));
        Assert.That(treeEven.Count, Is.EqualTo(0));
    }

    [Test]
    public void Add_ToTree()
    {
        var tree = new TestTree<int>(3);

        tree.Add(55);
        tree.Add(47);
        tree.Add(39);
        tree.Add(44); // overflow
        tree.Add(48);
        tree.Add(45);
        tree.Add(35); // overflow
        tree.Add(54); // overflow
        tree.Add(31);
        tree.Add(32); // overflow
        tree.Add(36);
        tree.Add(37); // overflow
        tree.Add(30);
        tree.Add(33); // overflow

        var root = (BranchNode<int>)tree.RootNode;
        var bnode1 = (BranchNode<int>)root.childNodes[0];
        var bnode11 = (BranchNode<int>)bnode1.childNodes[0];
        Assert.That(((LeafNode<int>)bnode11.childNodes[0]).keys[0], Is.EqualTo(30));
        Assert.That(((LeafNode<int>)bnode11.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode11.childNodes[1]).keys[0], Is.EqualTo(31));
        Assert.That(((LeafNode<int>)bnode11.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode11.idx, Is.EqualTo(1));
        var bnode12 = (BranchNode<int>)bnode1.childNodes[1];
        Assert.That(((LeafNode<int>)bnode12.childNodes[0]).keys[0], Is.EqualTo(32));
        Assert.That(((LeafNode<int>)bnode12.childNodes[0]).keys[1], Is.EqualTo(33));
        Assert.That(((LeafNode<int>)bnode12.childNodes[0]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode12.childNodes[1]).keys[0], Is.EqualTo(35));
        Assert.That(((LeafNode<int>)bnode12.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(bnode12.idx, Is.EqualTo(1));
        Assert.That(bnode1.idx, Is.EqualTo(1));
        var bnode2 = (BranchNode<int>)root.childNodes[1];
        var bnode21 = (BranchNode<int>)bnode2.childNodes[0];
        Assert.That(((LeafNode<int>)bnode21.childNodes[0]).keys[0], Is.EqualTo(36));
        Assert.That(((LeafNode<int>)bnode21.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode21.childNodes[1]).keys[0], Is.EqualTo(37));
        Assert.That(((LeafNode<int>)bnode21.childNodes[1]).keys[1], Is.EqualTo(39));
        Assert.That(((LeafNode<int>)bnode21.childNodes[1]).idx, Is.EqualTo(2));
        Assert.That(((LeafNode<int>)bnode21.childNodes[2]).keys[0], Is.EqualTo(44));
        Assert.That(((LeafNode<int>)bnode21.childNodes[2]).keys[1], Is.EqualTo(45));
        Assert.That(((LeafNode<int>)bnode21.childNodes[2]).idx, Is.EqualTo(2));
        Assert.That(bnode21.idx, Is.EqualTo(2));
        var bnode22 = (BranchNode<int>)bnode2.childNodes[1];
        Assert.That(((LeafNode<int>)bnode22.childNodes[0]).keys[0], Is.EqualTo(47));
        Assert.That(((LeafNode<int>)bnode22.childNodes[0]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode22.childNodes[1]).keys[0], Is.EqualTo(48));
        Assert.That(((LeafNode<int>)bnode22.childNodes[1]).idx, Is.EqualTo(1));
        Assert.That(((LeafNode<int>)bnode22.childNodes[2]).keys[0], Is.EqualTo(54));
        Assert.That(((LeafNode<int>)bnode22.childNodes[2]).keys[1], Is.EqualTo(55));
        Assert.That(((LeafNode<int>)bnode22.childNodes[2]).idx, Is.EqualTo(2));
        Assert.That(bnode22.idx, Is.EqualTo(2));
        Assert.That(bnode2.idx, Is.EqualTo(1));
        Assert.That(root.idx, Is.EqualTo(1));
    }

    [Test]
    public void Add_InParallel()
    {
        var tree = new BPlusTree<int>(5);
        Parallel.For(0, 31, id =>
        {
            for (var i = 0; i < 15; i++)
            {
                tree.Add(i);
            }
        });

        int[] vals = new int[15];
        tree.CopyTo(vals, 0);
        Assert.That(vals, Is.EqualTo(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }));
    }

    [Test]
    public void Clear_Tree()
    {
        var tree = new TestTree<int>(3);
        for (var i = 0; i < 100; ++i)
        {
            tree.Add(i);
        }
        Assert.That(tree.RootNode.idx, Is.GreaterThan(0));
        Assert.That(tree.Count, Is.EqualTo(100));

        tree.Clear();
        Assert.That(tree.RootNode.idx, Is.EqualTo(0));
        Assert.That(tree.Count, Is.EqualTo(0));
    }

    [Test]
    public void Contains_StringItems()
    {
        var tree = new TestTree<string>(3);
        Assert.That(tree.Contains("Philadelphia 76ers"), Is.False);
        Assert.That(tree.Contains("Detroit Pistons"), Is.False);
        Assert.That(tree.Contains("Charlotte Hornets"), Is.False);
        Assert.That(tree.Contains("Houston Rockets"), Is.False);

        tree.Add("Boston Celtics");
        tree.Add("Brooklyn Nets");
        tree.Add("New York Knicks");
        tree.Add("Philadelphia 76ers"); // overflow
        tree.Add("Toronto Raptors");
        tree.Add("Chicago Bulls");
        tree.Add("Cleveland Cavaliers");
        tree.Add("Detroit Pistons");
        tree.Add("Indiana Pacers");
        tree.Add("Milwaukee Bucks");
        tree.Add("Atlanta Hawks");
        tree.Add("Charlotte Hornets");
        tree.Add("Miami Heat");
        tree.Add("Orlando Magic");
        tree.Add("Washington Wizards");

        Assert.That(tree.Contains("Philadelphia 76ers"), Is.True);
        Assert.That(tree.Contains("Detroit Pistons"), Is.True);
        Assert.That(tree.Contains("Charlotte Hornets"), Is.True);
        Assert.That(tree.Contains("Houston Rockets"), Is.False);
    }

    [Test]
    public void CopyTo_FromSimpleTree()
    {
        var tree = new TestTree<int>(10);
        var root = new LeafNode<int>(tree, new int[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        tree.RootNode = root;

        var intArray = new int[10];
        tree.CopyTo(intArray, 0);
        Assert.That(intArray, Is.EqualTo(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 0, 0 }));
        Array.Clear(intArray);
        tree.CopyTo(intArray, 3);
        Assert.That(intArray, Is.EqualTo(new int[] { 4, 5, 6, 7, 8, 0, 0, 0, 0, 0 }));
        Array.Clear(intArray);
        tree.CopyTo(intArray, 20);
        Assert.That(intArray, Is.EqualTo(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
    }

    [Test]
    public void CopyTo_FromComplexTree()
    {
        var tree = new TestTree<int>(3);
        var root = new BranchNode<int>(tree, new Node<int>[] {
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 1, 2 }),
                new LeafNode<int>(tree, new int[] { 3 })
            }),
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 11 }),
                new LeafNode<int>(tree, new int[] { 12, 13 })
            }),
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 21, 22 }),
                new LeafNode<int>(tree, new int[] { 23, 24 })
            })
        });
        tree.RootNode = root;

        Assert.That(root.Verify(true), Is.True);
        var intArray = new int[10];
        tree.CopyTo(intArray, 0);
        Assert.That(intArray, Is.EqualTo(new int[] { 1, 2, 3, 11, 12, 13, 21, 22, 23, 24 }));
        Array.Clear(intArray);

        tree.CopyTo(intArray, 5);
        Assert.That(intArray, Is.EqualTo(new int[] { 13, 21, 22, 23, 24, 0, 0, 0, 0, 0 }));
        Array.Clear(intArray);

        tree.CopyTo(intArray, 11);
        Assert.That(intArray, Is.EqualTo(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
    }

    [Test]
    public void Enumerate_EmptyTree()
    {
        var tree = new TestTree<int>(11);
        var enumerator = tree.GetEnumerator();
        Assert.That(enumerator.MoveNext(), Is.False);
    }

    [Test]
    public void Enumerate_TreeWithLeafNode()
    {
        var tree = new TestTree<int>(11);
        var node = new LeafNode<int>(tree, new int[] { 1, 2, 3, 4, 5 });
        tree.RootNode = node;

        var enumerator = tree.GetEnumerator();
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(1));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(2));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(3));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(4));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(5));
        Assert.That(enumerator.MoveNext(), Is.False);
        Assert.That(enumerator.MoveNext(), Is.False);
    }

    [Test]
    public void Enumerate_TreeWithBranchNode()
    {
        var tree = new TestTree<int>(4);
        var bnode1 = new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 1 }),
                new LeafNode<int>(tree, new int[] { 2 }),
                new LeafNode<int>(tree, new int[] { 3, 4 }),
                new LeafNode<int>(tree, new int[] { 5 }),
            });
        var bnode2 = new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 11 }),
                new LeafNode<int>(tree, new int[] { 12, 13 }),
                new LeafNode<int>(tree, new int[] { 14 })
            });
        var bnode3 = new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 21 }),
                new LeafNode<int>(tree, new int[] { 22, 23 })
            });
        var bnode4 = new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 31 }),
                new LeafNode<int>(tree, new int[] { 32 }),
            });
        tree.RootNode = new BranchNode<int>(tree, new Node<int>[] { bnode1, bnode2, bnode3, bnode4 });
        Assert.That(tree.RootNode.Verify(true), Is.True);

        var enumerator = tree.GetEnumerator();
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(1));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(2));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(3));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(4));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(5));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(11));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(12));
        enumerator.Reset();
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(1));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(2));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(3));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(4));
        Assert.That(enumerator.Current, Is.EqualTo(4));
        Assert.That(enumerator.Current, Is.EqualTo(4));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(5));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(11));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(12));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(13));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(14));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(21));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(22));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(23));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(31));
        Assert.That(enumerator.MoveNext(), Is.True);
        Assert.That(enumerator.Current, Is.EqualTo(32));
        Assert.That(enumerator.MoveNext(), Is.False);
        Assert.That(enumerator.MoveNext(), Is.False);
    }

    [Test]
    public void Remove_Values()
    {
        var tree = new TestTree<int>(3);
        var node = new BranchNode<int>(tree, new Node<int>[] {
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 1 }),
                new LeafNode<int>(tree, new int[] { 2 }),
            }),
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 3 }),
                new LeafNode<int>(tree, new int[] { 4 }),
            }),
            new BranchNode<int>(tree, new Node<int>[] {
                new LeafNode<int>(tree, new int[] { 5 }),
                new LeafNode<int>(tree, new int[] { 6 })
            }),
        });
        tree.RootNode = node;
        Assert.That(node.Verify(true), Is.True);
        Assert.That(tree.Remove(3), Is.True);
        Assert.That(tree.Remove(4), Is.True);
        Assert.That(tree.Remove(2), Is.True);
        Assert.That(tree.Remove(5), Is.True);
        Assert.That(tree.Remove(1), Is.True);
        Assert.That(tree.RootNode, Is.TypeOf<LeafNode<int>>());
        Assert.That(tree.Remove(6), Is.True);
        Assert.That(tree.RootNode.idx, Is.EqualTo(0));
    }

    [Test]
    public void Remove_InParallel()
    {
        var tree = new BPlusTree<int>(5);
        for (int i = 0; i < 100; ++i)
        {
            tree.Add(i);
        }

        Parallel.For(0, 31, id =>
        {
            for (var i = 0; i < 100; i++)
            {
                tree.Remove(i);
            }
        });

        Assert.That(tree.Count, Is.EqualTo(0));
    }
}

public class TestTree<C> : BPlusTree<C> where C : IComparable
{
    public TestTree(int order) : base(order) { }

    public Node<C> RootNode
    {
        get => base.root;
        set
        {
            this.root = value;
        }
    }
}
