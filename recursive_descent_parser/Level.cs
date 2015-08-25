using System;
using System.Collections.Generic;

namespace recursive_descent_parser
{
	/// <summary>
	/// level is an abstraction of expression evaluation stage
	/// expression in parenthesis must be evaluated before one not surrounded with braces
	/// one may call such expression to be evaluated on higher level
	/// </summary>
	class Level : IComparable<Level>
	{
		public int Start { get; set; }
		public int End { get; set; }
		public string Value { get; set; }				// raw substring of the initial one
		public string PrimitiveValue { get; set; }		// all higher levels are replaced with one variable each
		public int Height { get; set; }

		private List<Level> m_replacedLevels = new List<Level>();

		public void AddReplacement(Level subLevel)
		{
			m_replacedLevels.Add(subLevel);
		}

		public int CurrentReplacementNumber()
		{
			return m_replacedLevels.Count;
		}

		public Level this[int idx]
		{
			get
			{
				return m_replacedLevels[idx];
			}
		}

		private List<PrimitiveOperation> m_operations = new List<PrimitiveOperation>();

		public void AddOperation(PrimitiveOperation operation)
		{
			m_operations.Add(operation);
		}

		public IEnumerable<PrimitiveOperation> OperationSequence()
		{
			foreach (var operation in m_operations)
			{
				yield return operation;
			}
		}

		public void IntersectionToOperation(int idxOfOperator, out string leftIntersection, out string rightIntersection)
		{
			leftIntersection = "";
			rightIntersection = "";
			foreach (var operation in m_operations)
			{
				if (operation.End == idxOfOperator - 1)
					leftIntersection = operation.Value;
				else if (operation.Start == idxOfOperator + 1)
					rightIntersection = operation.Value;
			}
		}

		public int CompareTo(Level other)
		{
			int diffHeights = other.Height - Height;
			return diffHeights == 0 ? Start - other.Start : diffHeights;
		}

		public bool ContainsLevel(Level other)
		{
			if (other.Height <= Height)
				throw new ArgumentException("lower or with equal height level cannot be nested!");
			return other.Start >= Start && other.End < End;
		}
	}
}
