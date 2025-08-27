namespace Application.DTOs
{
    /// <summary>
    /// Represents the data transfer object used to create a new item with a specified quantity.
    /// </summary>
    /// <param name="Quantity">The quantity of the item to be created. Must be a non-negative integer.</param>
    public record ItemCreateDto(int Quantity);

    /// <summary>
    /// Represents a data transfer object for reading item details.
    /// </summary>
    /// <remarks>This DTO is typically used to transfer item data from the server to the client in a read-only
    /// context.</remarks>
    /// <param name="Id"></param>
    /// <param name="Quantity"></param>
    /// <param name="ProductId"></param>
    public record ItemReadDto(int Id, int Quantity, int ProductId);
}
