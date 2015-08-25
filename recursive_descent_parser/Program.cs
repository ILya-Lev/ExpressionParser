using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace recursive_descent_parser
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				string expression = Expression(args.Length > 0 ? args[0] : "");
				ParserLevel pl = new ParserLevel(expression);
				List<string> expressionSequence = pl.Parse();
				foreach (var expr in expressionSequence)
				{
					Console.WriteLine(expr);
				}
			}
			catch (ArgumentException exc)
			{
				Console.WriteLine(exc.Message);
			}
			catch (OverflowException exc)
			{
				Console.WriteLine(exc.Message);
			}
			catch (FormatException exc)
			{
				Console.WriteLine(exc.Message);
			}
			finally
			{
				Console.ReadKey();
			}
		}

		static string Expression(string fileName)
		{
			if (fileName.Length == 0)
			{
				return "(a+(b>c)|c<(day off>another day off)|(150>879))&d=expression!";
			}
			using (FileStream stream = new FileStream(fileName, FileMode.Open))
			{
				using (TextReader reader = new StreamReader(stream))
				{
					return reader.ReadLine();
				}
			}
		}
	}
}
