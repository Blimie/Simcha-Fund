using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _180423_SimchaFund.Models; 
using _180423_SimchaFund.Data;

namespace _180423_SimchaFund.Controllers
{
    public class HomeController : Controller
    {
        SimchaFundDB dB = new SimchaFundDB(Properties.Settings.Default.ConStr);
        public ActionResult Index()
        {
            return View(new IndexViewModel
            {
                Simchas = dB.GetSimchas(),
                TotalContributorCount = dB.GetContributorsCount()
            });
        }
        public ActionResult Contributors()
        {
            return View(new ContributorsViewModel
            {
                Contributors = dB.GetContributorsWithBalances(),
                TotalBalance = dB.GetTotalBalance()
            });
        }
        public ActionResult Emails(int simchaId)
        {
            return View(new EmailsViewModel
            {
                Names = dB.GetContributorNamesForSimcha(simchaId),
                Simcha = dB.GetSimchaById(simchaId)
            });
        }
        public ActionResult Contributions(int simchaId)
        {
            IEnumerable<Contributor> contributors = dB.GetContributorsWithBalances();
            IEnumerable<Contribution> contributions = dB.GetContributionsForSimcha(simchaId);
            List<ContributorContribution> ccs = new List<ContributorContribution>();
            foreach (var c in contributors)
            {
                Contribution contribution = contributions.FirstOrDefault(item => item.ContributorId == c.Id);
                var cc = new ContributorContribution
                {
                    ContributorWithBalance = c
                };
                if (contribution != null)
                {
                    cc.Contribute = true;
                    cc.Amount = contribution.Amount;
                }
                ccs.Add(cc);
            }
            return View(new ContributionsViewModel
            {
                Contributors = ccs,
                Simcha = dB.GetSimchaById(simchaId)
            });
        }
        [HttpPost]
        public ActionResult AddSimcha(Simcha s)
        {
            dB.AddSimcha(s);
            return Redirect("/");
        }
        [HttpPost]
        public ActionResult AddContributor(Contributor c, decimal initialAmount)
        {
            dB.AddContributor(c);
            dB.Deposit(new Deposit { Amount = initialAmount, ContributorId = c.Id, Date = c.DateCreated });
            return Redirect("/Home/Contributors");
        }
        [HttpPost]
        public ActionResult UpdateContributor(Contributor contributor)
        {
            dB.UpdateContributor(contributor);
            return Redirect("/Home/Contributors");
        }
        [HttpPost]
        public ActionResult AddContributions(IEnumerable<Contribution> contributions, int simchaId)
        {
            dB.DeleteContributionsBySimchaId(simchaId);
            IEnumerable<Contribution> co = contributions.Where(c => c.Contribute);
            dB.AddContributions(co, simchaId);
            return Redirect("/");
        }
        [HttpPost]
        public ActionResult Deposit(Deposit deposit)
        {
            dB.Deposit(deposit);
            return Redirect("/Home/Contributors");
        }
        public ActionResult History(int contributorId)
        {
            List<Transaction> transactions = dB.GetSimchasForContributor(contributorId).ToList();
            transactions.AddRange(dB.GetDepositsForContributor(contributorId));
            return View(new HistoryViewModel
            {
                Contributor = dB.GetContributorWithBalance(contributorId),
                Transactions = transactions.OrderBy(t => t.Date)
            });
        }
    }
}