using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Location;

public record Address
{
    public string City { get; }
    public string Street { get; }
    public string HouseNumber { get; }

    private Address(string city, string street, string houseNumber)
    {
        City = city;
        Street = street;
        HouseNumber = houseNumber;
    }

    public static Result<Address> Create(string city, string street, string houseNumber)
    {
        if (string.IsNullOrEmpty(city))
            return Result.Failure<Address>("City cannot be null or empty");

        if (string.IsNullOrEmpty(street))
            return Result.Failure<Address>("Street cannot be null or empty");

        if (string.IsNullOrEmpty(houseNumber))
            return Result.Failure<Address>("House number cannot be null or empty");

        var address = new Address(city, street, houseNumber);
        return Result.Success(address);
    }

    public override string ToString()
    {
        var partOfAddress = new List<string>()
        {
            City,
            Street,
            HouseNumber
        };

        var filteredParts = partOfAddress.Where(part => string.IsNullOrWhiteSpace(part) == false);
        return string.Join(", ", filteredParts);
    }
}