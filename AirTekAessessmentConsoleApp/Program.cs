using System.Text.Json;

List<Flight> flights = new List<Flight>();

while (true)
{
    Console.WriteLine("Please select an operation:");
    Console.WriteLine("1. Load Flight Schedule");
    Console.WriteLine("2. Output Flight Schedule");
    Console.WriteLine("3. Generate Flight Itineraries");
    Console.WriteLine("4. Exit");

    var input = Console.ReadLine();

    switch (input)
    {
        case "1":
            flights = LoadFlightSchedule();
            break;
        case "2":
            OutputFlightSchedule(flights);
            break;
        case "3":
            GenerateFlightItineraries(flights);
            break;
        case "4":
            return;
        default:
            Console.WriteLine("Invalid input. Please select an operation by entering its number.");
            break;
    }
}

#region Methods

/// <summary>
/// Prompts the user to enter a flight schedule and returns a list of flights.
/// </summary>
/// <returns>A list of flights representing the flight schedule.</returns>
List<Flight> LoadFlightSchedule()
{
    var flights = new List<Flight>();

    Console.WriteLine("Enter flight schedule [departure,arrival,day] (press enter on an empty line to stop):");
    var flightNumber = 1;
    while (true)
    {
        Console.Write($"Flight {flightNumber}: ");
        var input = Console.ReadLine();
        if (string.IsNullOrEmpty(input))
        {
            break;
        }

        var parts = input.Split(',');
        if (parts.Length != 3)
        {
            Console.WriteLine("Invalid input. Please enter in the format 'departure,arrival,day'.");
            continue;
        }

        var departure = parts[0];
        var arrival = parts[1];

        if (!int.TryParse(parts[2], out var day))
        {
            Console.WriteLine("Invalid day. Please enter a number.");
            continue;
        }

        flights.Add(new Flight(flightNumber, departure, arrival, day));
        flightNumber++;
    }

    return flights;
}

/// <summary>
/// Outputs the flight schedule to the console.
/// </summary>
/// <param name="flights">The list of flights to output.</param>
void OutputFlightSchedule(List<Flight> flights)
{
    if (!flights.Any())
    {
        Console.WriteLine("No flights scheduled.");
    }
    foreach (var flight in flights)
    {
        Console.WriteLine($"Flight: {flight.Number}, departure: {flight.Departure}, arrival: {flight.Arrival}, day: {flight.Day}");
    }
}

/// <summary>
/// Generate the flight itineraries to the console, given a json file representing the orders.
/// </summary>
/// <param name="flights">The list of flights representing the flight schedule.</param>
/// <returns>A list of flights representing the flight schedule with the orders scheduled.</returns>
List<Flight> GenerateFlightItineraries (List<Flight> flights)
{
    Console.WriteLine("Please enter the path of the orders file:");

    var filePath = Console.ReadLine();
    var orders = LoadOrders(filePath);
    flights = ScheduleOrders(flights, orders);
    return flights;
}

/// <summary>
/// Loads the orders json file and returns a list of orders.
/// </summary>
/// <param name="filePath">The file path of the json file.</param>
/// <returns>A list of orders in the json file.</returns>
List<Order> LoadOrders(string filePath)
{
    try
    {
        var json = File.ReadAllText(filePath);
        var dictionary = JsonSerializer
            .Deserialize<Dictionary<string, Order>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        var orders = dictionary
            .Select(keyValuePair => new Order(keyValuePair.Key, keyValuePair.Value.Destination))
            .ToList();

        return orders;
    }
    catch (Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
        return new List<Order>();
    }
}

/// <summary>
/// Schedule orders into the flight schedule.
/// </summary>
/// <param name="flights">The list of flights representing the flight schedule.</param>
/// <param name="orders">The list of orders.</param>
/// <returns>A list of flights representing the flight schedule with the orders scheduled.</returns>
List<Flight> ScheduleOrders (List<Flight> flights, List<Order> orders)
{
    foreach (var order in orders)
    {
        var flight = flights.FirstOrDefault(f => f.Arrival == order.Destination && !f.IsFull());
        if (flight == default(Flight))
        {
            Console.WriteLine($"order: {order.Id}, flightNumber: not scheduled");
        }
        else
        {
            Console.WriteLine($"order: {order.Id}, flightNumber: {flight.Number}, departure: {flight.Departure}, arrival: {flight.Arrival}, day: {flight.Day}");
            flight.Orders.Add(order);
        }
    }
    return flights;
}
#endregion

#region Classes
class Flight
{
    public int Number { get; }
    public string Departure { get; }
    public string Arrival { get; }
    public int Day { get;  }
    public List<Order> Orders { get; set;  }
    public int Capacity { get; }

    public Flight(int number, string departure, string arrival, int day, int capacity = 20)
    {
        Number = number;
        Departure = departure;
        Arrival = arrival;
        Day = day;
        Capacity = capacity;
        Orders = new List<Order>();
    }

    public bool IsFull()
    {
        return Orders.Count >= Capacity;
    }
}

class Order
{
    public string Id { get; }
    public string Destination { get; set; }

    public Order(string id, string destination)
    {
        Id = id;
        this.Destination = destination;
    }
}
#endregion