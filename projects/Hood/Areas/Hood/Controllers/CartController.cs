using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PayPal.Api;
using Hood.Extensions;
using Microsoft.AspNetCore.Http;
using Hood.Services;
using Hood.Models;
using Hood.Models.Api;
using PayPal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Hood.Areas.Hood.Controllers
{
    [Area("Hood")]
    public class CartController : Controller
    {
        private readonly IPayPalService _paypal;
        private readonly IShoppingCart _shop;
        private readonly IContentRepository _data;
        private readonly IAccountRepository _auth;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(IPayPalService paypal, 
                              IShoppingCart shop,
                              UserManager<ApplicationUser> userManager,
                              IContentRepository data,
                              IAccountRepository auth)
        {
            _paypal = paypal;
            _shop = shop;
            _data = data;
            _auth = auth;
            _userManager = userManager;
        }

        #region "Cart"
        public IActionResult MiniCart()
        {
            Cart cart = _shop.Cart;
            return View(cart);
        }

        public IActionResult Cart()
        {
            Cart cart = _shop.Cart;
            return View(cart);
        }

        [HttpPost]
        public IActionResult Add(int productID, int qty)
        {
            try
            {
                var c = _data.GetContentByID(productID);
                Product p = new Product(c);
                _shop.Add(p);
                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        [HttpPost]
        public ActionResult Clear()
        {
            try
            {
                _shop.Clear();
                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        [HttpPost]
        public IActionResult Update(int productID, int qty)
        {
            try
            {
                var c = _data.GetContentByID(productID);
                Product p = new Product(c);
                _shop.Update(p, qty);
                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        [HttpPost]
        public IActionResult Remove(int productID)
        {
            try
            {
                var c = _data.GetContentByID(productID);
                Product p = new Product(c);
                _shop.Remove(p);
                return Json(new Response(true));
            }
            catch (Exception ex)
            {
                return Json(new Response(ex));
            }
        }

        #endregion

        #region "Checkout"

        public enum CheckoutMessageId
        {
            NoItemsError,
            Error,
            PaymentNotApproved,
            PayPalError
        }


        [Authorize]
        [Route("checkout/")]
        public IActionResult Checkout(CheckoutMessageId? message = null)
        {
            CheckoutModel model = new CheckoutModel()
            {
                ShoppingCart = _shop.Cart
            };

            model.Message = message == CheckoutMessageId.NoItemsError ? "There are no items in your cart!"
                : message == CheckoutMessageId.Error ? "There was an error, please try again."
                : message == CheckoutMessageId.PaymentNotApproved ? "We are sorry, your payment could not be approved, please try again."
                : message == CheckoutMessageId.PayPalError ? "There was an error commuincating with paypal, please try again."
                : ""; model.ShowCheckout = model.ShoppingCart.TotalItems > 0;

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [Route("checkout/")]
        public IActionResult Checkout(CheckoutModel model)
        {

            //Address for the payment
            // load the user's billing address - 
            var user = _auth.GetCurrentUser();
            var billingAdd = user.Addresses.Where(a => a.Id == model.BillingAddressId).FirstOrDefault();
            if (billingAdd == null)
            {
                throw new Exception("You need to select a valid billing address.");
            }
            Country country = _data.GetCountry(billingAdd.Country);
            PayPal.Api.Address billingAddress = new PayPal.Api.Address()
            {
                city = billingAdd.City,
                country_code = country.Iso2,
                line1 = billingAdd.Address1,
                postal_code = billingAdd.Postcode,
                state = billingAdd.County
            };

            //Now Create an object of credit card and add above details to it
            CreditCard crdtCard = new CreditCard()
            {
                billing_address = billingAddress,
                cvv2 = model.CVV2,
                expire_month = model.ExpiryMonth,
                expire_year = model.ExpiryYear,
                first_name = model.FirstName,
                last_name = model.LastName,
                number = model.Number.StripSpaces().Trim(),
                type = model.Type
            };
            var guid = Convert.ToString((new Random()).Next(100000));

            Transaction transaction = _paypal.CreateTransactionFromCart(_shop.Cart, guid);

            // Now, we have to make a list of trasaction and add the trasactions object
            // to this list. You can create one or more object as per your requirements

            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(transaction);

            // Now we need to specify the FundingInstrument of the Payer
            // for credit card payments, set the CreditCard which we made above

            FundingInstrument fundInstrument = new FundingInstrument()
            {
                credit_card = crdtCard
            };

            // The Payment creation API requires a list of FundingIntrument

            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);

            // Now create Payer object and assign the fundinginstrument list to the object
            Payer payr = new Payer()
            {
                funding_instruments = fundingInstrumentList,
                payment_method = "credit_card"
            };

            // finally create the payment object and assign the payer object & transaction list to it
            Payment payment = new Payment()
            {
                intent = "sale",
                payer = payr,
                transactions = transactions
            };
            try
            {
                //getting context from the paypal, basically we are sending the clientID and clientSecret key in this function 
                //to the get the context from the paypal API to make the payment for which we have created the object above.

                // Basically, apiContext has a accesstoken which is sent by the paypal to authenticate the payment to facilitator account. An access token could be an alphanumeric string

                APIContext apiContext = _paypal.GetAPIContext();

                // Create is a Payment class function which actually sends the payment details to the paypal API for the payment. The function is passed with the ApiContext which we received above.
                try
                {
                    payment = payment.Create(apiContext);
                }
                catch (PayPalException ex)
                {
                    if (User.IsInRole("Admin"))
                    {
                        var pex = new PayPalPaymentException("Payment Failed!", ex)
                        {
                            Payment = payment,
                            Transaction = transaction
                        };
                        return View("Failure", pex);
                    }
                    throw new Exception("There was an error processing your card payment. Please try again.");
                }
                //if the createdPayment.State is "approved" it means the payment was successfull else not

                if (payment.state.ToLower() != "approved")
                {
                    throw new Exception("Your card payment could not be approved. Please try again.");
                }
                else
                {
                    // payment is approved!! 
                    // create the order, store the details and do all the jazz.
                    return View("Success");
                }

            }
            catch (Exception ex)
            {
                model.Message = ex.Message;
                return View(model);
            }

        }

        [Route("pay/paypal/")]
        public IActionResult PaymentWithPaypal()
        {
            //getting the apiContext as earlier
            APIContext apiContext = _paypal.GetAPIContext();
            Payment payment = new Payment();
            Transaction transaction = new Transaction();
            try
            {
                string payerId = Request.Query["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist
                    //it is returned by the create function call of the payment class

                    // Creating a payment
                    // baseURL is the url on which paypal sendsback the data.
                    // So we have provided URL of this controller only
                    string baseURI = Url.AbsoluteUrl("pay/paypal?");

                    //guid we are generating for storing the paymentID received in session
                    //after calling the create function and it is used in the payment execution
                    var guid = Convert.ToString((new Random()).Next(100000));

                    //CreatePayment function gives us the payment approval url
                    //on which payer is redirected for paypal account payment

                    // Configure Redirect Urls here with RedirectUrls object
                    var redirects = new RedirectUrls()
                    {
                        cancel_url = baseURI + "guid=" + guid,
                        return_url = baseURI + "guid=" + guid
                    };

                    transaction = _paypal.CreateTransactionFromCart(_shop.Cart, guid);

                    payment = _paypal.CreatePayPalPayment(apiContext, redirects, transaction);

                    //get links returned from paypal in response to Create function call

                    var links = payment.links.GetEnumerator();

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
                    HttpContext.Session.SetString(guid, payment.id);

                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This section is executed when we have received all the payments parameters

                    // from the previous call to the function Create

                    // Executing a payment

                    var guid = Request.Query["guid"];
                    string sessionGuid = HttpContext.Session.GetString(guid);

                    try
                    {
                        payment = _paypal.ExecutePayPalPayment(apiContext, payerId, sessionGuid);
                    }
                    catch (PayPalException ex)
                    {
                        if (User.IsInRole("Admin"))
                        {
                            var pex = new PayPalPaymentException("Payment Failed!", ex)
                            {
                                Payment = payment,
                                Transaction = transaction
                            };
                            return View("Failure", pex);
                        }
                        throw new Exception("There was an error processing your PayPal payment. Please try again.");
                    }
                    //if the createdPayment.State is "approved" it means the payment was successfull else not

                    if (payment.state.ToLower() != "approved")
                    {
                        throw new Exception("Your PayPal payment could not be approved. Please try again.");
                    }
                    else
                    {
                        // payment is approved!! 
                        // create the order, store the details and do all the jazz.
                        return View("Success");
                    }

                }
            }
            catch (Exception)
            {
                return RedirectToAction("Checkout", CheckoutMessageId.PayPalError);
            }
        }

        #endregion
    }
}