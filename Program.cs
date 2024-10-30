using System;
using System.Collections.Generic;

abstract class User
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }

    protected User(string userId, string name, string phonenum)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        PhoneNumber = phonenum ?? throw new ArgumentNullException(nameof(phonenum));
    }

    public abstract void Register();
    public abstract void Login();
    public virtual void DisplayProfile()
    {
        Console.WriteLine($"User ID: {UserId}," +
            $" Name: {Name}, " +
            $"Phone: {PhoneNumber}");
    }
}

class Driver : User
{
    public string DriverId { get; set; }
    public string VehicleDetails { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<Trip> TripHistory { get; set; } = new List<Trip>();

    public Driver(string userId, string name, string phonenum, string driverId, string vehicleDetails)
        : base(userId, name, phonenum)
    {
        DriverId = driverId ?? throw new ArgumentNullException(nameof(driverId));
        VehicleDetails = vehicleDetails ?? throw new ArgumentNullException(nameof(vehicleDetails));
    }

    public override void Register() => Console.WriteLine($"Driver {Name} registered in the Log.");
    public override void Login() => Console.WriteLine($"Driver {Name} logged in Successfully.");

    public void AcceptRide(Trip trip)
    {
        if (IsAvailable)
        {
            IsAvailable = false;
            trip.DriverName = Name;
            Console.WriteLine($"{Name} has  accepted the ride.");
            TripHistory.Add(trip);
            trip.StartTrip();
        }
        else
        {
            Console.WriteLine($"{Name} is not currently available to accept rides RIGHT NOW!.");
        }
    }

    public void ViewTripHistory()
    {
        Console.WriteLine($"Trip History for {Name}:");
        foreach (var trip in TripHistory)
        {
            trip.DisplayTripDetails();
        }
    }

    public void ToggleAvailability()
    {
        IsAvailable = !IsAvailable;
        Console.WriteLine($"{Name} availability set to {IsAvailable}");
    }
}

class Rider : User
{
    public List<Trip> RideHistory { get; set; } = new List<Trip>();

    public Rider(string userId,string name,string phonenum) : base(userId, name, phonenum) { }

    public override void Register() => Console.WriteLine($"Rider {Name} registered.");
    public override void Login() => Console.WriteLine($"Rider {Name} logged in.");

    public Trip RequestRide(string strtloca, string destination)
    {
        if (string.IsNullOrEmpty(strtloca) || string.IsNullOrEmpty(destination))
            throw new ArgumentNullException("Start location and destination cannot be null or empty.");

        Console.WriteLine($"{Name} requested a ride from {strtloca} to {destination}");
        var trip = new Trip(RideHistory.Count+1, Name, null, strtloca, destination, 0);
        trip.CalculateFare();
        RideHistory.Add(trip);
        return trip;
    }

    public void ViewRideHistory()
    {
        Console.WriteLine($"Ride History for {Name}:");
        foreach (var trip in RideHistory)
        {
            trip.DisplayTripDetails();
        }
    }
}



class Trip
{
    public int TripId { get; set; }
    public string RiderName { get; set; }
    public string? DriverName { get; set; }
    public string StartLocation { get; set; }
    public string Destination { get; set; }
    public double Fare { get; set; }
    public string Status { get; set; } = " On Pending";

    public Trip(int tripId, string riderName, string? driverName, string startloca, string destination, double fare)
    {
        TripId=tripId;
        RiderName = riderName ?? throw new ArgumentNullException(nameof(riderName));
        DriverName = driverName;
        StartLocation = startloca ?? throw new ArgumentNullException(nameof(startloca));
        Destination=destination ?? throw new ArgumentNullException(nameof(destination));
        Fare = fare;
    }

    public void CalculateFare()
    {
        Fare = 70; 
    }

    public void StartTrip() => Status ="Ride Is Occupied";
    public void EndTrip() => Status = "Ride Completed";

    public void DisplayTripDetails()
    {
        Console.WriteLine($"Trip ID: {TripId}," +
            $" Rider:{RiderName}, " +
            $"Driver:{DriverName}, " +
            $"Start:{StartLocation}, " +
            $"Destination:{Destination}," +
            $"Fare:{Fare}, " +
            $"Status:{Status}");
    }
}
class RideSharingSystem
{
    private List<Rider> RegisteredRiders = new List<Rider>();
    private List<Driver> RegisteredDrivers = new List<Driver>();
    private List<Trip> AvailableTrips = new List<Trip>();

    public Rider ?GetRider(string riderId) => RegisteredRiders.FirstOrDefault(r => r.UserId==riderId);
    public Driver? GetDriver(string driverId) => RegisteredDrivers.FirstOrDefault(d => d.UserId==driverId);

