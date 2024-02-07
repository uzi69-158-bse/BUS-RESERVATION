using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using System.Security.Cryptography;
using System.Text;


public interface ILogin
{
    void SignUp();
    bool LoginUser();
}
class Login : ILogin
{
    private static string GetConnectionString()
    {
        // Replace this with your actual connection string or retrieve it from a configuration file.
        return "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\lenovo\\Documents\\databse.mdf;Integrated Security=True;Connect Timeout=30";
    }

    public void SignUp()
    {
        string connStr = GetConnectionString();

        using (SqlConnection conn = new SqlConnection(connStr))
        {
            try
            {
                conn.Open();

                // Get a non-empty and valid username
                string username;
                do
                {
                    Console.Write("Enter your username: ");
                    username = Console.ReadLine().Trim();

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        Console.WriteLine("Username cannot be empty. Please try again.");
                    }

                } while (string.IsNullOrWhiteSpace(username));

                // Get a non-empty and valid password
                string password;
                do
                {
                    Console.Write("Enter your password: ");
                    password = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        Console.WriteLine("Password cannot be empty. Please try again.");
                    }

                } while (string.IsNullOrWhiteSpace(password));

                // Insert the user into the database
                using (SqlCommand cmd = new SqlCommand("INSERT INTO users (username, PasswordHash, Salt) VALUES (@username, @passwordHash, @salt); SELECT SCOPE_IDENTITY();", conn))
                {
                    string salt = GenerateSalt();
                    string hashedPassword = HashPassword(password, salt);

                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@passwordHash", hashedPassword);
                    cmd.Parameters.AddWithValue("@salt", salt);

                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string userid = result.ToString();
                        Console.WriteLine($"Sign Up Successful. Your UserId is: {userid}");

                        string fetchedUsername = GetUsernameById(userid);
                        Console.WriteLine($"Fetched Username by UserId: {fetchedUsername}");
                    }
                    else
                    {
                        Console.WriteLine("Sign Up Failed. Unable to retrieve UserId.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }




    public bool LoginUser()
    {
        string connStr = GetConnectionString();

        using (SqlConnection conn = new SqlConnection(connStr))
        {
            try
            {
                conn.Open();

                // Get a non-empty and valid username
                string username;
                do
                {
                    Console.Write("Enter your username: ");
                    username = Console.ReadLine().Trim();

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        Console.WriteLine("Username cannot be empty. Please try again.");
                    }

                } while (string.IsNullOrWhiteSpace(username));

                // Get a non-empty and valid password
                string password;
                do
                {
                    Console.Write("Enter your password: ");
                    password = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        Console.WriteLine("Password cannot be empty. Please try again.");
                    }

                } while (string.IsNullOrWhiteSpace(password));

                using (SqlCommand cmd = new SqlCommand("SELECT PasswordHash, Salt FROM users WHERE Username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPasswordHash = reader["PasswordHash"].ToString();
                            string salt = reader["Salt"].ToString();

                            // Hash the provided password with the stored salt
                            string hashedPassword = HashPassword(password, salt);

                            // Compare the hashed passwords
                            if (storedPasswordHash == hashedPassword)
                            {
                                Console.WriteLine("Login Successful.");
                                return true;
                            }
                            else
                            {
                                Console.WriteLine("Invalid username or password.");

                                // Check if the user wants to sign up
                                Console.Write("Do you want to sign up? (yes/no): ");
                                string signUpChoice = Console.ReadLine().ToLower();
                                if (signUpChoice == "yes")
                                {
                                    SignUp();
                                }
                                else if (signUpChoice == "no")
                                {
                                    return false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        return false;
    }


    private string GenerateSalt()
    {
        byte[] saltBytes = new byte[32]; // You can adjust the size of the salt as needed
        using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    private string HashPassword(string password, string salt)
    {
        using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000))
        {
            byte[] hashedBytes = pbkdf2.GetBytes(32); // 32 bytes for a 256-bit key

            // Combine the salt and hashed password and convert to Base64
            byte[] combinedBytes = new byte[salt.Length + hashedBytes.Length];
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(salt), 0, combinedBytes, 0, salt.Length);
            Buffer.BlockCopy(hashedBytes, 0, combinedBytes, salt.Length, hashedBytes.Length);

            return Convert.ToBase64String(combinedBytes);
        }
    }
    public string GetUsernameById(string userid)
    {
        string connStr = GetConnectionString();

        string username = null;
        try
        {


            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.Open();

                string query = "SELECT username FROM users WHERE userid = @userid";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userid", userid);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            username = reader["username"].ToString();
                        }
                    }
                }

            }
        }

        catch (SqlException ex)
        {
            // Handle the exception, log it, or throw it further as needed
            Console.WriteLine($"SQL Error: {ex.Message}");
        }
        return username;


    }
}


public abstract class methods
{
    public abstract void CheckSeat(string busid);
    public abstract void BookSeat(string busid, string number, int seats);
    public abstract void CancelBooking(string busid);
    public abstract  BookingStatus ViewBookedSeats(string number);
    public abstract void AddBusRouteDetails(int routeId, string busid, string originCity, string destinationCity,string date, TimeSpan departureTime, TimeSpan arrivalTime);
    public abstract void DisplayTable();
    public abstract void ViewUserProfile(string username);

