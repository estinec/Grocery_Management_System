namespace Grocery_Management.Models.ViewModels;

public class PosCheckoutViewModel
{
    public List<CartItemInput> Items { get; set; } = new()
    {
        new CartItemInput { Quantity = 1 }
    };
}

public class CartItemInput
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}