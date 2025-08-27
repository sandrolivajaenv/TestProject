namespace Application.DTOs;

using System;
using System.Collections.Generic;

/// <summary>
/// Product create request
/// </summary>
/// <param name="ProductName"></param>
public record ProductCreateDto(string ProductName);

/// <summary>
/// Product update request
/// </summary>
/// <param name="ProductName"></param>
public record ProductUpdateDto(string ProductName);

/// <summary>
/// Represents a data transfer object for reading product information.
/// </summary>
/// <remarks>This DTO is typically used to transfer product data from the server to the client in a read-only
/// context. It includes details about the product, such as its identifier, name, creation metadata, modification
/// metadata, and associated items.</remarks>
/// <param name="Id"></param>
/// <param name="ProductName"></param>
/// <param name="CreatedBy"></param>
/// <param name="CreatedOn"></param>
/// <param name="ModifiedBy"></param>
/// <param name="ModifiedOn"></param>
/// <param name="Items"></param>
public record ProductReadDto(
    int Id,
    string ProductName,
    string CreatedBy,
    DateTime CreatedOn,
    string? ModifiedBy,
    DateTime? ModifiedOn,
    IEnumerable<ItemReadDto> Items
);