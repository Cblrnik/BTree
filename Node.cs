using System.Collections.Generic;

namespace B_Tree
{
    public class Node
    {
        private readonly int size;
        private List<Node> _leaves;
        private List<int> _values;

        public Node(int size)
        {
            this.size = size;
            _leaves = new List<Node>(size);
            _values = new List<int>(size);
        }

        public List<Node> Leaves 
        {
            get
            {
                return _leaves;
            }
            set 
            {
                if (value != null)
                {
                    _leaves = value;
                }
            } 
        }

        public List<int> Values
        {
            get
            {
                return _values;
            }
            set
            {
                if (value != null)
                {
                    _values = value;
                }
            }
        }

        public bool IsLeaf
        {
            get
            {
                return _leaves.Count == 0;
            }
        }

        public bool IsFull
        {
            get
            {
                return _values.Count == (2 * this.size) - 1;
            }
        }

        public bool IsMinValues
        {
            get
            {
                return _values.Count == this.size - 1;
            }
        }
    }
}
