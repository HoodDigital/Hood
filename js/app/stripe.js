"use strict";

if (!$.hood) $.hood = {};
$.hood.Stripe = {
  Init: function Init() {
    $('body').on('click', '.payment-method-setdefault', $.hood.Stripe.PaymentMethods.SetDefault);
    $('body').on('click', '.payment-method-delete', $.hood.Stripe.PaymentMethods.Delete);
    if ($('#card-add-form').doesExist()) $.hood.Stripe.PaymentMethods.Add();
    if ($('#buy-plan-form').doesExist()) $.hood.Stripe.Subscriptions.Buy();
  },
  Lists: {
    PaymentMethods: {
      Loaded: function Loaded(data) {
        $.hood.Loader(false);
      },
      Reload: function Reload(complete) {
        if ($('#payment-method-list').doesExist()) $.hood.Inline.Reload($('#payment-method-list'), complete);
      }
    }
  },
  PaymentMethods: {
    Stripe: null,
    CardElement: null,
    Add: function Add() {
      $.hood.Stripe.PaymentMethods.Stripe = Stripe($('#card-add-form').data('stripe'));
      var elements = $.hood.Stripe.PaymentMethods.Stripe.elements();
      $.hood.Stripe.PaymentMethods.CardElement = elements.create('card');
      $.hood.Stripe.PaymentMethods.CardElement.mount('#card-add-element');
      $('#card-add-form').validate({
        rules: {
          CardholderName: "required"
        }
      });
      $('body').on('click', '#card-add-submit', $.hood.Stripe.PaymentMethods.SubmitCardForm);
    },
    SubmitCardForm: function SubmitCardForm(e) {
      e.preventDefault();

      if (!$('#card-add-consent').is(":checked")) {
        $('#card-add-errors').html("You cannot add a card without agreeing to the customer agreement.");
        $.hood.Stripe.PaymentMethods.ToggleButton(false);
        return;
      }

      if ($('#card-add-form').valid() && !$('#card-add-submit').hasClass("processing")) {
        $.hood.Stripe.PaymentMethods.ToggleButton(true);
        $.hood.Stripe.PaymentMethods.Stripe.handleCardSetup($('#card-add-submit').data('secret'), $.hood.Stripe.PaymentMethods.CardElement, {
          payment_method_data: {
            billing_details: {
              name: $('#card-add-cardholdername').val()
            }
          }
        }).then(function (result) {
          if (result.error) {
            $('#card-add-errors').html(result.error.message);
            $.hood.Stripe.PaymentMethods.ToggleButton(false);
          } else {
            // Otherwise send paymentMethod.id to your server (see Step 2)
            $.hood.Stripe.PaymentMethods.ConfirmIntent(result);
          }
        });
      }
    },
    HandleServerResponse: function HandleServerResponse(response) {
      if (response.error) {
        $('#card-add-errors').html(response.error.message);
        $.hood.Stripe.PaymentMethods.ToggleButton(false);
      } else if (response.requires_action) {
        // Use Stripe.js to handle required card action
        $.hood.Stripe.PaymentMethods.Stripe.handleCardAction(response.payment_intent_client_secret).then(function (result) {
          if (result.error) {
            $('#card-add-errors').html(result.error.message);
            $.hood.Stripe.PaymentMethods.ToggleButton(false);
          } else {
            // The card action has been handled
            // The PaymentIntent can be confirmed on the server
            $.hood.Stripe.PaymentMethods.ConfirmIntent(result);
          }
        });
      } else {
        // Success, move user on to completed Url, received from the server.
        $('#card-add-errors').html('');
        $('#card-add-success').html("Card added successfully, please wait...");
        $.hood.Stripe.PaymentMethods.ToggleButton(false);
        $.hood.Stripe.Lists.PaymentMethods.Reload();
        $.hood.Inline.CloseModal();
      }
    },
    ConfirmIntent: function ConfirmIntent(result) {
      fetch($('#card-add-submit').data('url'), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          Id: result.setupIntent.id
        })
      }).then(function (confirmResult) {
        return confirmResult.json();
      }).then($.hood.Stripe.PaymentMethods.HandleServerResponse);
    },
    ToggleButton: function ToggleButton(processing) {
      if (processing) {
        $('#card-add-submit').addClass('processing');
        $('#card-add-submit').html('<i class="fa fa-spin fa-sync-alt"></i>&nbsp;Processing...');
      } else {
        $('#card-add-submit').removeClass("processing");
        $('#card-add-submit').html('Add card to wallet');
      }
    },
    SetDefault: function SetDefault(e) {
      e.preventDefault();
      $tag = $(this);

      setDefaultCardCallback = function setDefaultCardCallback(isConfirm) {
        if (isConfirm) {
          $.post($tag.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Stripe.Lists.PaymentMethods.Reload();
          });
        }
      };

      $.hood.Alerts.Confirm("The card will be used for all transactions from this point onwards.", "Are you sure?", setDefaultCardCallback, 'warning', '<span class="text-info"><i class="fa fa-exclamation-triangle"></i> <strong>You can change this at any time.</strong></span>');
    },
    Delete: function Delete(e) {
      e.preventDefault();
      $tag = $(this);

      deleteCardCallback = function deleteCardCallback(isConfirm) {
        if (isConfirm) {
          $.post($tag.attr('href'), function (data) {
            $.hood.Helpers.ProcessResponse(data);
            $.hood.Stripe.Lists.PaymentMethods.Reload();

            if (data.Success) {
              if ($tag && $tag.data('redirect')) {
                $.hood.Alerts.Success("<strong>Card deleted, redirecting...</strong><br />Just taking you back to the list.");
                setTimeout(function () {
                  window.location = $tag.data('redirect');
                }, 1500);
              }
            }
          });
        }
      };

      $.hood.Alerts.Confirm("The card will be permanently removed.", "Are you sure?", deleteCardCallback, 'error', '<span class="text-danger"><i class="fa fa-exclamation-triangle"></i> <strong>This process CANNOT be undone!</strong></span>');
    }
  },
  Subscriptions: {
    Stripe: null,
    CardElement: null,
    Buy: function Buy() {
      $('body').on('change', '.buy-plan-method', $.hood.Stripe.Subscriptions.ToggleNewCardForm);
      $.hood.Stripe.Subscriptions.Stripe = Stripe($('#buy-plan-form').data('stripe'));
      var elements = $.hood.Stripe.Subscriptions.Stripe.elements();
      $.hood.Stripe.Subscriptions.CardElement = elements.create('card');
      $.hood.Stripe.Subscriptions.CardElement.mount('#buy-plan-element');
      $.hood.Stripe.Subscriptions.CardElement.addEventListener('change', function (event) {
        if (event.error) {
          $('#buy-plan-errors').html(event.error.message);
        } else {
          $('#buy-plan-errors').html();
        }
      });
      $('#buy-plan-form').validate({
        rules: {
          CardholderName: "required"
        }
      });
      $('body').on('click', '#buy-plan-submit', $.hood.Stripe.Subscriptions.CompletePurchase);
    },
    CompletePurchase: function CompletePurchase(e) {
      var paymentMethod = $("input[name='buy-plan-method']:checked").val();

      if (!paymentMethod) {
        $('#buy-plan-errors').html("You have to select a payment method to continue with your purchase.");
        $.hood.Stripe.Subscriptions.ToggleButton(false);
        return;
      }

      if (!$('#buy-plan-consent').is(":checked")) {
        $('#buy-plan-errors').html("You cannot purchase a subscription without agreeing to the customer agreement.");
        $.hood.Stripe.Subscriptions.ToggleButton(false);
        return;
      }

      if (paymentMethod === "new-card") {
        // if new card is selected, run the BuyWithNewCard action.
        return $.hood.Stripe.Subscriptions.BuyWithNewCard(e);
      } // we nust have a PM, attempt to process using existing.


      return $.hood.Stripe.Subscriptions.BuyWithExisting(e);
    },
    ToggleNewCardForm: function ToggleNewCardForm(e) {
      var paymentMethod = $("input[name='buy-plan-method']:checked").val();

      if (paymentMethod === "new-card") {
        $('#buy-plan-add-card-form').slideDown();
      } else {
        $('#buy-plan-add-card-form').slideUp();
      }

      $('#buy-plan-errors').html('');
      $('#buy-plan-success').html('');
    },
    BuyWithExisting: function BuyWithExisting(e) {
      e.preventDefault();

      if (!$('#buy-plan-submit').hasClass("processing")) {
        fetch($('#buy-plan-submit').data('url'), {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            PlanId: $('#buy-plan-id').val(),
            PaymentMethodId: $("input[name='buy-plan-method']:checked").val()
          })
        }).then(function (confirmResult) {
          return confirmResult.json();
        }).then($.hood.Stripe.Subscriptions.HandleServerResponse);
      }
    },
    BuyWithNewCard: function BuyWithNewCard(e) {
      e.preventDefault();

      if ($('#buy-plan-form').valid() && !$('#buy-plan-submit').hasClass("processing")) {
        $.hood.Stripe.Subscriptions.ToggleButton(true);
        $.hood.Stripe.Subscriptions.Stripe.createToken($.hood.Stripe.Subscriptions.CardElement).then(function (result) {
          if (result.error) {
            $('#buy-plan-errors').html(result.error.message);
            $.hood.Stripe.Subscriptions.ToggleButton(false);
          } else {
            fetch($('#buy-plan-submit').data('url'), {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify({
                PlanId: $('#buy-plan-id').val(),
                Token: result.token.id
              })
            }).then(function (confirmResult) {
              return confirmResult.json();
            }).then($.hood.Stripe.Subscriptions.HandleServerResponse);
          }
        });
      }
    },
    HandleServerResponse: function HandleServerResponse(response) {
      if (response.error) {
        $('#buy-plan-errors').html(response.error.message);
        $.hood.Stripe.Subscriptions.ToggleButton(false);
      } else if (response.requires_action) {
        // Use Stripe.js to handle required card action
        $.hood.Stripe.Subscriptions.Stripe.handleCardPayment(response.payment_intent_client_secret).then(function (result) {
          if (result.error) {
            $('#buy-plan-errors').html(result.error.message);
            $.hood.Stripe.Subscriptions.ToggleButton(false);
          } else {
            // The card action has been handled
            // The Intent can be confirmed on the server
            $.hood.Stripe.Subscriptions.ConfirmIntent(result);
          }
        });
      } else {
        // Success, move user on to completed Url, received from the server.
        $('#buy-plan-errors').html('');
        $('#buy-plan-success').html("Subscription created successfully, please wait...");
        $.hood.Stripe.Subscriptions.ToggleButton(false);
        if (response.url) window.location = response.url;
      }
    },
    ConfirmIntent: function ConfirmIntent(result) {
      fetch($('#buy-plan-submit').data('url'), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          PlanId: $('#buy-plan-id').val(),
          IntentId: result.paymentIntent.id
        })
      }).then(function (confirmResult) {
        return confirmResult.json();
      }).then($.hood.Stripe.Subscriptions.HandleServerResponse);
    },
    ToggleButton: function ToggleButton(processing) {
      if (processing) {
        $('#buy-plan-submit').addClass('processing');
        $('#buy-plan-submit').html('<i class="fa fa-spin fa-sync-alt"></i>&nbsp;Processing...');
      } else {
        $('#buy-plan-submit').removeClass("processing");
        $('#buy-plan-submit').html('Confirm &amp; Pay');
      }
    }
  }
};
$.hood.Stripe.Init();