# C#による MCP Server Webサーバー版 DI仕様

MCPの関数部分でDIを使う方法です。

## 作り方

```shell
$ dotnet new console -n McpServerSseDI
$ dotnet add package ModelContextProtocol.AspNetCore --prerelease
$ dotnet add package MySql.EntityFrameworkCore
```

## テスト

`.vscode/mcp.json` があるので、 Visual Studio Code を起動し、Ctrl + Alt + i を押してCopilot Chatを起動する。
起動後、 `isbnが9784065341681の作品` と入力してテストを行う。