    public abstract void DeleteUserAccount(string username);




}

public class BookingRepository:methods
{
    private readonly string _connectionString;

    public BookingRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public override void CheckSeat(string busid)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT SUM(seats) AS TotalSeats " +
                               "FROM status " +
                               "WHERE busid = @busid";

                using (SqlCommand command = new SqlCommand(query, connection))
                {

                    command.Parameters.AddWithValue("@busid", busid);

                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        int totalSeats = Convert.ToInt32(result);
                        Console.WriteLine($"Total Seats for BusID {busid}: {totalSeats}");

                        if (totalSeats == 20)
                        {
                            Console.WriteLine("Seats are unavailable for BusID {busid}.");
                        }
                        else
                        {
                            Console.WriteLine("Seats are available for BusID {busid}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No data available for BusID {busid} in the Status table.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

    }
    
    

    public override void BookSeat(string busid, string number, int seats)
    {
        if (IsBusIdValid(busid))
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand("INSERT INTO status (busid, number, seats) VALUES (@busid, @number, @seats)", connection))
                {
                    command.Parameters.AddWithValue("@busid", busid);
                    command.Parameters.AddWithValue("@number", number);
                    command.Parameters.AddWithValue("@seats", seats);

                    int affectedRows = command.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        Console.WriteLine($"Successfully booked {seats} seats on bus {busid}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to book {seats} seats on bus {busid}");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"Invalid Bus ID {busid}. Booking failed.");
        }
    }

