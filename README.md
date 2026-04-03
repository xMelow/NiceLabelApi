# NiceLabel SDK API

## Introduction
This is an API built around the NiceLabel SDK to use in different applications.
The NiceLabel SDK is built for .NET 4.7.2 so this API lets you use the SDK For all projects and applications.

## Requirements
- .NET 4.7.2
- IDE
- NiceLabel 10 (with valid SDK license)

## Installation
1. Clone the repository
2. open the project
3. run the project 
4. open https://localhost:44368

## Configuration
- NiceLabel 10 must be installed on the same machine as this API
- A valid NiceLabel license is required
- The API runs on port `44368` by default

## Endpoints
These are all the endpoints of the API.

### Variables
Returns a list of variables from a NiceLabel label file.

`POST /nicelabel/variables`

**Request**

`multipart/form-data`

| Parameter  | Type | Required | Description |
|------------|------|----------|-------------|
| label | file | yes | The NiceLabel label file |

**Response**

```json
[
  "Teller",
  "Name"
]
```
- `200 OK` - Returns list of strings
- `400 Bad Request` - If parameters fails

### Print
Prints a NiceLabel label file.

`POST /nicelabel/print`

**Request**

`multipart/form-data`

| Parameter   | Type | Required | Description                                                                 |
|-------------|------|----------|-----------------------------------------------------------------------------|
| label       | file | yes | The NiceLabel label file                                                    |
| quantity    | int | yes | The print amount                                                            |
| printerName | string | no | The name of the printer to print to if no printer is specified in the label |

**Response**
- `200 OK` - "Printing label..."
- `400 Bad Request` - If validation fails
- `500 Internal Server Error` - If printing fails

## License
This is an internal tool developed for Altec. All rights reserved.
