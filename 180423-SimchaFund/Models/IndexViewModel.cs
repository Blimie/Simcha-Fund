using _180423_SimchaFund.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _180423_SimchaFund.Models
{
    public class IndexViewModel
    {
        public IEnumerable<SimchaWithCountAndTotal> Simchas { get; set; }
        public int TotalContributorCount { get; set; } 
    }
    public class ContributorsViewModel
    {
        public IEnumerable<Contributor> Contributors { get; set; }
        public decimal TotalBalance { get; set; }
    }
    public class ContributionsViewModel
    {
        public IEnumerable<ContributorContribution> Contributors { get; set; }
        public Simcha Simcha { get; set; }
    }
    public class EmailsViewModel
    {
        public IEnumerable<string> Names { get; set; }
        public Simcha Simcha { get; set; }
    }
    public class HistoryViewModel
    {
        public Contributor Contributor { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}