    private bool IsBusIdValid(string busid)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM bus_routes WHERE busid = @busid", connection))
            {
                command.Parameters.AddWithValue("@busid", busid);

                int result = (int)command.ExecuteScalar();

                return result == 1;
            }
        }
    }


    public override void CancelBooking(string busid)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                using (SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM status WHERE busid = @busid", connection, transaction))
                {
                    checkCommand.Parameters.AddWithValue("@busid", busid);
                    int bookingCount = (int)checkCommand.ExecuteScalar();

                    if (bookingCount > 0)
                    {
                        using (SqlCommand cancelCommand = new SqlCommand("DELETE FROM status WHERE busid = @busid", connection, transaction))
                        {
                            cancelCommand.Parameters.AddWithValue("@busid", busid);
                            cancelCommand.ExecuteNonQuery();
                            transaction.Commit();
                            Console.WriteLine($"Booking {busid} canceled successfully.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Booking {busid} not found.");
                    }
                }
            }
        }
    }

    public override BookingStatus ViewBookedSeats(string number)
    {
        BookingStatus status = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string query = "SELECT  busid, seats FROM status WHERE number = @number";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@number", number);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        status = new BookingStatus
                        {
                            Number = number,
                            Bookings = new List<Booking>()
                        };

                        while (reader.Read())
                        {
                            Booking booking = new Booking
                            {
                                BusId = (string)reader["busid"],
                                Seats = (int)reader["seats"]
                            };


                            status.Bookings.Add(booking);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No bookings found for number {number}.");
                    }
                }
            }
        }

        return status;
    }
    public override void AddBusRouteDetails(int routeId, string busid, string originCity, string destinationCity,string date, TimeSpan departureTime, TimeSpan arrivalTime)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string insertQuery = "INSERT INTO bus_routes (route_id, busid, origin_city, destination_city,date , departure_time, arrival_time) " +
                                 "VALUES (@RouteId, @busid, @OriginCity, @DestinationCity,@date, @DepartureTime, @ArrivalTime)";

            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@RouteId", routeId);
                command.Parameters.AddWithValue("@busid", busid);
                command.Parameters.AddWithValue("@OriginCity", originCity);
                command.Parameters.AddWithValue("@DestinationCity", destinationCity);
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@DepartureTime", departureTime);
                command.Parameters.AddWithValue("@ArrivalTime", arrivalTime);

                command.ExecuteNonQuery();
            }
        }
    }
    public override void DisplayTable()
    {

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            try
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand($"SELECT * FROM bus_routes", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Display column names
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader.GetName(i),-20} | ");
                        }
                        Console.WriteLine();

                        // Display separator line
                        Console.WriteLine(new string('-', reader.FieldCount * 22));

                        // Display data
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write($"{reader[i],-20} | ");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
    public override void ViewUserProfile(string username)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM users WHERE username = @username";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", username);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine($"User Profile for {username}:");

                        return;
                    }
                    else
                    {
                        Console.WriteLine($"User '{username}' not found.");
                    }
                }
            }
        }
    }

    public override void DeleteUserAccount(string username)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Ensure the user is authenticated and is deleting their own account
            string deleteQuery = "DELETE FROM users WHERE username = @username";
            using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
            {
                deleteCommand.Parameters.AddWithValue("@username", username);
                int rowsAffected = deleteCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine($"Account for user '{username}' deleted successfully.");
                    // Optionally, perform additional cleanup or logging
                }
                else
                {
                    Console.WriteLine($"Failed to delete the account for user '{username}'.");
                }
            }
        }
    }
}

public class BookingStatus
{
    public string Number { get; set; }
    public List<Booking> Bookings { get; set; }
}

public class Booking
{
    public string Status { get; set; }
    public string BusId { get; set; }
    public int Seats { get; set; }
}

interface Ibookingprogram
{
    void SignUp();
    bool LoginUser();
    void displaytable();

    void CheckSeat(string busid);
    void BookSeat(string busid, string number, int seats);
    void CancelBooking(string busid);
    BookingStatus ViewBookedSeats(string number);
    void Adding();
    void viewUserProfile();
    void Logout();

    void Run();


}


public class BookingProgram : ILogin, Ibookingprogram
{
    private readonly Login login;
    private readonly BookingRepository bookingRepository;
    private bool isLoggedIn;
    private string loggedInUser; // You might want to store the username of the logged-in user

    public BookingProgram()
    {
        // Initialize instances of Login and BookingRepository
        login = new Login();
        string connStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\lenovo\\Documents\\databse.mdf;Integrated Security=True;Connect Timeout=30"; // Add your connection string
        bookingRepository = new BookingRepository(connStr);
        isLoggedIn = false;
    }

    public void SignUp()
    {
        login.SignUp();
    }

    public bool LoginUser()
    {
        login.LoginUser();
        isLoggedIn = true;
        return true;
    }

    public void displaytable()
    {
        bookingRepository.DisplayTable();
    }

    // Existing methods from the 'methods' abstract class

    public void CheckSeat(string busid)
    {
        bookingRepository.CheckSeat(busid);
    }

    public void BookSeat(string busid, string number, int seats)
    {
        bookingRepository.BookSeat(busid, number, seats);
    }

    public void CancelBooking(string busid)
    {
        bookingRepository.CancelBooking(busid);
    }

