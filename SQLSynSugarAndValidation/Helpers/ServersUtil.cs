using DBSchema.schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;

namespace SQLSynSugarAndValidation.Helpers
{
	public class ServersUtil
	{
		public async Task<Server> DeserializeAsync
		(
			string serverName,
			string schemaPath
		)
		{
			Server serverDeserializedFromDB = null;
			//var filePath = $@"{schemaPath}\schemaSerialized.json";

			var rewriter = new Rewriter.SqlRewriter();

			serverDeserializedFromDB = await rewriter.GetSchema
			(
				serverName,
				schemaPath
			);
			return serverDeserializedFromDB;
#if false
			var options = new JsonSerializerOptions
			{
				//IncludeFields = true
			};
			if( File.Exists( filePath ) )
			{
				try
				{
					using( var stream = new FileStream
						(
							filePath, FileMode.Open,
							FileAccess.Read,
							FileShare.Read
						) )
					{
						//serverDeserializedFromDB = await JsonSerializer
						//	.DeserializeAsync<Server>( stream, options );
						//serverDeserializedFromDB.Comment = filePath;
						stream.Close();
					}
				}
				catch( Exception e )
				{
					System.Diagnostics.Debug.WriteLine( $"Exception {e.Message}" );
					System.Diagnostics.Debug.WriteLine( $"Stack {e.StackTrace}" );
					return serverDeserializedFromDB;
				}
			}
#endif
		}
	}
}
