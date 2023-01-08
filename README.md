# omnivore-api-demo-csharp

A simple .NET 7 C# demo of Omnivore's API. This app connects to Omnivore's API, and paginates through
all of the supplied user's recently saved articles and saves their contents as markdown
into a `documents` directory.

Optionally you can set a search term.

A port .NET 7 C# from https://github.com/omnivore-app/omnivore-api-demo

## Requirements

- .NET >= version 7
- An Omnivore account
- Auth token from Omnivore

### Environment Variables

* `OMNIVORE_AUTH_TOKEN=<string>` // mandatory
* `OMNIVORE_API_URL=<string>` // optional, defaults to production service
* `SEARCH_TERM=<string>` // optional, defaults to an empty string

## Getting Started

Currently Omnivore does not expose API Tokens, but you can use the `auth` Cookie
from a logged in session.

1. Go to https://omnivore.app, login, and copy the value of the auth cookie. How to do this
differs for every browser. If you're using Chrome:

- Open Developer Tools (Option + Cmd + I)
- Go the Application tab
- Find the `auth` cookie in the list. Copy the value. It should look something like this: `eyJhbGciOiJIUzI1NiI`

2. Run `dotnet run`
