using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace B_Tree
{
    public class BTree
    {
        private Node _root;

        private readonly int _size;

        public BTree(int degree)
        {
            if (degree < 2)
            {
                throw new ArgumentException("BTree degree must be at least 2", "degree");
            }

            _root = new Node(degree);
            _size = degree;
        }

        #region Search
        public int? Search(int key)
        {
            return SearchInternal(_root, key);
        }

        private int? SearchInternal(Node node, int value)
        {
            int index = 0;
            for (int i = 0; i < node.Values.Count; i++)
            {
                if (value > node.Values[i])
                {
                    index++;
                }
            }

            if (index < node.Values.Count && node.Values[index] == value)
            {
                return node.Values[index];
            }

            return node.IsLeaf ? null : SearchInternal(node.Leaves[index], value);
        }

        #endregion

        #region Insert
        public void Insert(int newValueToInsert)
        {
            if (!_root.IsFull)
            {
                InsertNonFull(_root, newValueToInsert);
                return;
            }

            Node oldRoot = _root;
            _root = new Node(_size);
            _root.Leaves.Add(oldRoot);
            SplitChild(_root, 0, oldRoot);
            InsertNonFull(_root, newValueToInsert);
        }

        private void InsertNonFull(Node node, int newValueToInsert)
        {
            int positionToInsert = 0;
            for (int i = 0; i < node.Values.Count; i++)
            {
                if (newValueToInsert >= node.Values[i])
                {
                    positionToInsert++;
                }
            }

            if (node.IsLeaf)
            {
                node.Values.Insert(positionToInsert, newValueToInsert);
                return;
            }

            Node child = node.Leaves[positionToInsert];
            if (child.IsFull)
            {
                SplitChild(node, positionToInsert, child);
                if (newValueToInsert > node.Values[positionToInsert])
                {
                    positionToInsert++;
                }
            }

            InsertNonFull(node.Leaves[positionToInsert], newValueToInsert);
        }

        private void SplitChild(Node parentNode, int nodeToBeSplitIndex, Node nodeToBeSplit)
        {
            var newNode = new Node(_size);

            parentNode.Values.Insert(nodeToBeSplitIndex, nodeToBeSplit.Values[_size - 1]);
            parentNode.Leaves.Insert(nodeToBeSplitIndex + 1, newNode);

            newNode.Values.AddRange(nodeToBeSplit.Values.GetRange(_size, _size - 1));

            nodeToBeSplit.Values.RemoveRange(_size - 1, _size);

            if (!nodeToBeSplit.IsLeaf)
            {
                newNode.Leaves.AddRange(nodeToBeSplit.Leaves.GetRange(_size, _size));
                nodeToBeSplit.Leaves.RemoveRange(_size, _size);
            }
        }

        #endregion

        public void Delete(int valueToDelete)
        {
            CoreDelete(_root, valueToDelete);

            // if root's last entry was moved to a child node, remove it
            if (_root.Values.Count == 0 && !_root.IsLeaf)
            {
                _root = _root.Leaves.Single();
            }
        }

        private void CoreDelete(Node node, int valueToDelete)
        {
            int index = 0;
            for (int i = 0; i < node.Values.Count; i++)
            {
                if (valueToDelete > node.Values[i])
                {
                    index++;
                }
            }

            if (index < node.Values.Count && node.Values[index] == valueToDelete)
            {
                DeleteValueFromNode(node, valueToDelete, index);
                return;
            }

            if (!node.IsLeaf)
            {
                DeleteValueFromChild(node, valueToDelete, index);
            }
        }

        private void DeleteValueFromChild(Node parentNode, int valueToDelete, int indexInLeaves)
        {
            Node childNode = parentNode.Leaves[indexInLeaves];

            if (childNode.IsMinValues)
            {
                int leftIndex = indexInLeaves - 1;
                Node leftSibling = indexInLeaves > 0 ? parentNode.Leaves[leftIndex] : null;

                int rightIndex = indexInLeaves + 1;
                Node rightSibling = indexInLeaves < parentNode.Leaves.Count - 1
                                                ? parentNode.Leaves[rightIndex]
                                                : null;

                if (leftSibling != null && leftSibling.Values.Count > _size - 1)
                {
                    childNode.Values.Insert(0, parentNode.Values[indexInLeaves]);
                    parentNode.Values[indexInLeaves] = leftSibling.Values.Last();
                    leftSibling.Values.RemoveAt(leftSibling.Values.Count - 1);

                    if (!leftSibling.IsLeaf)
                    {
                        childNode.Leaves.Insert(0, leftSibling.Leaves.Last());
                        leftSibling.Leaves.RemoveAt(leftSibling.Leaves.Count - 1);
                    }
                }
                else if (rightSibling != null && rightSibling.Values.Count > _size - 1)
                {
                    childNode.Values.Add(parentNode.Values[indexInLeaves]);
                    parentNode.Values[indexInLeaves] = rightSibling.Values.First();
                    rightSibling.Values.RemoveAt(0);

                    if (!rightSibling.IsLeaf)
                    {
                        childNode.Leaves.Add(rightSibling.Leaves.First());
                        rightSibling.Leaves.RemoveAt(0);
                    }
                }
                else
                {
                    if (leftSibling != null)
                    {
                        var position = indexInLeaves - 1 >= 0 ? indexInLeaves - 1 : 0;
                        childNode.Values.Insert(0, parentNode.Values[position]);
                        var oldValues = childNode.Values;
                        childNode.Values = leftSibling.Values;
                        childNode.Values.AddRange(oldValues);
                        if (!leftSibling.IsLeaf)
                        {
                            var oldLeaves = childNode.Leaves;
                            childNode.Leaves = leftSibling.Leaves;
                            childNode.Leaves.AddRange(oldLeaves);
                        }

                        parentNode.Leaves.RemoveAt(leftIndex);
                        parentNode.Values.RemoveAt(position);
                    }
                    else
                    {
                        childNode.Values.Add(parentNode.Values[indexInLeaves]);
                        childNode.Values.AddRange(rightSibling.Values);
                        if (!rightSibling.IsLeaf)
                        {
                            childNode.Leaves.AddRange(rightSibling.Leaves);
                        }

                        parentNode.Leaves.RemoveAt(rightIndex);
                        parentNode.Values.RemoveAt(indexInLeaves);
                    }
                }
            }

            CoreDelete(childNode, valueToDelete);
        }

        private void DeleteValueFromNode(Node node, int valueToDelete, int keyIndexInNode)
        {
            if (node.IsLeaf)
            {
                node.Values.RemoveAt(keyIndexInNode);
                return;
            }

            Node leftNode = node.Leaves[keyIndexInNode];
            if (leftNode.Values.Count >= _size)
            {
                int predecessor = DeleteRightChild(leftNode);
                node.Values[keyIndexInNode] = predecessor;
            }
            else
            {
                Node rightNode = node.Leaves[keyIndexInNode + 1];
                if (rightNode.Values.Count >= _size)
                {
                    int successor = DeleteLeftChild(leftNode);
                    node.Values[keyIndexInNode] = successor;
                }
                else
                {
                    leftNode.Values.Add(node.Values[keyIndexInNode]);
                    leftNode.Values.AddRange(rightNode.Values);
                    leftNode.Leaves.AddRange(rightNode.Leaves);

                    node.Values.RemoveAt(keyIndexInNode);
                    node.Leaves.RemoveAt(keyIndexInNode + 1);

                    CoreDelete(leftNode, valueToDelete);
                }
            }
        }

        private int DeleteRightChild(Node node)
        {
            if (node.IsLeaf)
            {
                var result = node.Values[^1];
                node.Values.RemoveAt(node.Values.Count - 1);
                return result;
            }

            return DeleteRightChild(node.Leaves.Last());
        }

        private int DeleteLeftChild(Node node)
        {
            if (node.IsLeaf)
            {
                var result = node.Values[0];
                node.Values.RemoveAt(0);
                return result;
            }

            return DeleteRightChild(node.Leaves.First());
        }

        private int degree = 0;

        public override string ToString()
        {
            var pairs = new List<(int, int[])>();

            var strings = new List<string>();
            Temp(_root, pairs, 0);
            for (int i = degree; i >= 0; i--)
            {
                strings.Insert(0, Temp1(pairs, degree - i));
            }

            for (int i = 0; i < strings.Count; i++)
            {
                Console.WriteLine(strings[i]);
                Console.WriteLine();
            }

            return string.Empty;
        }

        private string Temp1(List<(int, int[])> pairs, int tabsCount)
        {
            var list = pairs.Where(x => x.Item1 == degree - tabsCount).Select(x => x.Item2).ToList();

            string output = string.Empty;
            string tabs = tabsCount == 0 ? "  ": new string('\t', tabsCount);
            foreach (var item in list)
            {
                string values = string.Empty;
                for (int i = 0; i < item.Length; i++)
                {
                    values += $" {item[i]} ";
                }

                output += $"{tabs} {values} {tabs}";
            }

            return output;
        }

        private void Temp(Node node, List<(int,int[])> pairs, int position)
        {
            pairs.Add((position, node.Values.ToArray()));

            if (degree < position)
            {
                degree = position;
            }

            foreach (var leaf in node.Leaves)
            {
                Temp(leaf, pairs, position + 1);
            }
        }
    }
}
