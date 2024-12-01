namespace lab4.Services
{
    internal class ParamsParser
	{
		internal static async Task<(string, string, string, QueryType, string, string, string, int)> Parse(string[] args)
		{
			int tokenAttrIndex = Array.FindIndex(args, arg => arg == "--access-token" || arg == "-t");
			int uriIndex = Array.FindIndex(args, arg => arg == "--uri");
            int pathAttrIndex = Array.FindIndex(args, arg => arg == "--path" || arg == "-p");
			int userIdIndex = Array.FindIndex(args, arg => arg == "--user-id" || arg == "-u");
            int loggingLevelIndex = Array.FindIndex(args, arg => arg == "--logging-level" || arg == "-l");
            int loginIndex = Array.FindIndex(args, arg => arg == "--neo-login");
            int passwordIndex = Array.FindIndex(args, arg => arg == "--neo-password");
			int queryIndex = Array.FindIndex(args, arg => arg == "--query" || arg == "-q");
			int requestsPerSecIndex = Array.FindIndex(args, arg => arg == "--requests-per-sec" || arg == "-r");

            if (tokenAttrIndex == -1 && queryIndex == -1)
				throw new ArgumentNullException("Access token не задан");

			string accessToken = tokenAttrIndex == -1 ? "" : GetNextArg(args, tokenAttrIndex);
			string uri = uriIndex == -1 ? "bolt://localhost:7687" : GetNextArg(args, uriIndex);
            string filePath = pathAttrIndex == -1 ? "result.json" : GetNextArg(args, pathAttrIndex);
			string userIdStr = userIdIndex == -1 ? "astraz1one" : GetNextArg(args, userIdIndex);
            string loggingLevelStr = loggingLevelIndex == -1 ? "" : GetNextArg(args, loggingLevelIndex);
            string login = loginIndex == -1 ? "neo4j" : GetNextArg(args, loginIndex);
            string password = passwordIndex == -1 ? "password" : GetNextArg(args, passwordIndex);
            string queryStr = queryIndex == -1 ? "" : GetNextArg(args, queryIndex);
			int requestsPerSec;

			if (requestsPerSecIndex == -1 || !int.TryParse(GetNextArg(args, requestsPerSecIndex), out requestsPerSec))
			{
				requestsPerSec = 3;
                await Logger.W($"Количество запросов в секунду было автоматически установлено на {requestsPerSec}");
            }

            if (loggingLevelStr != "")
				if (Enum.TryParse(loggingLevelStr, true, out LoggingLevel loggingLevel))
					Logger.LoggingLevel = loggingLevel;
				else
					await Logger.W($"Уровень логирования не был задан коректно и был выставлен автоматически как {Logger.LoggingLevel}");

			QueryType queryType = QueryType.NoQuery;

			if (queryStr != "")
				if (Enum.TryParse(queryStr, true, out QueryType parsedQueryType))
					queryType = parsedQueryType;
				else
					throw new ArgumentException($"Указан несуществующий тип запроса.");

            return (accessToken, userIdStr, filePath, queryType, uri, login, password, requestsPerSec);
		}

		static string GetNextArg(string[] arr, int index)
		{
			if (arr.Length > ++index)
				return arr[index];
			else
				throw new Exception($"Аргумент {arr[index - 1]} не указан");
		}
	}
}
