using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace _180423_SimchaFund.Data
{
    public class Contributor
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CellNumber { get; set; }
        public decimal Balance { get; set; }
    }      
    public class ContributorContribution
    {
        public Contributor ContributorWithBalance { get; set; }
        public decimal? Amount { get; set; }
        public bool Contribute { get; set; }
    }
    public class Simcha
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
    }
    public class SimchaWithCountAndTotal : Simcha
    {                     
        public int Count { get; set; }
        public decimal Total { get; set; }
    }
    public class Deposit
    {
        public int Id { get; set; }
        public int ContributorId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
    public class Contribution
    {
        public int SimchaId { get; set; }
        public int ContributorId { get; set; }
        public decimal Amount { get; set; }
        public bool Contribute { get; set; }
    }
    public class Transaction
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public string Amount { get; set; } 
    }

    public class SimchaFundDB
    {
        private string _connectionString;
        public SimchaFundDB(string connectionString)
        {
            _connectionString = connectionString;
        }
        public void AddContributor(Contributor contributor)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Contributors(DateCreated, FirstName, LastName, CellNumber) 
                                    VALUES(@date, @first, @last, @cell)
                                    SELECT CAST(SCOPE_IDENTITY() AS INT)";
                cmd.Parameters.AddWithValue("@date", contributor.DateCreated);
                cmd.Parameters.AddWithValue("@first", contributor.FirstName);
                cmd.Parameters.AddWithValue("@last", contributor.LastName);
                cmd.Parameters.AddWithValue("@cell", contributor.CellNumber);
                connection.Open();
                contributor.Id = (int)cmd.ExecuteScalar();
            }
        }
        public void AddSimcha(Simcha simcha)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Simchas(Date, Name) 
                                    VALUES(@date, @name)
                                    SELECT CAST(SCOPE_IDENTITY() AS INT)";
                cmd.Parameters.AddWithValue("@date", simcha.Date);
                cmd.Parameters.AddWithValue("@name", simcha.Name);
                connection.Open();
                simcha.Id = (int)cmd.ExecuteScalar();
            }
        }
        public void AddContributions(IEnumerable<Contribution> contributions, int simchaId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Contributions(SimchaId, ContributorId, Amount) 
                                    VALUES(@simchaId, @contributorId, @amount)";
                connection.Open();
                foreach (Contribution c in contributions)
                {
                    cmd.Parameters.AddWithValue("@simchaId", simchaId);
                    cmd.Parameters.AddWithValue("@contributorId", c.ContributorId);
                    cmd.Parameters.AddWithValue("@amount", c.Amount);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }
        }
        public void Deposit(Deposit deposit)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"INSERT INTO Deposits(ContributorId, Amount, Date) 
                                    VALUES(@contributorId, @amount, @date)
                                    SELECT CAST(SCOPE_IDENTITY() AS INT)";
                cmd.Parameters.AddWithValue("@contributorId", deposit.ContributorId);
                cmd.Parameters.AddWithValue("@amount", deposit.Amount);
                cmd.Parameters.AddWithValue("@date", deposit.Date);
                connection.Open();
                deposit.Id = (int)cmd.ExecuteScalar();
            }
        }    
        public IEnumerable<Contributor> GetContributorsWithBalances()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT c.*, (SELECT ISNULL(SUM(Amount), 0) FROM Deposits WHERE ContributorId = c.Id) - 
                                    (SELECT ISNULL(SUM(Amount), 0) 
                                    FROM Contributions 
                                    WHERE ContributorId = c.Id) AS Balance
                                    FROM Contributors c";
                connection.Open();
                List<Contributor> list = new List<Contributor>();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Contributor
                    {
                        Id = (int)reader["Id"],
                        FirstName = (string)reader["FirstName"],
                        DateCreated = (DateTime)reader["DateCreated"],
                        LastName = (string)reader["LastName"],
                        CellNumber = (string)reader["CellNumber"],
                        Balance = (decimal)reader["Balance"]
                    });
                }
                return list;
            }
        }
        public Contributor GetContributorWithBalance(int contributorId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT c.*, (SELECT ISNULL(SUM(Amount), 0) FROM Deposits WHERE ContributorId = c.Id) - 
                                    (SELECT ISNULL(SUM(Amount), 0) 
                                    FROM Contributions 
                                    WHERE ContributorId = c.Id) AS Balance
                                    FROM Contributors c
                                    WHERE c.Id = @id";
                connection.Open();
                cmd.Parameters.AddWithValue("@id", contributorId);
                SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;                       
                }
                return new Contributor {
                    Id = (int)reader["Id"],
                    FirstName = (string)reader["FirstName"],
                    DateCreated = (DateTime)reader["DateCreated"],
                    LastName = (string)reader["LastName"],
                    CellNumber = (string)reader["CellNumber"],
                    Balance = (decimal)reader["Balance"]
                };      
            }
        }
        public IEnumerable<Contribution> GetContributionsForSimcha(int simchaId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Contributions WHERE SimchaId = @id";
                connection.Open();
                cmd.Parameters.AddWithValue("@id", simchaId);
                List<Contribution> list = new List<Contribution>();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Contribution
                    {
                        ContributorId = (int)reader["ContributorId"],
                        Amount = (decimal)reader["Amount"]
                    });
                }
                return list;
            }
        }
        public IEnumerable<string> GetContributorNamesForSimcha(int simchaId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT co.FirstName + ' ' + co.LastName AS Name FROM Contributions c
                                    JOIN Contributors co
                                    ON c.ContributorId = co.Id
                                    WHERE c.SimchaId = @id";
                connection.Open();
                cmd.Parameters.AddWithValue("@id", simchaId);
                List<string> list = new List<string>();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add((string)reader["Name"]);
                }
                return list;
            }
        }
        public IEnumerable<Transaction> GetSimchasForContributor(int contributorId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"select s.Name, s.Date, c.Amount from Contributions c 
                                    join Simchas s
                                    on s.Id = c.SimchaId
                                    where c.ContributorId = @id";
                connection.Open();
                cmd.Parameters.AddWithValue("@id", contributorId);
                List<Transaction> list = new List<Transaction>();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Transaction
                    {
                        Action = $"Contribution to the {(string)reader["Name"]} simcha",
                        Date = (DateTime)reader["Date"], 
                        Amount = $"- {(decimal)reader["Amount"]}"
                    });
                }
                return list;
            }
        }
        public IEnumerable<Transaction> GetDepositsForContributor(int contributorId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT Date, Amount FROM Deposits 
                                    WHERE ContributorId = @id";
                connection.Open();
                cmd.Parameters.AddWithValue("@id", contributorId);
                List<Transaction> list = new List<Transaction>();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Transaction
                    {
                        Action = "Deposit",
                        Date = (DateTime)reader["Date"],
                        Amount = $"+ {(decimal)reader["Amount"]}"
                    });
                }
                return list;
            }
        }
        public IEnumerable<SimchaWithCountAndTotal> GetSimchas()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT s.*, Count(c.SimchaID) AS Count, IsNull(Sum(c.Amount),0) AS Total
                                    FROM Simchas s
                                    LEFT JOIN Contributions c
                                    ON s.ID = c.SimchaID
                                    GROUP BY s.ID, s.Name, s.Date";
                connection.Open();
                List<SimchaWithCountAndTotal> list = new List<SimchaWithCountAndTotal>();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new SimchaWithCountAndTotal
                    {
                        Id = (int)reader["Id"],
                        Date = (DateTime)reader["Date"],
                        Name = (string)reader["Name"],
                        Count = (int)reader["Count"],
                        Total = (decimal)reader["Total"]
                    });
                }
                return list;
            }
        }
        public void DeleteContributionsBySimchaId(int simchaId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Contributions WHERE SimchaId = @id";
                command.Parameters.AddWithValue("@id", simchaId);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        public Simcha GetSimchaById(int simchaId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Simchas WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", simchaId);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                return new Simcha
                {
                    Id = (int)reader["Id"],
                    Date = (DateTime)reader["Date"],
                    Name = (string)reader["Name"]
                };
            }
        }
        public int GetContributorsCount()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Count(id) FROM Contributors";
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }
        public decimal GetTotalBalance()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT (SELECT ISNULL(SUM(Amount), 0) FROM Deposits) - 
                                           (SELECT ISNULL(SUM(Amount), 0) FROM Contributions)";
                connection.Open();
                return (decimal)cmd.ExecuteScalar();
            }
        }           
        public void UpdateContributor(Contributor contributor)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand cmd = connection.CreateCommand())
            {                       
                cmd.CommandText = @"UPDATE Contributors 
                                    SET FirstName = @firstName, LastName = @lastName, CellNumber = @cellnumber, DateCreated = @date                                     
                                    WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", contributor.Id);
                cmd.Parameters.AddWithValue("@firstName", contributor.FirstName);
                cmd.Parameters.AddWithValue("@lastName", contributor.LastName);
                cmd.Parameters.AddWithValue("@cellnumber", contributor.CellNumber);
                cmd.Parameters.AddWithValue("@date", contributor.DateCreated);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
