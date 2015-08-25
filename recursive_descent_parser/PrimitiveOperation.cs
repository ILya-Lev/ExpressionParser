namespace recursive_descent_parser
{
	/// <summary>
	/// simple expression with no argument in parenthesis
	/// however may use alias or long-named variable as an argument
	/// e.g.: hello world + have a nice day
	/// e.g.: @1 + a
	/// </summary>
	class PrimitiveOperation
	{
		public int Start { get; set; }
		public int End { get; set; }
		public string Value { get; set; }
		public int SequentalNumber { get; set; }
	}
}