# OData Query String Generator

## Overview

The **OData Query String Generator** is a library designed to generate query string parameters for OData APIs using objects and LINQ expressions. It was originally developed for use with the Azure Cognitive Search engine, which leverages the Elastic Search engine.

## Features

- Generate OData query strings dynamically using LINQ expressions.
- Supports complex queries, including nested collections and logical operators.
- Handles various data types, including `DateTime`, `Guid`, and nullable types.
- Designed to work seamlessly with Azure Cognitive Search and other OData-compliant APIs.

## Getting Started

### Prerequisites

- .NET Standard 2.0 or higher.
- .NET Framework 4.8 for testing.

### Installation

Clone the repository and include the `ODataGenerator` project in your solution.

```bash
git clone <repository-url>
```

### Usage

1. Add a reference to the `ODataGenerator` project in your solution.
2. Use the `FilterGenerator<T>` class to generate OData query strings.

#### Example

```csharp
using ODataGenerator;
using System;

var filterGenerator = new FilterGenerator<Record>();
var query = filterGenerator.Generate(record => record.Status == Status.Active && record.Number > 100);
Console.WriteLine(query); // Outputs: "Status eq 0 and Number gt 100"
```

## Testing

The project includes a comprehensive test suite in the `ODataGeneratorTests` project. To run the tests:

1. Open the solution in Visual Studio.
2. Build the solution.
3. Run the tests using the Test Explorer.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Acknowledgments

- Azure Cognitive Search
- Elastic Search

---

For more information, please refer to the official OData documentation: [https://www.odata.org/](https://www.odata.org/)