    public BookingStatus ViewBookedSeats(string number)
    {
        return bookingRepository.ViewBookedSeats(number);
    }

    public void Adding()
    {
        Console.WriteLine("Enter password to add :");
        string passwordAttempt = Console.ReadLine();

        if (ValidateAdminPassword(passwordAttempt))
        {
            Console.WriteLine("Enter Bus Route Details:");

            // Add proper error handling for user input
            int routeId;
            do
            {
                Console.Write("Route ID: ");
            } while (!int.TryParse(Console.ReadLine(), out routeId));

            Console.Write("Bus ID: ");
            string busId = Console.ReadLine();

            Console.Write("Origin City: ");
            string originCity = Console.ReadLine();

            Console.Write("Destination City: ");
            string destinationCity = Console.ReadLine();

            Console.Write("Departure date: ");
            string date = Console.ReadLine();

            TimeSpan departureTime;
            do
            {
                Console.Write("Departure Time (HH:mm:ss): ");
            } while (!TimeSpan.TryParse(Console.ReadLine(), out departureTime));

            TimeSpan arrivalTime;
            do
            {
                Console.Write("Arrival Time (HH:mm:ss): ");
            } while (!TimeSpan.TryParse(Console.ReadLine(), out arrivalTime));

            // Call the function to add bus route details
            bookingRepository.AddBusRouteDetails(routeId, busId, originCity, destinationCity, date, departureTime, arrivalTime);

            Console.WriteLine("Bus route details added successfully.");
        }
        else
        {
            Console.WriteLine("Invalid password! Do you want to try again? (yes/no): ");
            string retryChoice = Console.ReadLine().ToLower();
            if (retryChoice == "yes")
            {
                Adding();
            }
            else
            {
                MainMenu();
            }
        }
    }

    // Function to validate the admin password
    private bool ValidateAdminPassword(string passwordAttempt)
    {
        return passwordAttempt == "admin";
    }




    public void viewUserProfile()
    {
        do
        {
            Console.WriteLine("enter your id no :");
            loggedInUser = Console.ReadLine();
        
            if (string.IsNullOrWhiteSpace(loggedInUser))
                    
            Console.WriteLine("ID cannot be empty. Please try again.");
        }

        while (string.IsNullOrWhiteSpace(loggedInUser));

        loggedInUser = login.GetUsernameById(loggedInUser); 
        if (string.IsNullOrEmpty(loggedInUser))
        {
            Console.WriteLine("Failed to retrieve user profile. Please try again later.");
            return;
        }

        Console.WriteLine($"User Profile for {loggedInUser}:");
        // Add other user profile information as needed

        Console.WriteLine("Do you want to delete your account? (yes/no)");
        string response = Console.ReadLine();
        if (response.ToLower() == "yes")
        {
            bookingRepository.DeleteUserAccount(loggedInUser);
            loggedInUser = null; // Optionally clear the loggedInUser after account deletion
        }
    }

    

      
    
    public void Logout()
    {
        isLoggedIn = false;
        loggedInUser = null;
        Console.WriteLine("Logged out successfully.");
    }

    public void MainMenu()
    {
        Console.WriteLine("\nChoose an option:");
        Console.WriteLine("1. See bus detail");
        Console.WriteLine("2. Check Seat Availability");
        Console.WriteLine("3. Book Seat");
        Console.WriteLine("4. Cancel Booking");
        Console.WriteLine("5. View Booked Seats");
        Console.WriteLine("6. View User Profile");
        Console.WriteLine("7. Add bus detail ");
        Console.WriteLine("8. Logout");
        Console.WriteLine("9. Exit");
    }

