using System;
using System.Collections.Generic;

namespace B_Tree
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tree = new BTree(5);

            while (true)
            {
                Console.WriteLine("Enter integer for tree:");
                if (int.TryParse(Console.ReadLine(), out int num))
                {
                    tree.Insert(num);
                }
                else
                {
                    break;
                }
            }

            tree.ToString();

            Console.WriteLine("Enter value to search:");
            if (int.TryParse(Console.ReadLine(), out int valueToSearch))
            {
                Console.WriteLine(tree.Search(valueToSearch));
            }
            else
            {
                Console.WriteLine("Value is not valid");
            }

            Console.WriteLine("Enter value to delete:");
            if (int.TryParse(Console.ReadLine(), out int valueToDelete))
            {
                tree.Delete(valueToDelete);
            }
            else
            {
                Console.WriteLine("Value is not valid");
            }

            tree.ToString();
        }
    }
}