    public void RegisterUser(string userType,string userId,string name,string phonenum,string? driverId = null,string? vehicleDetails = null)
    {
        if (userType == "Rider")
        {
            var rider = new Rider(userId, name, phonenum);
            RegisteredRiders.Add(rider);
            rider.Register();
        }
        else if (userType == "Driver")
        {
            if (driverId == null||vehicleDetails ==null)
                throw new ArgumentException("Driver ID and Vehicle details cannot be null for a driver.");

            var driver = new Driver(userId, name, phonenum, driverId,vehicleDetails);
            RegisteredDrivers.Add(driver);
            driver.Register();
        }
    }

    public void RequestRide(string riderId, string startloca, string destination)
    {
        var rider = GetRider(riderId);
        if (rider == null)
        {
            Console.WriteLine("Rider not found.");
            return;
        }

        var trip=rider.RequestRide(startloca,destination);
        AvailableTrips.Add(trip);
    }

    public void AcceptRide(string driverId)
    {
        var driver = GetDriver(driverId);
        var availableTrip =AvailableTrips.FirstOrDefault();

        if (driver ==null)
        {
            Console.WriteLine("Driver not found.");
            return;
        }
        if (availableTrip == null)
        {
            Console.WriteLine("No available trips Currently.");
            return;
        }

        driver.AcceptRide(availableTrip);
    }

    public void CompleteTrip(string driverId)
    {
        var driver = GetDriver(driverId);
        if (driver != null && driver.TripHistory.Any())
        {
            var trip = driver.TripHistory.Last();
            trip.EndTrip();
            driver.ToggleAvailability();
            AvailableTrips.Remove(trip);
            Console.WriteLine("Trip completed:");
            trip.DisplayTripDetails();
        }
    }
    public void DisplayAllTrips()
    {
        Console.WriteLine("All Trips:");
        foreach (var trip in AvailableTrips)
        {
            trip.DisplayTripDetails();
        }
    }
}
class Program
{
    static void Main()
    {
        var rideSharingSystem = new RideSharingSystem();
        bool exit=false;

        while (!exit)
        {
           Console.WriteLine("\n====================================" +
               "\nWelcome To My Ride Sharing System\n" +
               "\n===================================\n"+
                "1. Register as rider\n" +
                "2. Register as driver\n" +
                "3. Request a Ride (Rider)\n" +
                "4. Accept a Ride (Driver)\n" +
                "5. Complete a Trip (Driver)\n" +
                "6. View Ride History (Rider)\n" +
                "7.View Trip History (Driver)\n" +
                "8.Display All Trips\n" +
                "9. Exit\n");
            Console.Write("Choose an option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Enter Rider ID: ");
                    var riderId = Console.ReadLine()!;
                    Console.Write("Enter Name: ");
                    var riderName = Console.ReadLine()!;
                    Console.Write("Enter Phone Number: ");
                    var riderPhone = Console.ReadLine()!;
                    rideSharingSystem.RegisterUser("Rider", riderId, riderName, riderPhone);
                    break;

                case "2":
                    Console.Write("Enter Driver ID: ");
                    var driverId = Console.ReadLine()!;
                    Console.Write("Enter Name: ");
                    var driverName = Console.ReadLine()!;
                    Console.Write("Enter Phone Number: ");
                    var driverPhone = Console.ReadLine()!;
                    Console.Write("Enter Vehicle Details: ");
                    var vehicleDetails = Console.ReadLine()!;
                    rideSharingSystem.RegisterUser("Driver", driverId, driverName, driverPhone, driverId, vehicleDetails);
                    break;
                case "3":
                    Console.Write("Enter Rider ID: ");
                    var requestRiderId = Console.ReadLine()!;
                    Console.Write("Enter Start Location: ");
                    var startLocation = Console.ReadLine()!;
                    Console.Write("Enter Destination: ");
                    var destination = Console.ReadLine()!;
                    rideSharingSystem.RequestRide(requestRiderId, startLocation, destination);
                    break;
                case "4":
                    Console.Write("Enter Driver ID: ");
                    var acceptDriverId = Console.ReadLine()!;
                    rideSharingSystem.AcceptRide(acceptDriverId);
                    break;
                case "5":
                    Console.Write("Enter Driver ID: ");
                    var completeDriverId = Console.ReadLine()!;
                    rideSharingSystem.CompleteTrip(completeDriverId);
                    break;
                case "6":
                    Console.Write("Enter Rider ID: ");
                    var viewRiderId = Console.ReadLine()!;
                    var rider = rideSharingSystem.GetRider(viewRiderId);
                    rider?.ViewRideHistory();
                    break;
                case "7":
                    Console.Write("Enter Driver ID: ");
                    var viewDriverId = Console.ReadLine()!;
                    var driver = rideSharingSystem.GetDriver(viewDriverId);
                    driver?.ViewTripHistory();
                    break;
                case "8":
                    rideSharingSystem.DisplayAllTrips();
                    break;
                case "9":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}
