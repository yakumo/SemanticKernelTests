# C#による MCP Server STDIO版

基本的にはこちらの、 [.NET を使用して最小限の MCP サーバーを作成して接続する](https://learn.microsoft.com/ja-jp/dotnet/ai/quickstarts/build-mcp-server) に書かれていることそのままです。

## 作り方

```shell
$ dotnet new console -n McpServerStdio
$ dotnet add package ModelContextProtocol --prerelease
$ dotnet add package Microsoft.Extensions.Hosting
$ dotnet add reference ../McpServerCommon
```

## テスト

`.vscode/mcp.json` があるので、 Visual Studio Code を起動し、Ctrl + Alt + i を押してCopilot Chatを起動する。
起動後、 `注目の漫画の情報を` と入力してテストを行う。
