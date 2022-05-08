This sample shows how to use MassTransit's container-based test harness with the `WebApplicationFactory`, without requiring the application under test to know about the test harness.

The included `docker-compose.yml` can be used to start RabbitMQ and Redis so that the `Sample.Api` project can be run and interactively tested in the browser using the Swagger UI.

The `Sample.Tests` project uses `AddMassTransitTestHarness` to replace the RabbitMQ transport and Redis saga repository with the in-memory transport and in-memory saga repository, allowing the test to run without any backing services.

> This requires MassTransit 8.0.3, develop-444 or later.