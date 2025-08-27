using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Tests
{
    public record CreatedProductResponse(int Id);
    public record CreatedItemResponse(int Id, int Quantity, int ProductId);
}
