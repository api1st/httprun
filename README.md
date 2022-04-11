# httprun - Command line .http script executor

`httprun` allows execution of .http scripts, in batches, from the command line.

## Background

At [Apiway](https://www.apiway.net) we make platforms that help organisations publish APIs, we do a lot of demos, which utilise a lot of HTTP API calls. We were using the [Visual Studio Code Rest-client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) to drive the demos, but the setup, which could involve hundreds of HTTP requests, were tedious to click through.

The Visual Studio Code Rest-Client has become the defacto way of building and executing HTTP requests on the fly. However as a VS Code plugin, all its functionality is only available from within the VS Code IDE and must be mouse driven.

`httprun` is an attempt at a multi-platform, Rest-Client compatible way of executing scripts from a command line.

## Installation

1. Download the pre-built binaries

| OS | Download |
| -- | -------- |
| Windows | [httprun.exe](releases/httprun.exe) |
| Unix | [httprun](releases/ubuntu/httprun) |

or 

Compile source code `dotnet build`

2. [Optional] Add httprun to the path.


## Features

### Multiple requests per file

Use `###` to separate individual requests.

```http
GET /anything HTTP/1.1
Host: httpbin.org

###

GET /get HTTP/1.1
Host: httpbin.org

###
```

### Define 'File' variables per request

Use `@<variableName> = <value>` to define a variable for use in this or subsequent requests.

Use `{{<variableName>}}` to substitute variable value into Verb, Header and Body.

```http
# Create variable
@myVariable = Hello

GET /anything/{{myVariable}} HTTP/1.1
Host: httpbin.org

###
```

### Use variables within variable declarations

Embed variables within the value declaration of a new variable.

```http
@var_one = World
@var_two = Hello, {{var_one}}
```

### Name requests for reference in the future

Use `# @name <name>` within a request block to make the request and response addressible in subsequent requests.

```http
# Name the request
# @name myRequest

GET /anything HTTP/1.1
Host: httpbin.org

###

# Refer to named request
@host = {{myRequest.response.body.$.headers.host}}

GET /anything HTTP/1.1
Host: {{host}}

###
```

### Confirm execution of sensitive requests

Use `# @note` to pause execution of the script and require user input.

```http
# @note

GET /anything HTTP/1.1
Host: httpbin.org

###
```

Confirmation to continue must be provided via the command line.

Note: There is a command line switch to auto-confirm all notes when running in headless.

### Configurable success codes

Use the command line switch `-s` or `--success-codes` to supply a space separated list of HTTP status codes that should be treated as a success (and not termniate the script).

Useful if the API is known to respond with non-standard HTTP codes in success scenarios.

### Configurable request timeout

Use the command line switch `-t` or `--request-timeout` to change the response wait timeout for the script.

### Configurable inter-request delay

Use the command line switch `-d` or `--delay` to change how long delay should occur between each sequential request.

### Configurable following of 3xx responses

Use the command line switch `-f` or `--follow-300` to automatically follow 3xx based redirects.

## Feature Matrix

`httprun` aims to be compatible with the .http files that VS Code Rest Client supports. However, the functionality set offered by `httprun` is deliberately different, reflecting the different mode of usage.

Note: VS Code UI features are excluded from this table.

| Feature | `httprun` | VS Code Rest Client |
| ------- | --------- | ------------------- |
| Send, Cancel & Re-run http requests | Partial - Send only | Yes |
| Send GraphQL queries | No | Yes |
| Copy as cURL | No | Yes |
| Request History | No | Yes |
| Multiple requests in same file | Yes | Yes |
| Preview of Response | Yes | Yes |
| Save response to disk | No | Yes |
| Authentication support | | |
| * Basic Auth | Yes | Yes |
| * Digest Auth | Coming | Yes |
| * SSL Client Certificates | Coming | Yes |
| * Azure active directory | No | Yes |
| * Microsoft Identity Platform | No | Yes |
| * AWS Signature v4 | No | Yes |
| Environment and Variable support | | |
| Use variables in URL \ Headers \ Body | Yes | Yes |
| Environment, File and Request variables | Yes | Yes |
| System Dynamic variables | | |
| * {{$guid}} | Yes | Yes |
| * {{$randomInt min max}} | Yes | Yes |
| * {{$timestamp [offset option]}} | Yes | Yes |
| * {{$datetime rfc1123\|iso8601 [offset option]}} | Yes | Yes |
| * {{$localDatetime rfc1123\|iso8601 [offset option]}} | Yes | Yes |
| * {{$processEnv [%]envVarName}} | Yes | Yes |
| * {{$dotenv [%]variableName}} | No | Yes |
| * {{$aadToken [new] [public\|cn\|de\|us\|ppe] [<domain\|tenantId>] [aud:<domain\|tenantId>]}} | No | Yes |
| Cookie support | No | Yes |
| Proxy Support | No | Yes |
| Http Language support | | |
| * .http and .rest file support | Yes | Yes |
| * Comments (// & #) | Partial - # only | Yes |

## Command Line Options

Access the help using the `--help` command line switch.

