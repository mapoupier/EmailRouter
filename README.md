# EmailRouter

EmailRouter is a tool designed to help legacy applications send emails over SMTP without needing to open the outbound SMTP protocol on their network. The program sets up a small SMTP server and forwards emails using the SendGrid API.

## Features

- Creates a local SMTP server
- Forwards emails using SendGrid API
- Suitable for legacy applications

## Requirements

- SendGrid API Key

## Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/mapoupier/EmailRouter.git
    ```
2. Navigate to the project directory:
    ```bash
    cd EmailRouter
    ```
3. Build the project:
    ```bash
    dotnet build
    ```

## Configuration

1. Create an `appsettings.json` file in the `MAPEK.EmailRouter` project directory with the following content:
    ```json
    {
        "SendGrid": {
            "ApiKey": "your-sendgrid-api-key"
        },
        "Smtp": {
            "Port": 25
        }
    }
    ```
    Replace `your-sendgrid-api-key` with your actual SendGrid API key.

2. Optionally, you can configure the SMTP port in the `appsettings.json`.

## Usage

1. Run the application:
    ```bash
    dotnet run --project MAPEK.EmailRouter
    ```
2. Configure your legacy application to send emails to `localhost` on the configured SMTP port.

## Docker

To run the application using Docker:

1. Build the Docker image:
    ```bash
    docker build -t emailrouter .
    ```
2. Run the Docker container:
    ```bash
    docker run -d -p 25:25 -e SENDGRID_API_KEY=your-sendgrid-api-key emailrouter
    ```
    Replace `your-sendgrid-api-key` with your actual SendGrid API key.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Contact

For any inquiries, please contact the project maintainer.

