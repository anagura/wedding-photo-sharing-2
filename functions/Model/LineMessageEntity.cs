using Microsoft.WindowsAzure.Storage.Table;

namespace functions.Model
{
	public class LineMessageEntity : TableEntity
	{
		public LineMessageEntity(string name, string id)
		{
			this.PartitionKey = name;
			this.RowKey = id;
		}

		public LineMessageEntity() { }

		public long Id { get; set; }

		public string Name { get; set; }

		public string Message { get; set; }
	}
}
