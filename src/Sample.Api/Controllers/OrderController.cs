using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Sample.Api.Contracts;

namespace Sample.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController :
    ControllerBase
{
    readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger)
    {
        _logger = logger;
    }


    [HttpPost(Name = "SubmitOrder")]
    public async Task<IActionResult> Post([FromBody] Order order, [FromServices] IRequestClient<SubmitOrder> client)
    {
        var response = await client.GetResponse<OrderSubmissionAccepted>(new
        {
            order.OrderId
        });

        return Ok(new
        {
            response.Message.OrderId
        });
    }

    [HttpGet(Name = "GetOrderStatus")]
    public async Task<IActionResult> Get(Guid id, [FromServices] IRequestClient<GetOrderStatus> client)
    {
        var response = await client.GetResponse<OrderStatus, OrderNotFound>(new
        {
            OrderId = id
        });

        if (response.Is(out Response<OrderStatus>? order))
            return Ok(order!.Message);

        return NotFound();
    }
}