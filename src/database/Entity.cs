using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

namespace AutomateSender.DatabaseHandler
{
	public abstract class Entity
	{
		public override string ToString()
		{
			string result = GetType().Name + " {\n";
			List<string> objects = GetType().GetProperties().Where(el => el.Name.EndsWith("Id")).ToList().ConvertAll(el => Regex.Replace(el.Name, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1])).Split(" ")[0]);
			foreach (var prop in GetType().GetProperties())
			{
				if (!objects.Contains(prop.Name)) //This condition is to avoid stack overflow with nested Entity inherited properties
					result += "\t" + prop.Name + ": " + prop.GetValue(this, null)?.ToString() + "\n";
			}
			result += "}";
			return result;
		}
	}
}