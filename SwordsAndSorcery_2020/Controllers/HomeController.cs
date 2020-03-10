using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PayPal.Api;
using System.Web.Mvc;
using Utilities;
using SwordsAndSorcery_2020.Models;
using SwordsAndSorcery_2020.ModelTypes;

namespace SwordsAndSorcery_2020.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            HomeModel model = new HomeModel();

            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            var model = new FeedbackModel();

            return View(model);
        }

        public ActionResult ErrorPage()
        {

            return View();

        }
        public JsonResult SaveData(string email, string name)
        {
            HomeModel model = new HomeModel();
            string sRet = model.SaveEMail(email, name);

            return Json("", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Special_Offer()
        {
            ViewBag.Title = "A special offer from The Fovean Chronicles!";
            return View();
        }

        [HttpPost]
        public ActionResult Contact(FeedbackModel model)
        {
            //verify that this isn't a robot
            if (model.Value2 + model.Value1 != model.Solution)
            {
                ModelState.AddModelError("", "Sorry, but " + model.Value1.ToString() + " + " + model.Value2.ToString() +
                    " does not equal " + model.Solution.ToString());
                return View(model);
            }
            else if (model.SenderEmail == null || model.SenderName == null)
            {
                ModelState.AddModelError("", "Name and Email Address are both required");
                return View(model);
            }
            else
            {
                bool res = model.SaveFeedback(HttpContext.Request.UserHostAddress.StringSafe());
                if (res)
                {
                    Email e = new Email();
                    e.FromAddress = model.SenderEmail;
                    e.Message = model.Message;
                    e.SenderName = model.SenderName;
                    e.Subject = "Feedback from Swords and Sorcery";
                    e.IP = HttpContext.Request.UserHostAddress.StringSafe();
                    e.SendEmail(false);
                    return View("Index");
                }
                else
                {
                    ModelState.AddModelError("", model.Error);

                    return View(model);
                }
            }
        }
        public ActionResult Fovea()
        {
            FoveaModel model = new FoveaModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult NewTerm(FoveaModel model)
        {
            if (model.Value2 + model.Value1 != model.Solution)
            {
                ModelState.AddModelError("", "Sorry, but " + model.Value1.ToString() + " + " + model.Value2.ToString() +
                    " does not equal " + model.Solution.ToString());
                return View("Fovea", model);
            }
            else
            {
                bool bRet = model.AddTerm();
                if (bRet)
                {
                    model = new FoveaModel();
                    model.Message = "Your update will be added when it is approved";
                }
                return View("Fovea", model);
            }
        }
        public ActionResult News()
        {
            var model = new News();
            return View(model);
        }
        public ActionResult Contests()
        {
            return View();
        }
        public ActionResult Indomitus_Est()
        {
            return View();
        }

        public ActionResult PaymentWithPaypal(string guid, string paymentId, string token, string PayerID)
        {
            //getting the apiContext
            APIContext apiContext = PaypalConfiguration.GetAPIContext();

            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal
                //Payer Id will be returned when payment proceeds or click to pay
                string payerId = PayerID.StringSafe();

                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist
                    //it is returned by the create function call of the payment class

                    // Creating a payment
                    // baseURL is the url on which paypal sendsback the data.
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority +
                                "/Home/PaymentWithPayPal?";

                    //here we are generating guid for storing the paymentID received in session
                    //which will be used in the payment execution

                    var _guid = Convert.ToString((new Random()).Next(100000));

                    //CreatePayment function gives us the payment approval url
                    //on which payer is redirected for paypal account payment

                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + _guid);

                    //get links returned from paypal in response to Create function call

                    var links = createdPayment.links.GetEnumerator();

                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;

                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    // saving the paymentID in the key guid
                    Session.Add(guid, createdPayment.id);

                    return Redirect(paypalRedirectUrl);
                }
                else
                {

                    // This function exectues after receving all parameters for the payment


                    var executedPayment = ExecutePayment(apiContext, PayerID, paymentId);

                    //If executed payment failed then we will show payment failure message to user
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                SaveError error = new SaveError();
                UserType user = UserCache.GetFromCache(0, ViewBag.IPAddress.StringSafe());
                ErrorType err = new ErrorType
                {
                    Err_Message = ex.Message,
                    Err_Subject = "Attempt to charge to pay pal failed",
                    LiteralDesc = ex.StackTrace,
                    Page = "PaymentWithPayPal",
                    Process = "PaymentWithPaypal",
                    UserID = user.UserID,
                    SQL = "",
                    Time_Of_Error = DateTime.Now
                };
                error.ReportError(err);
                return View("FailureView");
            }

            //on successful payment, show success page to user.
            return View("SuccessView");
        }

        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            return View();
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {

            //create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };

            //Adding Item Details like name, currency, price etc
            itemList.items.Add(new Item()
            {
                name = "Indomitus Est",
                currency = "USD",
                price = "9.95",
                quantity = "1",
                sku = "780979367908"
            });

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            // Adding Tax, shipping and Subtotal details
            var details = new Details()
            {
                tax = ".95",
                shipping = "2.80",
                subtotal = "9.95"
            };

            //Final amount with details
            var amount = new Amount()
            {
                currency = "USD",
                total = "13.70", // Total must be equal to sum of tax, shipping and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = "your generated invoice number", //Generate an Invoice No
                amount = amount,
                item_list = itemList
            });


            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);
        }
    }
}