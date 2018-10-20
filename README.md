# Cloutility API consumer example
This repository contains a C#.NET example of consuming the Cloutility REST API.

## Getting started
Before being able to access the API, the application needs to be autohorized by the Cloutility instance. To do this:

1. Sign into Cloutility and navigate to the business unit which you want your application to access as its root/main/home business unit. When choosing a business unit, remember that the API will be able to access the said business unit as well as all of its descendants.
- Go to "settings" for the business unit.
- Go to "API access".
- Click "add".
- Fill in details:
  - Name: A friendly name that helps you identify the application - both under "API access" and as part of audit logs.
  - Origin: A URL which is required as part of the HTTP-request's `Origin` header, which doesn't have to be a reachable destination, e.g `https://my.application`. The purpose of the `Origin` is to provide an extra layer of security in the sense that the `client_id` (see below) alone cannot be used to access the API.
  - Allow refresh tokens: Decide whether your application will use "refresh tokens" or simply request a new `access token` before each request.
- Click "add application".

The application will now receive a `client_id` and be visible in the list of external applications with access to the said business unit. You now have the required `Origin` and `client_id` in order for you to access the API with the application.
