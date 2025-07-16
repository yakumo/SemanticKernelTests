# C#による MCP Server Webサーバー版

こちらの、 [ASP.NET Core extensions for the MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk/blob/main/src/ModelContextProtocol.AspNetCore/README.md) に書かれていることそのままです。

## 作り方

```shell
$ dotnet new console -n McpServerSse
$ dotnet add package ModelContextProtocol.AspNetCore --prerelease
$ dotnet add reference ../McpServerCommon
```

## テスト

`.vscode/mcp.json` があるので、 Visual Studio Code を起動し、Ctrl + Alt + i を押してCopilot Chatを起動する。
起動後、 `注目の漫画の情報を` と入力してテストを行う。
