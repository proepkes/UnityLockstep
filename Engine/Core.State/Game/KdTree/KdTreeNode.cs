using System;
using System.Text;

namespace Lockstep.Core.State.Game.KdTree
{
    [Serializable]
	public class KdTreeNode<TKey, TValue>
	{      
		public KdTreeNode(TKey[] point, TValue value)
		{
			Point = point;
			Value = value;
		}

		public readonly TKey[] Point;
		public TValue Value;

		internal KdTreeNode<TKey, TValue> LeftChild;
		internal KdTreeNode<TKey, TValue> RightChild;

		internal KdTreeNode<TKey, TValue> this[int compare]
		{
			get
            {
                if (compare <= 0)
					return LeftChild;
                return RightChild;
            }
			set
			{
				if (compare <= 0)
					LeftChild = value;
				else
					RightChild = value;
			}
		}

		public bool IsLeaf => (LeftChild == null) && (RightChild == null);

        public override string ToString()
		{
			var sb = new StringBuilder();

			foreach (var value in Point)
            {
                sb.Append(value + "\t");
            }

			if (Value == null)
				sb.Append("null");
			else
				sb.Append(Value);

			return sb.ToString();
		}
	}
}