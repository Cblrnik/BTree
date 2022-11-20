using System;
using System.Collections.Generic;

namespace B_Tree
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tree = new BTree(2);
            tree.Insert(3);
            tree.Insert(4);
            tree.Insert(5);
            tree.Insert(6);
            tree.Insert(7);
            tree.Insert(8);
            tree.Insert(9);
            tree.Insert(10);
            tree.Insert(11);
            tree.Insert(12);
            //tree.Search(5);
            tree.Delete(10);
            tree.ToString();
        }
    }
}
