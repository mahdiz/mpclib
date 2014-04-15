using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpcLib.Common.FiniteField
{
	public enum Operation
	{
		None,
		Add,
		Mul,
		Div,
		Sub
	}

	public enum FunctionType
	{
		Sum,
		Mul,
		Equal
	}

	/// <summary>
	/// Parser line type.
	/// </summary>
	internal enum LineType
	{
		None,
		Inputs,
		Output,
		Formula,
		Empty
	}

	internal enum SearchFor
	{
		AddMin,
		DivMul,
		Equal,
		QuestionMark
	}

	public enum VectorType
	{
		Row,
		Column
	}
}