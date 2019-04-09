///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Solution/Project:  BTree Project 5
//	File Name:         BTree.cs
//	Description:       This class represents the BTree, handles searching, inserting and deleting values from the
//                     BTree, as well as formating the BTree for display in PreOrder Traversal.
//	Course:            CSCI 2210 - Data Structures	
//	Author:            Andrew Haselden, andrewhaselden@gmail.com                                                
//	Created:           Friday, April 14, 2017
//	Copyright:         Andrew Haselden, 2017
//
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
namespace BTree
{
    /// <summary>
    /// This class represents the BTree, handles searching, inserting and deleting values from the
    /// BTree, as well as formating the BTree for display in PreOrder Traversal.
    /// </summary>
    class BTree
    {
        #region Properties
        private int M { get; set; }
        private int Max { get; set; }
        private int Min { get; set; }
        private Node Root { get; set; }
        private double CollectPercentFull { get; set; }
        private int IndexCount { get; set; }
        private int LeafCount { get; set; }
        private int Height { get; set; }
        public int ValueCount { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="BTree"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public BTree(int size)
        {
            Root = null;
            M = size;
            Max = M - 1;
            Min = (int)Math.Ceiling((double)M / 2) - 1;
            ValueCount = 0;
            IndexCount = 0;
            LeafCount = 0;
            Height = 0;
            CollectPercentFull = 0;
        }
        #endregion

        #region Searching
        /// <summary>
        /// Searches for the specified value x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public bool Search(int x)
        {
            if (Search(x, Root) == null)
                return false;
            return true;
        }

        /// <summary>
        /// Searches the specified value x in the Node p.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        private Node Search(int x, Node p)
        {
            //if key is not present in the tree
            if (p == null)
                return null;

            //if key x is found in Node p
            int n = 0;
            if (SearchNode(x, p, ref n) == true)
                return p;

            //Search in node p.Children[n]
            return Search(x, p.Children[n]);
        }

        /// <summary>
        /// Searches the node for the specified value x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="p">The p.</param>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        private bool SearchNode(int x, Node p, ref int n)
        {
            // if key x is less than the leftmost key
            if(x < p.Keys[1])
            {
                n = 0;
                return false;
            }

            //find the value
            n = p.NumKeys;
            while ((x < p.Keys[n]) && n > 1)
                n--;

            if (x == p.Keys[n])
                return true;
            else
                return false;
        }
        #endregion

        #region Insertion
        /// <summary>
        /// Inserts the specified value x into the BTree.
        /// </summary>
        /// <param name="x">The x.</param>
        public void Insert(int x)
        {
            int iKey = 0;
            Node iKeyRchild = null;

            bool taller = Insert(x, Root, ref iKey, ref iKeyRchild);

            //Height is increased by one, new root node has to be created
            if(taller) 
            {
                Node temp = new Node(M);
                temp.Children[0] = Root;
                Root = temp;

                Root.NumKeys = 1;
                Root.Keys[1] = iKey;
                Root.Children[1] = iKeyRchild;
                ValueCount++;
            }
        }

        /// <summary>
        /// Inserts the specified value x into Node p (recursive)
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="p">The p.</param>
        /// <param name="iKey">The i key.</param>
        /// <param name="iKeyRchild">The i key rchild.</param>
        /// <returns></returns>
        private bool Insert(int x, Node p, ref int iKey, ref Node iKeyRchild)
        {
            //first base case
            //if key not found
            if(p == null)
            {
                iKey = x;
                iKeyRchild = null;
                return true;
            }

            //second base case
            //if key is found, than do not insert a duplicate
            int n = 0;
            if (SearchNode(x, p, ref n) == true)
                return false;

            //Search recursively for an insertion point
            bool flag = Insert(x, p.Children[n], ref iKey, ref iKeyRchild);

            if(flag == true)
            {
                //if there is room in the node then insert by shifting
                if(p.NumKeys < Max)
                {
                    InsertByShift(p, n, iKey, iKeyRchild);
                    return false;  //insertion is complete
                }
                //if there is no room in the node then split node
                else
                {
                    SplitNode(p, n, ref iKey, ref iKeyRchild);
                    return true;  //insertion is not complete, median key needs to be inserted
                }
            }
            return false;
        }

        /// <summary>
        /// Inserts the value by shifting the current values of the node.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="n">The n.</param>
        /// <param name="iKey">The i key.</param>
        /// <param name="iKeyRchild">The i key rchild.</param>
        private void InsertByShift(Node p, int n, int iKey, Node iKeyRchild)
        {
            for (int i = p.NumKeys; i > n; i--)
            {
                p.Keys[i + 1] = p.Keys[i];
                p.Children[i + 1] = p.Children[i];
            }

            p.Keys[n + 1] = iKey;
            p.Children[n + 1] = iKeyRchild;
            p.NumKeys++;
            ValueCount++;
        }

        /// <summary>
        /// Splits the node.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="n">The n.</param>
        /// <param name="iKey">The i key.</param>
        /// <param name="iKeyRchild">The i key rchild.</param>
        private void SplitNode(Node p, int n, ref int iKey, ref Node iKeyRchild)
        {
            int i, j;
            int lastKey;
            Node lastChild;

            //if node is maxed
            if(n == Max)
            {
                lastKey = iKey;
                lastChild = iKeyRchild;
            }
            //if node isn't maxed but needs a split
            else
            {
                lastKey = p.Keys[Max];
                lastChild = p.Children[Max];

                for (i = p.NumKeys-1; i > n; i--)
                {
                    p.Keys[i + 1] = p.Keys[i];
                    p.Children[i + 1] = p.Children[i];
                }

                p.Keys[i + 1] = iKey;
                p.Children[i + 1] = iKeyRchild;
            }

            //create new node from the split
            int d = (M + 1) / 2;
            int medianKey = p.Keys[d];
            Node newNode = new Node(M);
            newNode.NumKeys = M - d;

            newNode.Children[0] = p.Children[d];
            for (i = 1, j = d + 1; j <= Max; i++, j++)
            {
                newNode.Keys[i] = p.Keys[j];
                newNode.Children[i] = p.Children[j];
            }
            newNode.Keys[i] = lastKey;
            newNode.Children[i] = lastChild;

            p.NumKeys = d-1;

            iKey = medianKey;
            iKeyRchild = newNode;
        }
        #endregion

        #region Deletion
        /// <summary>
        /// Deletes the specified value x from the BTree.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        public bool Delete(int x)
        {
            bool check = true;

            if (Root == null)
                return false;

            Delete(x, Root, ref check);

            //height of tree is decreased by 1
            if (Root != null && Root.NumKeys == 0)
                Root = Root.Children[0];

            //decrement the value count
            if (check)
                ValueCount--;

            return check;
        }

        /// <summary>
        /// Deletes the specified value x from Node p. (recursive)
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="p">The p.</param>
        /// <param name="check">if set to <c>true</c> [check].</param>
        private void Delete(int x, Node p, ref bool check)
        {
            int n = 0;

            //if key x is found in node p
            if (SearchNode(x, p, ref n) == true)
            {
                //if node p is a leaf node
                if (p.Type == Node.NodeType.Leaf)
                {
                    DeleteByShift(p, n);
                    return;
                }
                //if node p is a non-leaf node
                else
                {
                    Node s = p.Children[n];
                    while (s.Children[0] != null)
                        s = s.Children[0];
                    p.Keys[n] = s.Keys[1];
                    Delete(s.Keys[1], p.Children[n], ref check);
                }
            }
            //if key x not found in node p
            else
            {
                //if p is a leaf node
                if (p.Type == Node.NodeType.Leaf)
                {
                    check = false;
                    return;
                }
                //if p is a non leaf node
                else
                    Delete(x, p.Children[n], ref check);
            }

            if (p.Children[n].NumKeys < Min)
                Restore(p, n);
        }

        /// <summary>
        /// Deletes the value by shifting values of node p.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="n">The n.</param>
        private void DeleteByShift(Node p, int n)
        {
            for (int i = n+1; i <= p.NumKeys; i++)
            {
                p.Keys[i - 1] = p.Keys[i];
                p.Children[i - 1] = p.Children[i];
            }
            p.NumKeys--;
        }

        /// <summary>
        /// Called when p.Children[n] becomes underflow
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="n">The n.</param>
        private void Restore(Node p, int n)
        {
            if (n != 0 && p.Children[n - 1].NumKeys > Min)
                BorrowLeft(p, n);
            else if (n != p.NumKeys && p.Children[n + 1].NumKeys > Min)
                BorrowRight(p, n);
            else
            {
                //if there is a left sibling
                if (n != 0)
                    Combine(p, n); //combine with left sibling
                else
                    Combine(p, n + 1); //combine with right sibling
            }
        }

        /// <summary>
        /// Used to delete value by borrowing left.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="n">The n.</param>
        private void BorrowLeft(Node p, int n)
        {
            Node underflowNode = p.Children[n];
            Node leftSibling = p.Children[n - 1];

            underflowNode.NumKeys++;

            //Shift all the keys and children in underflowNode one position right
            for (int i = underflowNode.NumKeys; i > 0; i--)
            {
                underflowNode.Keys[i + 1] = underflowNode.Keys[i];
                underflowNode.Children[i + 1] = underflowNode.Children[i];
            }
            underflowNode.Children[1] = underflowNode.Children[0];

            //move the seperator key from parent node p to underflowNode
            underflowNode.Keys[1] = p.Keys[n];

            //move the rightmost key of the node's leftSibling to the parent node p
            p.Keys[n] = leftSibling.Keys[leftSibling.NumKeys];

            //rightmost child of leftSibling becomes leftmost child of underflowNode
            underflowNode.Children[0] = leftSibling.Children[leftSibling.NumKeys];

            leftSibling.NumKeys--;
        }

        /// <summary>
        /// Used to delete value by borrowing right.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="n">The n.</param>
        private void BorrowRight(Node p, int n)
        {
            Node underflowNode = p.Children[n];
            Node rightSibling = p.Children[n + 1];

            //move the seperator key from the parent node p to underflowNode
            underflowNode.NumKeys++;
            underflowNode.Keys[underflowNode.NumKeys] = p.Keys[n + 1];

            //leftmost child of rightSibling becomes the rightmost child of underflowNode
            underflowNode.Children[underflowNode.NumKeys] = rightSibling.Children[0];
            rightSibling.NumKeys--;

            //move the leftmost key from the rightSibling to the parent node p
            p.Keys[n + 1] = rightSibling.Keys[1];

            //Shift all the keys and children in underflowNode one position left
            for (int i = 1; i <= rightSibling.NumKeys; i++)
            {
                rightSibling.Keys[i] = rightSibling.Keys[i + 1];
                rightSibling.Children[i] = rightSibling.Children[n + 1];
            }
        }

        /// <summary>
        /// Used to combine nodes after value deletion
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="m">The m.</param>
        private void Combine(Node p, int m)
        {
            Node nodeA = p.Children[m - 1];
            Node nodeB = p.Children[m];

            nodeA.NumKeys++;

            //Move the seperator key from the parent node p to nodeA
            nodeA.Keys[nodeA.NumKeys] = p.Keys[m];

            //Shift the keys and children that are after seperator key in node p one position left
            for (int i = m; i < p.NumKeys; i++)
            {
                p.Keys[i] = p.Keys[i + 1];
                p.Children[i] = p.Children[i + 1];
            }
            p.NumKeys--;

            //leftmost child of nodeB becomes rightmost child of nodeA
            nodeA.Children[nodeA.NumKeys] = nodeB.Children[0];

            //Insert all the keys and children of nodeB at the end of nodeA
            for (int i = 1; i < nodeB.NumKeys; i++)
            {
                nodeA.NumKeys++;
                nodeA.Keys[nodeA.NumKeys] = nodeB.Keys[i];
                nodeA.Children[nodeA.NumKeys] = nodeB.Children[i];
            }
        }
        #endregion

        #region Display PreOrder Traversal
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance in PreOrder traversal.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        override
        public string ToString()
        {
            string display = string.Empty;
            int height = 0;
            
            display = "Root Node:\n\n" + Display(Root, ref display, ref height);
            display += "\n\n" + String.Format("Number of Index nodes is : {0}\n", IndexCount) +
                String.Format("Number of Leaf nodes is : {0}\n         and they Average {1:##.#}% Full\n\n", LeafCount, (CollectPercentFull / LeafCount)) +
                String.Format("The Depth of the BTree is : {0}\n", Height) +
                String.Format("         with {0} levels of Index nodes and 1 level of Leaf nodes\n\n", Height) +
                String.Format("The total number of values in the tree is {0}\n", ValueCount);

            return display;
        }

        /// <summary>
        /// Recursive method to display the BTree in PreOrder Traversal
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="display">The display.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        private string Display(Node p, ref string display, ref int height)
        {
            if (p != null)
            {
                //increment height
                height++;

                //increment counters  
                if (this.Height < (height-1))
                    Height = (height-1);
                if (p.Type == Node.NodeType.Index)
                    IndexCount++;
                else
                {
                    LeafCount++;
                    CollectPercentFull += GetPercentFull(p);
                }

                //Display the Node
                display += "Node Type:  " + p.Type + "\n";
                if (p.Type == Node.NodeType.Index)
                    display += "Number of Keys:  " + (p.NumKeys + 1) + "  ";
                else
                    display += "Number of Keys:  " + p.NumKeys + "  ";

                display +=  String.Format("{0:##.#}% Full", GetPercentFull(p)) + "\n";
                display += "Values:\n";
                if(p.Type == Node.NodeType.Index)
                     display += "**".PadLeft(8);
                for (int i = 1; i <= p.NumKeys; i++)
                    display += String.Format("{0}", p.Keys[i]).PadLeft(8);
                if (p.Type == Node.NodeType.Index)
                    display += String.Format("\n\nLevel of Index in BTree:  {0}", height);
                display += "\n\n";

                //Search for children recursively
                if(p.Children[0] != null)
                {
                    for (int i = 0; i < p.Children.Length; i++)
                    {
                        display = Display(p.Children[i], ref display, ref height);
                    }
                }
                height--;
            }
            return display;       
        }

        /// <summary>
        /// Gets the percentage for how full a node is.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        private double GetPercentFull(Node p)
        {
            double percent = 0;

            if(p.Type == Node.NodeType.Leaf)
                percent = ((double)p.NumKeys/M)*100;
            else
                percent = ((double)(p.NumKeys + 1) / M) * 100;

            return percent;
        }
        #endregion
    }
}