    public void Run()
    {
        Ibookingprogram repos = new BookingProgram();
        Console.WriteLine("Welcome to the Booking System!");

        while (true)
        {
            MainMenu();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    repos.displaytable();
                    break;

                case "2":
                    string busId;

                    while (true)
                    {
                        Console.Write("Enter bus ID: ");
                        busId = Console.ReadLine()?.Trim();

                        if (!string.IsNullOrEmpty(busId))
                        {

                            try
                            {
                                bookingRepository.CheckSeat(busId);
                                Console.WriteLine($"Seats available on bus {busId} for booking.");
                            }
                            catch (InvalidOperationException ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                            break;
                        }

                        Console.WriteLine("Bus ID cannot be empty. Please try again.");
                    }

                    break;


    

                case "3":
                    bookingRepository.DisplayTable();
                    Console.WriteLine("\n Enter the deatils");


                    string busIdToBook;
                    string phoneNumber;
                    int seatsToBook;

                    while (true)
                    {
                        Console.Write("Enter bus ID to book: ");
                        busIdToBook = Console.ReadLine();

                        Console.Write("Enter your phone number: ");
                        phoneNumber = Console.ReadLine();

                        Console.Write("Enter the number of seats to book: ");
                        string seatsInput = Console.ReadLine();

                        // Validate that none of the inputs are empty
                        if (!string.IsNullOrWhiteSpace(busIdToBook) && !string.IsNullOrWhiteSpace(phoneNumber) && !string.IsNullOrWhiteSpace(seatsInput))
                        {
                            // Validate and parse the seats input
                            if (int.TryParse(seatsInput, out seatsToBook))
                            {
                                try
                                {
                                    repos.BookSeat(busIdToBook, phoneNumber, seatsToBook);
                                    Console.WriteLine("Booking successful!\nYou will get call for paymet procedure");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error: {ex.Message}");
                                }

                                break; 
                            }
                            else
                            {
                                Console.WriteLine("Invalid seats input. Please enter a valid number.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("All inputs must be provided. Please try again.");
                        }
                    }

                    break;

                case "4":
                    string busIdToCancel;

                    while (true)
                    {
                        Console.Write("Enter bus ID for cancellation: ");
                        busIdToCancel = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(busIdToCancel))
                        {
                            try
                            {
                                repos.CancelBooking(busIdToCancel);
                                Console.WriteLine($"Booking for bus ID {busIdToCancel} successfully cancelled.");
                                break; // Exit the loop if cancellation is successful
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                                // Optionally, you can decide whether to break the loop here or allow the user to try again
                            }
                        }
                        else
                        {
                            Console.WriteLine("Bus ID cannot be empty. Please try again.");
                        }
                    }
                    break;

                case "5":
                    Console.Write("Enter your phone number to view booked seats: ");
                    string phoneNumberToView = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(phoneNumberToView))
                    {
                        BookingStatus bookingStatus = repos.ViewBookedSeats(phoneNumberToView);

                        if (bookingStatus != null)
                        {
                            Console.WriteLine($"Bookings for phone number {phoneNumberToView}:");
                            foreach (Booking booking in bookingStatus.Bookings)
                            {
                                Console.WriteLine($"Bus ID: {booking.BusId}, Seats: {booking.Seats}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No bookings found for phone number {phoneNumberToView}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Phone number cannot be empty. Please try again.");
                    }

                    break;

                case "6":
                    viewUserProfile();
                    break;

                case "7":
                    Adding();
                    break;


                case "8":
                    Logout();
                    login.LoginUser();
                    break;

                case "9":
                    Console.WriteLine("Exiting the Booking System. Goodbye!");
                    return;

                default:
                    Console.WriteLine("Invalid option. Please choose a valid option.");
                    break;
            }
        }

    }
}

class Program
{ 

    public static void Main()
    {
        BookingProgram bookingProgram = new BookingProgram();
        ILogin log = new Login();

        while (true)
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Sign Up");
            Console.WriteLine("2. Log In");
            Console.WriteLine("3. Exit");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    bookingProgram.SignUp();
                    break;

                case "2":
                    if (log.LoginUser())
                    {
                        bookingProgram.Run(); // Continue to main menu for logged-in users
                    }
                    else
                    {
                        Console.WriteLine("Invalid username or password.");
                    }
                    break;

                case "3":
                    Console.WriteLine("Exiting the Booking System. Goodbye!");
                    return;

                default:
                    Console.WriteLine("Invalid option. Please choose a valid option.");
                    break;
            }
        }
    }
}


