using System;
using System.Collections.Generic;
using System.Linq;

namespace recursive_descent_parser
{
	class ParserLevel
	{
		private readonly string m_initString;
		private const string s_operations = "+-><=&|";
		private const string s_token = "@";
		private readonly List<Level> m_levels = new List<Level>();

		public ParserLevel(string initString)
		{
			m_initString = initString;
		}
		/// <summary>
		/// parses the initial expression; consciously designed to take no arguments
		/// </summary>
		/// <returns>a sequence of expressions to be evaluated</returns>
		public List<string> Parse()
		{
			DefineLevels();

			//Console.WriteLine("\nPrimitives");
			ConstructPrimitives();

			//Console.WriteLine("\ntracing levels\n");
			foreach (var level in m_levels)
			{
				TraceLevel(level);
			}

			List<string> sequenceExpressions = new List<string>();
			foreach (var level in m_levels)
			{
				sequenceExpressions.AddRange(InstantiatePrimitive(level));
			}
			return sequenceExpressions;
		}
		/// <summary>
		/// each sub-expression - a set of symbols in parenthesis - must be evaluated on higher level
		/// as higher the level is as earlier the expression must be evaluated
		/// </summary>
		private void DefineLevels()
		{
			m_levels.Add(new Level { Start = 0, End = m_initString.Length, Height = 0, Value = m_initString });
			int height = 0;
			Stack<Level> stackLevels = new Stack<Level>();
			for (int i = 0; i < m_initString.Length; i++)
			{
				if (m_initString[i] == '(')
				{
					height++;
					var level = new Level { Start = i, Height = height };
					stackLevels.Push(level);
					m_levels.Add(level);
				}
				else if (m_initString[i] == ')')
				{
					if (stackLevels.Count == 0)
					{
						throw new ArgumentException("initial string has corrupted parenthesis structure - there is closing brace after no opening one");
					}
					var level = stackLevels.Peek();
					level.End = i;
					level.Value = m_initString.Substring(level.Start, level.End - level.Start + 1);		//bad idea for more than 2 levels
					stackLevels.Pop();
					height--;
				}
			}
			if (stackLevels.Count != 0)
			{
				throw new ArgumentException("initial string has corrupted parenthesis structure - there is opening brace with no closing one");
			}
			m_levels.Sort();

			//// development check
			//foreach (var level in m_levels)
			//{
			//	Console.WriteLine(level.Value);
			//}
		}
		/// <summary>
		/// higher level expressions could be theoretically evaluated and the result value could be instantiated into lower level expression
		/// to get rid of parenthesis at current level the sub-level is replaced with abstract value=@index of sub-level in Level.m_replacedLevels
		/// </summary>
		private void ConstructPrimitives()
		{
			foreach (var level in m_levels)
			{
				if (level.Height == m_levels[0].Height)
				{
					level.PrimitiveValue = level.Value;
				}
			}
			for (int height = m_levels[0].Height - 1; height >= 0; height--)
			{
				for (int i = 0; i < m_levels.Count; i++)
				{
					var level = m_levels[i];
					if (level.Height == height)
					{
						level.PrimitiveValue = level.Value;
						for (int j = 0; j < i; j++)
						{
							var subLevel = m_levels[j];
							if (subLevel.Height == height + 1)
							{
								if (level.ContainsLevel(subLevel))
								{
									level.PrimitiveValue = level.PrimitiveValue.Replace(subLevel.Value, string.Format("{0}{1}", s_token, level.CurrentReplacementNumber()));
									level.AddReplacement(subLevel);
								}
							}
						}
					}
				}
			}
			foreach (var level in m_levels)
			{
				level.PrimitiveValue = level.PrimitiveValue.Replace("(", "");
				level.PrimitiveValue = level.PrimitiveValue.Replace(")", "");
			}
			//// development check
			//foreach (var level in m_levels)
			//{
			//	Console.WriteLine(level.PrimitiveValue);
			//}
		}
		/// <summary>
		/// primitive value of one level expression could be split into sequence of operations regarding operator priorities
		/// results are stored into Level.m_operations
		/// </summary>
		/// <param name="level">level to work with</param>
		private static void TraceLevel(Level level)
		{
			List<string> levelsTrace = new List<string>();
			Dictionary<int, string> operationByPriority = new Dictionary<int, string> { { 2, "+-><=" }, { 1, "&" }, { 0, "|" } };
			int operationCounter = 0;
			foreach (int priority in operationByPriority.Keys)
			{
				for (int i = 1; i + 1 < level.PrimitiveValue.Length; i++)
				{
					if (operationByPriority[priority].Contains(level.PrimitiveValue[i]))
					{
						string leftIntersection, rightIntersection;
						level.IntersectionToOperation(i, out leftIntersection, out rightIntersection);
						PrimitiveOperation operation = new PrimitiveOperation
						{
							Start = i - (leftIntersection == "" ? DistanceToLeftOperation(level.PrimitiveValue, i) : leftIntersection.Length),
							End = i + (rightIntersection == "" ? DistanceToRightOperation(level.PrimitiveValue, i) : rightIntersection.Length)
						};

						operation.Value = level.PrimitiveValue.Substring(operation.Start, operation.End - operation.Start + 1);
						operation.SequentalNumber = operationCounter++;

						level.AddOperation(operation);
					}
				}
			}

			//// development check
			//Console.WriteLine("for level {0} we have the sequence: ", level.Value);
			//foreach (var operation in level.OperationSequence())
			//{
			//	Console.WriteLine(operation.Value);
			//}
		}
		/// <summary>
		/// prompt function to find out what is the length of left operand - it might have more than 1 symbol
		/// </summary>
		/// <param name="expression">current expression to work with</param>
		/// <param name="idxCurrentOperation">index of current operation for which we are defining left operand</param>
		/// <returns></returns>
		private static int DistanceToLeftOperation(string expression, int idxCurrentOperation)
		{
			for (int i = idxCurrentOperation - 1; i > 0; i--)
			{
				if (s_operations.Contains(expression[i]))
				{
					return idxCurrentOperation - i - 1;
				}
			}
			return idxCurrentOperation;
		}
		/// <summary>
		/// prompt function to find out what is the length of right operand - it might have more than 1 symbol
		/// </summary>
		/// <param name="expression">current expression to work with</param>
		/// <param name="idxCurrentOperation">index of current operation for which we are defining right operand</param>
		/// <returns></returns>
		private static int DistanceToRightOperation(string expression, int idxCurrentOperation)
		{
			for (int i = idxCurrentOperation + 1; i < expression.Length; i++)
			{
				if (s_operations.Contains(expression[i]))
				{
					return i - idxCurrentOperation - 1;
				}
			}
			return expression.Length - idxCurrentOperation - 1;
		}
		/// <summary>
		/// at this point we already know the sequence of operators to be evaluated on current level
		/// the last touch we should apply is to return back sub-level raw value in place of its alias
		///  (i.e. @index - see ConstructPrimitives)
		/// </summary>
		/// <param name="level">current level to work with</param>
		/// <returns>sequence of expressions to be evaluated</returns>
		private static IEnumerable<string> InstantiatePrimitive(Level level)
		{
			List<string> sequentalExpressions = new List<string>();
			foreach (var operation in level.OperationSequence())
			{
				string expression = operation.Value;
				int tokenIdx = expression.IndexOf(s_token, StringComparison.InvariantCultureIgnoreCase);
				while (tokenIdx != -1)
				{
					string levelAlias = expression.Substring(tokenIdx, DistanceToRightOperation(expression, tokenIdx) + 1);
					int sublevelIdx = int.Parse(levelAlias.TrimStart(s_token.ToCharArray()));
					expression = expression.Replace(levelAlias, level[sublevelIdx].Value);
					tokenIdx = expression.IndexOf(s_token, StringComparison.InvariantCultureIgnoreCase);
				}
				sequentalExpressions.Add(expression);
			}
			//// development check
			//Console.WriteLine("for level {0} we have the sequence: ", level.Value);
			//foreach (var expression in sequentalExpressions)
			//{
			//	Console.WriteLine(expression);
			//}

			return sequentalExpressions;
		}
	}
}
