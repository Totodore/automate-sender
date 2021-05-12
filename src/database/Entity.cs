namespace DatabaseHandler
{
	public abstract class Entity
	{
		public override string ToString()
		{
			string result = "";
			foreach (var prop in GetType().GetProperties())
				result += prop.Name + "=" + prop.GetValue(this, null)?.ToString() + "\n";
			return result;
		}
	}
}