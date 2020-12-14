namespace SqlCollector.Models
{
	public class ResourceDto
	{
		public string id { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public string location { get; set; }
		public string resourceGroup { get; set; }
		public string tenantId { get; set; }
		public string subscriptionId { get; set; }

		public ResourceDto()
		{

		}
	}
}
