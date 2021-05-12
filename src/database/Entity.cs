namespace DatabaseHandler
{
	public abstract class Entity
	{
		public override string ToString()
		{
			string result = GetType().Name + " {\n";
			foreach (var prop in GetType().GetProperties())
				result += "\t" + prop.Name + ": " + prop.GetValue(this, null)?.ToString() + "\n";
			result += "}";
			return result;
		}
	}
}